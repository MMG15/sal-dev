import { useState, useEffect, useCallback } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './AnalisisPage.module.css'

interface Grupo {
  idGrupo: number
  codigoAgrupador: string
  descripcion: string | null
  area: string
  tipoMuestra: string | null
  norma: string | null
  cantidadMuestra: string | null
}

export default function GruposTab() {
  const [grupos, setGrupos] = useState<Grupo[]>([])
  const [cargando, setCargando] = useState(true)
  const [errorCarga, setErrorCarga] = useState('')

  const [editando, setEditando] = useState<Grupo | null>(null)
  const [form, setForm] = useState({ tipoMuestra: '', norma: '', cantidadMuestra: '' })
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  const [creando, setCreando] = useState(false)
  const [formNueva, setFormNueva] = useState({ codigoAgrupador: '', area: 'MIC', tipoMuestra: '', norma: '', cantidadMuestra: '' })
  const [guardandoNueva, setGuardandoNueva] = useState(false)
  const [errorNueva, setErrorNueva] = useState('')

  const cargar = useCallback(async () => {
    setCargando(true)
    setErrorCarga('')
    try {
      const res = await apiFetch('/api/analisis/grupos')
      if (!res.ok) {
        setGrupos([])
        setErrorCarga(`No se pudieron cargar las categorías (error ${res.status}).`)
        return
      }
      setGrupos(await res.json())
    } catch {
      setGrupos([])
      setErrorCarga('No se pudo conectar con el servidor.')
    } finally {
      setCargando(false)
    }
  }, [])

  useEffect(() => { cargar() }, [cargar])

  function abrirEditar(g: Grupo) {
    setForm({ tipoMuestra: g.tipoMuestra ?? '', norma: g.norma ?? '', cantidadMuestra: g.cantidadMuestra ?? '' })
    setError('')
    setEditando(g)
  }

  function abrirNueva() {
    setFormNueva({ codigoAgrupador: '', area: 'MIC', tipoMuestra: '', norma: '', cantidadMuestra: '' })
    setErrorNueva('')
    setCreando(true)
  }

  async function guardarNueva() {
    if (!formNueva.codigoAgrupador.trim()) { setErrorNueva('El nombre de la categoría es obligatorio.'); return }
    setErrorNueva('')
    setGuardandoNueva(true)
    try {
      const res = await apiFetch('/api/analisis/grupos', {
        method: 'POST',
        body: JSON.stringify({
          codigoAgrupador: formNueva.codigoAgrupador.trim(),
          area: formNueva.area,
          tipoMuestra: formNueva.tipoMuestra.trim() || null,
          norma: formNueva.norma.trim() || null,
          cantidadMuestra: formNueva.cantidadMuestra.trim() || null,
        }),
      })
      if (!res.ok) {
        const data = await res.json().catch(() => ({}))
        setErrorNueva(data.error ?? 'Error al crear la categoría.')
        return
      }
      setCreando(false)
      await cargar()
    } finally {
      setGuardandoNueva(false)
    }
  }

  async function guardar() {
    if (!editando) return
    setError('')
    setGuardando(true)
    try {
      const res = await apiFetch(`/api/analisis/grupos/${editando.idGrupo}`, {
        method: 'PUT',
        body: JSON.stringify({
          tipoMuestra: form.tipoMuestra.trim() || null,
          norma: form.norma.trim() || null,
          cantidadMuestra: form.cantidadMuestra.trim() || null,
        }),
      })
      if (!res.ok) {
        const data = await res.json().catch(() => ({}))
        setError(data.error ?? 'Error al guardar.')
        return
      }
      setEditando(null)
      await cargar()
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div>
      <div className={styles.cabecera}>
        <div>
          <h1 className={styles.titulo}>Categorías de análisis</h1>
          <p className={styles.subtitulo}>
            Agrupan análisis relacionados (ej. "Microbiológicos de agua"). Acá se completa qué norma rigen, el tipo de muestra y la cantidad requerida.
          </p>
        </div>
        <button className={styles.btnPrimario} onClick={abrirNueva}>+ Nueva categoría</button>
      </div>

      {errorCarga && <p className={styles.errorModal}>{errorCarga}</p>}

      {cargando ? (
        <div className={styles.cargando}>Cargando…</div>
      ) : grupos.length === 0 ? (
        <div className={styles.vacio}>Todavía no hay categorías cargadas.</div>
      ) : (
        <div className={styles.tablaWrap}>
          <table className={styles.tabla}>
            <thead>
              <tr>
                <th>Categoría</th>
                <th>Área</th>
                <th>Tipo de muestra</th>
                <th>Norma</th>
                <th>Cantidad de muestra</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {grupos.map(g => (
                <tr key={g.idGrupo}>
                  <td>{g.codigoAgrupador}</td>
                  <td>
                    <span className={`${styles.areaTag} ${g.area === 'MIC' ? styles.areaMic : styles.areaFq}`}>
                      {g.area}
                    </span>
                  </td>
                  <td className={styles.tdGrupo}>{g.tipoMuestra ?? <span className={styles.sinDato}>—</span>}</td>
                  <td className={styles.tdReferencia}>{g.norma ?? <span className={styles.sinDato}>—</span>}</td>
                  <td className={styles.tdReferencia}>{g.cantidadMuestra ?? <span className={styles.sinDato}>—</span>}</td>
                  <td className={styles.tdAcciones}>
                    <button className={styles.btnEditar} onClick={() => abrirEditar(g)}>Editar</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {creando && (
        <div className={styles.overlay}>
          <div className={styles.modal}>
            <div className={styles.modalHeader}>
              <h2 className={styles.modalTitulo}>Nueva categoría</h2>
              <button className={styles.btnCerrar} onClick={() => setCreando(false)}>✕</button>
            </div>
            <div className={styles.modalBody}>
              <div className={styles.campo}>
                <label>Categoría *</label>
                <input
                  placeholder="Ej: Composición proximal"
                  value={formNueva.codigoAgrupador}
                  onChange={e => setFormNueva(f => ({ ...f, codigoAgrupador: e.target.value }))}
                />
              </div>
              <div className={styles.campo}>
                <label>Área *</label>
                <select
                  value={formNueva.area}
                  onChange={e => setFormNueva(f => ({ ...f, area: e.target.value }))}
                >
                  <option value="MIC">MIC</option>
                  <option value="FQ">FQ</option>
                </select>
              </div>
              <div className={styles.campo}>
                <label>Tipo de muestra</label>
                <input
                  placeholder="Ej: Agua, Alimentos, Ambiente"
                  value={formNueva.tipoMuestra}
                  onChange={e => setFormNueva(f => ({ ...f, tipoMuestra: e.target.value }))}
                />
              </div>
              <div className={styles.campo}>
                <label>Norma / referencia</label>
                <input
                  placeholder="Ej: C.A.A. art. N° 982"
                  value={formNueva.norma}
                  onChange={e => setFormNueva(f => ({ ...f, norma: e.target.value }))}
                />
              </div>
              <div className={styles.campo}>
                <label>Cantidad de muestra requerida</label>
                <input
                  placeholder="Ej: 400mL en envase estéril"
                  value={formNueva.cantidadMuestra}
                  onChange={e => setFormNueva(f => ({ ...f, cantidadMuestra: e.target.value }))}
                />
              </div>
              {errorNueva && <p className={styles.errorModal}>{errorNueva}</p>}
            </div>
            <div className={styles.modalFooter}>
              <button className={styles.btnSecundario} onClick={() => setCreando(false)}>Cancelar</button>
              <button className={styles.btnPrimario} onClick={guardarNueva} disabled={guardandoNueva}>
                {guardandoNueva ? 'Creando...' : 'Crear categoría'}
              </button>
            </div>
          </div>
        </div>
      )}

      {editando && (
        <div className={styles.overlay}>
          <div className={styles.modal}>
            <div className={styles.modalHeader}>
              <h2 className={styles.modalTitulo}>Editar categoría — {editando.codigoAgrupador}</h2>
              <button className={styles.btnCerrar} onClick={() => setEditando(null)}>✕</button>
            </div>
            <div className={styles.modalBody}>
              <div className={styles.campo}>
                <label>Tipo de muestra</label>
                <input
                  placeholder="Ej: Agua, Alimentos, Ambiente"
                  value={form.tipoMuestra}
                  onChange={e => setForm(f => ({ ...f, tipoMuestra: e.target.value }))}
                />
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
              {error && <p className={styles.errorModal}>{error}</p>}
            </div>
            <div className={styles.modalFooter}>
              <button className={styles.btnSecundario} onClick={() => setEditando(null)}>Cancelar</button>
              <button className={styles.btnPrimario} onClick={guardar} disabled={guardando}>
                {guardando ? 'Guardando...' : 'Guardar cambios'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
