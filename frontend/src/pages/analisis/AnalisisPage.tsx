import { useEffect, useState, useCallback, useMemo } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './AnalisisPage.module.css'

interface AnalisisItem {
  idAnalisis: number
  codigo: string
  nombre: string
  area: string
  precioUsd: number
  activo: boolean
  grupo: { idGrupo: number; codigoAgrupador: string; area: string }
}

interface GrupoOpcion {
  idGrupo: number
  codigoAgrupador: string
  area: string
}

type FiltroArea = 'todas' | 'MIC' | 'FQ'
type FiltroEstado = 'todos' | 'activos' | 'inactivos'

export default function AnalisisPage() {
  const [analisis, setAnalisis] = useState<AnalisisItem[]>([])
  const [grupos, setGrupos] = useState<GrupoOpcion[]>([])
  const [cargando, setCargando] = useState(true)
  const [busqueda, setBusqueda] = useState('')
  const [filtroArea, setFiltroArea] = useState<FiltroArea>('todas')
  const [filtroEstado, setFiltroEstado] = useState<FiltroEstado>('todos')

  const [modal, setModal] = useState<{ tipo: 'crear' | 'editar'; item?: AnalisisItem } | null>(null)
  const [confirmEliminar, setConfirmEliminar] = useState<AnalisisItem | null>(null)
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  const [form, setForm] = useState({
    idGrupo: 0,
    nuevoGrupo: '',
    codigo: '',
    nombre: '',
    area: 'MIC',
    precioUsd: '',
  })

  const cargar = useCallback(async () => {
    setCargando(true)
    try {
      const [resA, resG] = await Promise.all([
        apiFetch('/api/analisis/todos'),
        apiFetch('/api/analisis/grupos'),
      ])
      setAnalisis(await resA.json())
      setGrupos(await resG.json())
    } finally {
      setCargando(false)
    }
  }, [])

  useEffect(() => { cargar() }, [cargar])

  const listaFiltrada = useMemo(() => {
    const texto = busqueda.trim().toLowerCase()
    return analisis.filter(a => {
      if (filtroArea !== 'todas' && a.area !== filtroArea) return false
      if (filtroEstado === 'activos' && !a.activo) return false
      if (filtroEstado === 'inactivos' && a.activo) return false
      if (texto && !`${a.codigo} ${a.nombre} ${a.grupo.codigoAgrupador}`.toLowerCase().includes(texto)) return false
      return true
    })
  }, [analisis, busqueda, filtroArea, filtroEstado])

  function abrirCrear() {
    setForm({ idGrupo: 0, nuevoGrupo: '', codigo: '', nombre: '', area: 'MIC', precioUsd: '' })
    setError('')
    setModal({ tipo: 'crear' })
  }

  function abrirEditar(item: AnalisisItem) {
    setForm({
      idGrupo: item.grupo.idGrupo,
      nuevoGrupo: '',
      codigo: item.codigo,
      nombre: item.nombre,
      area: item.area,
      precioUsd: String(item.precioUsd),
    })
    setError('')
    setModal({ tipo: 'editar', item })
  }

  async function guardar() {
    setError('')
    if (!form.codigo.trim() || !form.nombre.trim() || !form.precioUsd) {
      setError('Completá todos los campos obligatorios.')
      return
    }
    if (form.idGrupo === 0 && !form.nuevoGrupo.trim()) {
      setError('Seleccioná un grupo o escribí el nombre del nuevo grupo.')
      return
    }
    setGuardando(true)
    try {
      const body = {
        idGrupo: form.idGrupo,
        nuevoGrupo: form.nuevoGrupo || null,
        codigo: form.codigo,
        nombre: form.nombre,
        area: form.area,
        precioUsd: parseFloat(form.precioUsd),
      }
      const isEditar = modal?.tipo === 'editar'
      const res = await apiFetch(
        isEditar ? `/api/analisis/${modal!.item!.idAnalisis}` : '/api/analisis',
        { method: isEditar ? 'PUT' : 'POST', body: JSON.stringify(body) }
      )
      if (!res.ok) {
        const data = await res.json()
        setError(data.error ?? 'Error al guardar.')
        return
      }
      setModal(null)
      await cargar()
    } finally {
      setGuardando(false)
    }
  }

  async function toggleActivo(item: AnalisisItem) {
    await apiFetch(`/api/analisis/${item.idAnalisis}/activo`, { method: 'PATCH' })
    await cargar()
  }

  async function eliminar(item: AnalisisItem) {
    const res = await apiFetch(`/api/analisis/${item.idAnalisis}`, { method: 'DELETE' })
    if (!res.ok) {
      const data = await res.json()
      alert(data.error ?? 'No se pudo eliminar.')
      return
    }
    setConfirmEliminar(null)
    await cargar()
  }

  const gruposFiltrados = grupos.filter(g => g.area === form.area)
  const totalActivos = analisis.filter(a => a.activo).length

  return (
    <div>
      <div className={styles.cabecera}>
        <div>
          <h1 className={styles.titulo}>Catálogo de análisis</h1>
          <p className={styles.subtitulo}>
            {cargando ? 'Cargando…' : `${totalActivos} activos de ${analisis.length} en total`}
          </p>
        </div>
        <button className={styles.btnPrimario} onClick={abrirCrear}>+ Nuevo análisis</button>
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
        <input
          className={styles.buscador}
          placeholder="Buscar por código, nombre o grupo..."
          value={busqueda}
          onChange={e => setBusqueda(e.target.value)}
        />
        <select
          className={styles.selectEstado}
          value={filtroEstado}
          onChange={e => setFiltroEstado(e.target.value as FiltroEstado)}
        >
          <option value="todos">Todos los estados</option>
          <option value="activos">Solo activos</option>
          <option value="inactivos">Solo inactivos</option>
        </select>
      </div>

      {cargando ? (
        <div className={styles.cargando}>Cargando...</div>
      ) : listaFiltrada.length === 0 ? (
        <div className={styles.vacio}>
          {busqueda || filtroArea !== 'todas' || filtroEstado !== 'todos'
            ? 'Sin resultados para ese filtro.'
            : 'Todavía no hay análisis cargados.'}
        </div>
      ) : (
        <div className={styles.tablaWrap}>
          <table className={styles.tabla}>
            <thead>
              <tr>
                <th>Código</th>
                <th>Nombre</th>
                <th>Área</th>
                <th>Grupo</th>
                <th>Precio USD</th>
                <th>Estado</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {listaFiltrada.map(a => (
                <tr key={a.idAnalisis} className={a.activo ? '' : styles.filaInactiva}>
                  <td className={styles.tdCodigo}>{a.codigo}</td>
                  <td>{a.nombre}</td>
                  <td>
                    <span className={`${styles.areaTag} ${a.area === 'MIC' ? styles.areaMic : styles.areaFq}`}>
                      {a.area}
                    </span>
                  </td>
                  <td className={styles.tdGrupo}>{a.grupo.codigoAgrupador}</td>
                  <td className={styles.tdPrecio}>USD {Number(a.precioUsd).toFixed(2)}</td>
                  <td>
                    <button
                      className={`${styles.btnEstado} ${a.activo ? styles.btnEstadoActivo : styles.btnEstadoInactivo}`}
                      onClick={() => toggleActivo(a)}
                      title={a.activo ? 'Clic para desactivar' : 'Clic para activar'}
                    >
                      {a.activo ? 'Activo' : 'Inactivo'}
                    </button>
                  </td>
                  <td className={styles.tdAcciones}>
                    <button className={styles.btnEditar} onClick={() => abrirEditar(a)}>Editar</button>
                    <button className={styles.btnEliminar} onClick={() => setConfirmEliminar(a)}>Eliminar</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Modal crear / editar */}
      {modal && (
        <div className={styles.overlay}>
          <div className={styles.modal}>
            <div className={styles.modalHeader}>
              <h2 className={styles.modalTitulo}>{modal.tipo === 'crear' ? 'Nuevo análisis' : 'Editar análisis'}</h2>
              <button className={styles.btnCerrar} onClick={() => setModal(null)}>✕</button>
            </div>
            <div className={styles.modalBody}>
              <div className={styles.campo}>
                <label>Área *</label>
                <div className={styles.radioGroup}>
                  {['MIC', 'FQ'].map(area => (
                    <label key={area} className={`${styles.radioOpt} ${form.area === area ? styles.radioActivo : ''}`}>
                      <input type="radio" checked={form.area === area} onChange={() => setForm(f => ({ ...f, area, idGrupo: 0 }))} />
                      {area}
                    </label>
                  ))}
                </div>
              </div>

              <div className={styles.campo}>
                <label>Grupo *</label>
                <select
                  value={form.idGrupo}
                  onChange={e => setForm(f => ({ ...f, idGrupo: Number(e.target.value), nuevoGrupo: '' }))}
                >
                  <option value={0}>— Nuevo grupo —</option>
                  {gruposFiltrados.map(g => (
                    <option key={g.idGrupo} value={g.idGrupo}>{g.codigoAgrupador}</option>
                  ))}
                </select>
              </div>

              {form.idGrupo === 0 && (
                <div className={styles.campo}>
                  <label>Nombre del nuevo grupo *</label>
                  <input
                    placeholder="Ej: Vitaminas"
                    value={form.nuevoGrupo}
                    onChange={e => setForm(f => ({ ...f, nuevoGrupo: e.target.value }))}
                  />
                </div>
              )}

              <div className={styles.campo}>
                <label>Código *</label>
                <input
                  placeholder="Ej: MIC-001"
                  value={form.codigo}
                  onChange={e => setForm(f => ({ ...f, codigo: e.target.value }))}
                />
              </div>

              <div className={styles.campo}>
                <label>Nombre *</label>
                <input
                  placeholder="Ej: Recuento total en placa"
                  value={form.nombre}
                  onChange={e => setForm(f => ({ ...f, nombre: e.target.value }))}
                />
              </div>

              <div className={styles.campo}>
                <label>Precio USD *</label>
                <input
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder="0.00"
                  value={form.precioUsd}
                  onChange={e => setForm(f => ({ ...f, precioUsd: e.target.value }))}
                />
              </div>

              {error && <p className={styles.errorModal}>{error}</p>}
            </div>
            <div className={styles.modalFooter}>
              <button className={styles.btnSecundario} onClick={() => setModal(null)}>Cancelar</button>
              <button className={styles.btnPrimario} onClick={guardar} disabled={guardando}>
                {guardando ? 'Guardando...' : modal.tipo === 'crear' ? 'Crear análisis' : 'Guardar cambios'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Confirmar eliminación */}
      {confirmEliminar && (
        <div className={styles.overlay}>
          <div className={`${styles.modal} ${styles.modalChico}`}>
            <div className={styles.modalHeader}>
              <h2 className={styles.modalTitulo}>Confirmar eliminación</h2>
              <button className={styles.btnCerrar} onClick={() => setConfirmEliminar(null)}>✕</button>
            </div>
            <div className={styles.modalBody}>
              <p className={styles.confirmTexto}>
                ¿Eliminar el análisis <strong>{confirmEliminar.codigo} — {confirmEliminar.nombre}</strong>?
              </p>
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
