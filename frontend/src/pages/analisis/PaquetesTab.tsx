import { useState, useEffect, useCallback } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './AnalisisPage.module.css'

interface ItemAnalisis {
  idAnalisis: number
  codigo: string
  nombre: string
  area: string
  precioUsd: number
}

interface Paquete {
  idPaquete: number
  nombre: string
  area: string
  tipoMuestra: string | null
  norma: string | null
  cantidadMuestra: string | null
  precioUsd: number
  activo: boolean
  analisis: ItemAnalisis[]
}

interface AnalisisCatalogo {
  idAnalisis: number
  codigo: string
  nombre: string
  area: string
  precioUsd: number
  activo: boolean
}

type FiltroArea = 'todas' | 'MIC' | 'FQ'

export default function PaquetesTab() {
  const [paquetes, setPaquetes] = useState<Paquete[]>([])
  const [catalogo, setCatalogo] = useState<AnalisisCatalogo[]>([])
  const [cargando, setCargando] = useState(true)
  const [errorCarga, setErrorCarga] = useState('')
  const [filtroArea, setFiltroArea] = useState<FiltroArea>('todas')

  const [modal, setModal] = useState<{ tipo: 'crear' | 'editar'; item?: Paquete } | null>(null)
  const [confirmEliminar, setConfirmEliminar] = useState<Paquete | null>(null)
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  const [form, setForm] = useState({
    nombre: '', area: 'MIC', tipoMuestra: '', norma: '', cantidadMuestra: '', precioUsd: '',
  })
  const [idsSeleccionados, setIdsSeleccionados] = useState<number[]>([])

  const cargar = useCallback(async () => {
    setCargando(true)
    setErrorCarga('')
    try {
      const [resP, resA] = await Promise.all([
        apiFetch('/api/analisis/paquetes/todos'),
        apiFetch('/api/analisis/todos'),
      ])
      if (!resP.ok || !resA.ok) {
        setPaquetes([])
        setCatalogo([])
        setErrorCarga(`No se pudieron cargar los paquetes (error ${resP.ok ? resA.status : resP.status}).`)
        return
      }
      setPaquetes(await resP.json())
      setCatalogo(await resA.json())
    } catch {
      setPaquetes([])
      setCatalogo([])
      setErrorCarga('No se pudo conectar con el servidor.')
    } finally {
      setCargando(false)
    }
  }, [])

  useEffect(() => { cargar() }, [cargar])

  const listaFiltrada = filtroArea === 'todas' ? paquetes : paquetes.filter(p => p.area === filtroArea)
  const catalogoDelArea = catalogo.filter(a => a.activo && a.area === form.area)

  function abrirCrear() {
    setForm({ nombre: '', area: 'MIC', tipoMuestra: '', norma: '', cantidadMuestra: '', precioUsd: '' })
    setIdsSeleccionados([])
    setError('')
    setModal({ tipo: 'crear' })
  }

  function abrirEditar(p: Paquete) {
    setForm({
      nombre: p.nombre,
      area: p.area,
      tipoMuestra: p.tipoMuestra ?? '',
      norma: p.norma ?? '',
      cantidadMuestra: p.cantidadMuestra ?? '',
      precioUsd: String(p.precioUsd),
    })
    setIdsSeleccionados(p.analisis.map(a => a.idAnalisis))
    setError('')
    setModal({ tipo: 'editar', item: p })
  }

  function toggleSeleccion(id: number) {
    setIdsSeleccionados(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id])
  }

  async function guardar() {
    setError('')
    if (!form.nombre.trim() || !form.precioUsd) {
      setError('Completá nombre y precio.')
      return
    }
    if (idsSeleccionados.length === 0) {
      setError('Seleccioná al menos un análisis para incluir en el paquete.')
      return
    }
    setGuardando(true)
    try {
      const body = {
        nombre: form.nombre.trim(),
        area: form.area,
        tipoMuestra: form.tipoMuestra.trim() || null,
        norma: form.norma.trim() || null,
        cantidadMuestra: form.cantidadMuestra.trim() || null,
        precioUsd: parseFloat(form.precioUsd),
        idsAnalisis: idsSeleccionados,
      }
      const isEditar = modal?.tipo === 'editar'
      const res = await apiFetch(
        isEditar ? `/api/analisis/paquetes/${modal!.item!.idPaquete}` : '/api/analisis/paquetes',
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

  async function toggleActivo(p: Paquete) {
    await apiFetch(`/api/analisis/paquetes/${p.idPaquete}/activo`, { method: 'PATCH' })
    await cargar()
  }

  async function eliminar(p: Paquete) {
    const res = await apiFetch(`/api/analisis/paquetes/${p.idPaquete}`, { method: 'DELETE' })
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
          <h1 className={styles.titulo}>Paquetes</h1>
          <p className={styles.subtitulo}>
            Combos de análisis vendidos como unidad, con precio propio bonificado — se reutilizan en cualquier presupuesto.
          </p>
        </div>
        <button className={styles.btnPrimario} onClick={abrirCrear}>+ Nuevo paquete</button>
      </div>

      <div className={styles.controles}>
        <div className={styles.tabs}>
          {(['todas', 'MIC', 'FQ'] as FiltroArea[]).map(a => (
            <button
              key={a}
              className={`${styles.tab} ${filtroArea === a ? styles.tabActivo : ''}`}
              onClick={() => setFiltroArea(a)}
            >
              {a === 'todas' ? 'Todas las áreas' : a}
            </button>
          ))}
        </div>
      </div>

      {errorCarga && <p className={styles.errorModal}>{errorCarga}</p>}

      {cargando ? (
        <div className={styles.cargando}>Cargando…</div>
      ) : listaFiltrada.length === 0 ? (
        <div className={styles.vacio}>Todavía no hay paquetes cargados.</div>
      ) : (
        <div className={styles.tablaWrap}>
          <table className={styles.tabla}>
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Área</th>
                <th>Tipo de muestra</th>
                <th>Análisis incluidos</th>
                <th>Precio USD</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {listaFiltrada.map(p => (
                <tr key={p.idPaquete} className={p.activo ? '' : styles.filaInactiva}>
                  <td>
                    {p.nombre}
                    {p.norma && <div className={styles.tdReferencia}>{p.norma}</div>}
                  </td>
                  <td>
                    <span className={`${styles.areaTag} ${p.area === 'MIC' ? styles.areaMic : styles.areaFq}`}>
                      {p.area}
                    </span>
                  </td>
                  <td className={styles.tdGrupo}>{p.tipoMuestra ?? <span className={styles.sinDato}>—</span>}</td>
                  <td className={styles.tdReferencia} title={p.analisis.map(a => a.nombre).join(', ')}>
                    {p.analisis.length} análisis
                  </td>
                  <td className={styles.tdPrecio}>USD {Number(p.precioUsd).toFixed(2)}</td>
                  <td>
                    <button
                      className={`${styles.btnEstado} ${p.activo ? styles.btnEstadoActivo : styles.btnEstadoInactivo}`}
                      onClick={() => toggleActivo(p)}
                      title={p.activo ? 'Clic para desactivar' : 'Clic para activar'}
                    >
                      {p.activo ? 'Activo' : 'Inactivo'}
                    </button>
                  </td>
                  <td className={styles.tdAcciones}>
                    <button className={styles.btnEditar} onClick={() => abrirEditar(p)}>Editar</button>
                    <button className={styles.btnEliminar} onClick={() => setConfirmEliminar(p)}>Eliminar</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {modal && (
        <div className={styles.overlay}>
          <div className={`${styles.modal} ${styles.modalGrande}`}>
            <div className={styles.modalHeader}>
              <h2 className={styles.modalTitulo}>{modal.tipo === 'crear' ? 'Nuevo paquete' : 'Editar paquete'}</h2>
              <button className={styles.btnCerrar} onClick={() => setModal(null)}>✕</button>
            </div>
            <div className={styles.modalBody}>
              <div className={styles.campo}>
                <label>Área *</label>
                <div className={styles.radioGroup}>
                  {['MIC', 'FQ'].map(area => (
                    <label key={area} className={`${styles.radioOpt} ${form.area === area ? styles.radioActivo : ''}`}>
                      <input type="radio" checked={form.area === area} onChange={() => { setForm(f => ({ ...f, area })); setIdsSeleccionados([]) }} />
                      {area}
                    </label>
                  ))}
                </div>
              </div>

              <div className={styles.campo}>
                <label>Nombre *</label>
                <input
                  placeholder="Ej: Determinaciones microbiológicas en agua potable"
                  value={form.nombre}
                  onChange={e => setForm(f => ({ ...f, nombre: e.target.value }))}
                />
              </div>

              <div className={styles.fila2}>
                <div className={styles.campo}>
                  <label>Tipo de muestra</label>
                  <input
                    placeholder="Ej: Agua, Alimentos"
                    value={form.tipoMuestra}
                    onChange={e => setForm(f => ({ ...f, tipoMuestra: e.target.value }))}
                  />
                </div>
                <div className={styles.campo}>
                  <label>Precio del paquete (USD) *</label>
                  <input
                    type="number" min="0" step="0.01" placeholder="0.00"
                    value={form.precioUsd}
                    onChange={e => setForm(f => ({ ...f, precioUsd: e.target.value }))}
                  />
                </div>
              </div>

              <div className={styles.campo}>
                <label>Norma / referencia</label>
                <input
                  placeholder="Ej: C.A.A. art. N° 982"
                  value={form.norma}
                  onChange={e => setForm(f => ({ ...f, norma: e.target.value }))}
                />
              </div>

              <div className={styles.campo}>
                <label>Cantidad de muestra requerida</label>
                <input
                  placeholder="Ej: 400mL en envase estéril"
                  value={form.cantidadMuestra}
                  onChange={e => setForm(f => ({ ...f, cantidadMuestra: e.target.value }))}
                />
              </div>

              <div className={styles.campo}>
                <label>Análisis incluidos * ({idsSeleccionados.length} seleccionados)</label>
                {catalogoDelArea.length === 0 ? (
                  <p className={styles.sinDato}>No hay análisis activos cargados en el área {form.area}.</p>
                ) : (
                  <div className={styles.radioGroup} style={{ flexWrap: 'wrap', flexDirection: 'column', alignItems: 'flex-start', gap: 6 }}>
                    {catalogoDelArea.map(a => (
                      <label key={a.idAnalisis} className={styles.checkboxLabel}>
                        <input
                          type="checkbox"
                          checked={idsSeleccionados.includes(a.idAnalisis)}
                          onChange={() => toggleSeleccion(a.idAnalisis)}
                        />
                        {a.codigo} — {a.nombre} (USD {a.precioUsd.toFixed(2)})
                      </label>
                    ))}
                  </div>
                )}
              </div>

              {error && <p className={styles.errorModal}>{error}</p>}
            </div>
            <div className={styles.modalFooter}>
              <button className={styles.btnSecundario} onClick={() => setModal(null)}>Cancelar</button>
              <button className={styles.btnPrimario} onClick={guardar} disabled={guardando}>
                {guardando ? 'Guardando...' : modal.tipo === 'crear' ? 'Crear paquete' : 'Guardar cambios'}
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
              <p className={styles.confirmTexto}>¿Eliminar el paquete <strong>{confirmEliminar.nombre}</strong>?</p>
              <p className={styles.confirmNota}>
                Solo se puede eliminar si no está incluido en ningún presupuesto. Si ya fue usado, desactivalo en su lugar.
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
