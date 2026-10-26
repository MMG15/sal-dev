import { useState, useEffect, useCallback } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './AnalisisPage.module.css'

interface Metodo {
  idMetodo: number
  nombre: string
  descripcion: string | null
  area: string
  activo: boolean
}

type FiltroArea = 'todas' | 'MIC' | 'FQ' | 'AMBAS'

const AREA_LABEL: Record<string, string> = { MIC: 'MIC', FQ: 'FQ', AMBAS: 'Ambas' }

export default function MetodosTab() {
  const [metodos, setMetodos] = useState<Metodo[]>([])
  const [cargando, setCargando] = useState(true)
  const [errorCarga, setErrorCarga] = useState('')
  const [filtroArea, setFiltroArea] = useState<FiltroArea>('todas')

  const [modal, setModal] = useState<{ tipo: 'crear' | 'editar'; item?: Metodo } | null>(null)
  const [confirmEliminar, setConfirmEliminar] = useState<Metodo | null>(null)
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  const [form, setForm] = useState({ nombre: '', descripcion: '', area: 'MIC' })

  const cargar = useCallback(async () => {
    setCargando(true)
    setErrorCarga('')
    try {
      const res = await apiFetch('/api/analisis/metodos/todos')
      if (!res.ok) {
        setMetodos([])
        setErrorCarga(`No se pudieron cargar los métodos (error ${res.status}).`)
        return
      }
      setMetodos(await res.json())
    } catch {
      setMetodos([])
      setErrorCarga('No se pudo conectar con el servidor.')
    } finally {
      setCargando(false)
    }
  }, [])

  useEffect(() => { cargar() }, [cargar])

  const listaFiltrada = filtroArea === 'todas' ? metodos : metodos.filter(m => m.area === filtroArea)

  function abrirCrear() {
    setForm({ nombre: '', descripcion: '', area: 'MIC' })
    setError('')
    setModal({ tipo: 'crear' })
  }

  function abrirEditar(m: Metodo) {
    setForm({ nombre: m.nombre, descripcion: m.descripcion ?? '', area: m.area })
    setError('')
    setModal({ tipo: 'editar', item: m })
  }

  async function guardar() {
    setError('')
    if (!form.nombre.trim()) {
      setError('El nombre del método es obligatorio.')
      return
    }
    setGuardando(true)
    try {
      const body = {
        nombre: form.nombre.trim(),
        descripcion: form.descripcion.trim() || null,
        area: form.area,
      }
      const isEditar = modal?.tipo === 'editar'
      const res = await apiFetch(
        isEditar ? `/api/analisis/metodos/${modal!.item!.idMetodo}` : '/api/analisis/metodos',
        { method: isEditar ? 'PUT' : 'POST', body: JSON.stringify(body) }
      )
      if (!res.ok) {
        const data = await res.json().catch(() => ({}))
        setError(data.error ?? 'Error al guardar.')
        return
      }
      setModal(null)
      await cargar()
    } finally {
      setGuardando(false)
    }
  }

  async function toggleActivo(m: Metodo) {
    await apiFetch(`/api/analisis/metodos/${m.idMetodo}/activo`, { method: 'PATCH' })
    await cargar()
  }

  async function eliminar(m: Metodo) {
    const res = await apiFetch(`/api/analisis/metodos/${m.idMetodo}`, { method: 'DELETE' })
    if (!res.ok) {
      const data = await res.json().catch(() => ({}))
      alert(data.error ?? 'No se pudo eliminar.')
      return
    }
    setConfirmEliminar(null)
    await cargar()
  }

  return (
    <div>
      <div className={styles.cabecera}>
        <div>
          <h1 className={styles.titulo}>Métodos</h1>
          <p className={styles.subtitulo}>
            Norma, equipo y condiciones con las que se hace un análisis — se cargan una vez y se referencian desde el catálogo y desde los presupuestos.
          </p>
        </div>
        <button className={styles.btnPrimario} onClick={abrirCrear}>+ Nuevo método</button>
      </div>

      <div className={styles.controles}>
        <div className={styles.tabs}>
          {(['todas', 'MIC', 'FQ', 'AMBAS'] as FiltroArea[]).map(a => (
            <button
              key={a}
              className={`${styles.tab} ${filtroArea === a ? styles.tabActivo : ''}`}
              onClick={() => setFiltroArea(a)}
            >
              {a === 'todas' ? 'Todas las áreas' : AREA_LABEL[a]}
            </button>
          ))}
        </div>
      </div>

      {errorCarga && <p className={styles.errorModal}>{errorCarga}</p>}

      {cargando ? (
        <div className={styles.cargando}>Cargando…</div>
      ) : listaFiltrada.length === 0 ? (
        <div className={styles.vacio}>Todavía no hay métodos cargados.</div>
      ) : (
        <div className={styles.tablaWrap}>
          <table className={styles.tabla}>
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Área</th>
                <th>Descripción</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {listaFiltrada.map(m => (
                <tr key={m.idMetodo} className={m.activo ? '' : styles.filaInactiva}>
                  <td>{m.nombre}</td>
                  <td>
                    <span className={`${styles.areaTag} ${m.area === 'MIC' ? styles.areaMic : m.area === 'FQ' ? styles.areaFq : ''}`}>
                      {AREA_LABEL[m.area] ?? m.area}
                    </span>
                  </td>
                  <td className={styles.tdReferencia}>{m.descripcion ?? <span className={styles.sinDato}>—</span>}</td>
                  <td>
                    <button
                      className={`${styles.btnEstado} ${m.activo ? styles.btnEstadoActivo : styles.btnEstadoInactivo}`}
                      onClick={() => toggleActivo(m)}
                      title={m.activo ? 'Clic para desactivar' : 'Clic para activar'}
                    >
                      {m.activo ? 'Activo' : 'Inactivo'}
                    </button>
                  </td>
                  <td className={styles.tdAcciones}>
                    <button className={styles.btnEditar} onClick={() => abrirEditar(m)}>Editar</button>
                    <button className={styles.btnEliminar} onClick={() => setConfirmEliminar(m)}>Eliminar</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {modal && (
        <div className={styles.overlay}>
          <div className={styles.modal}>
            <div className={styles.modalHeader}>
              <h2 className={styles.modalTitulo}>{modal.tipo === 'crear' ? 'Nuevo método' : 'Editar método'}</h2>
              <button className={styles.btnCerrar} onClick={() => setModal(null)}>✕</button>
            </div>
            <div className={styles.modalBody}>
              <div className={styles.campo}>
                <label>Nombre *</label>
                <input
                  placeholder="Ej: Método APHA 9215B"
                  value={form.nombre}
                  onChange={e => setForm(f => ({ ...f, nombre: e.target.value }))}
                />
              </div>

              <div className={styles.campo}>
                <label>Área *</label>
                <div className={styles.radioGroup}>
                  {['MIC', 'FQ', 'AMBAS'].map(area => (
                    <label key={area} className={`${styles.radioOpt} ${form.area === area ? styles.radioActivo : ''}`}>
                      <input type="radio" checked={form.area === area} onChange={() => setForm(f => ({ ...f, area }))} />
                      {AREA_LABEL[area]}
                    </label>
                  ))}
                </div>
              </div>

              <div className={styles.campo}>
                <label>Descripción (norma, equipo, sensibilidad, condiciones)</label>
                <textarea
                  placeholder="Ej: Norma + equipo + sensibilidad + rango + condiciones especiales"
                  value={form.descripcion}
                  onChange={e => setForm(f => ({ ...f, descripcion: e.target.value }))}
                  rows={4}
                />
              </div>

              {error && <p className={styles.errorModal}>{error}</p>}
            </div>
            <div className={styles.modalFooter}>
              <button className={styles.btnSecundario} onClick={() => setModal(null)}>Cancelar</button>
              <button className={styles.btnPrimario} onClick={guardar} disabled={guardando}>
                {guardando ? 'Guardando...' : modal.tipo === 'crear' ? 'Crear método' : 'Guardar cambios'}
              </button>
            </div>
          </div>
        </div>
      )}

      {confirmEliminar && (
        <div className={styles.overlay}>
          <div className={`${styles.modal} ${styles.modalChico}`}>
            <div className={styles.modalHeader}>
              <h2 className={styles.modalTitulo}>Confirmar eliminación</h2>
              <button className={styles.btnCerrar} onClick={() => setConfirmEliminar(null)}>✕</button>
            </div>
            <div className={styles.modalBody}>
              <p className={styles.confirmTexto}>¿Eliminar el método <strong>{confirmEliminar.nombre}</strong>?</p>
              <p className={styles.confirmNota}>
                Solo se puede eliminar si no está en uso en ningún análisis o presupuesto. Si ya fue usado, desactivalo en su lugar.
              </p>
            </div>
            <div className={styles.modalFooter}>
              <button className={styles.btnSecundario} onClick={() => setConfirmEliminar(null)}>Cancelar</button>
              <button className={styles.btnEliminarDefinitivo} onClick={() => eliminar(confirmEliminar)}>
                Eliminar definitivamente
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
