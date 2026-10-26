import { useEffect, useState, useCallback, useRef } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './UsuariosPage.module.css'

interface DocumentoInstitucional {
  id: number
  nombre: string
  nombreOriginal: string
  tipoMime: string
  tamañoBytes: number
  subidoEn: string
  subidoPor: string
}

function fechaHora(iso: string) {
  return new Date(iso).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' })
}

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

// Biblioteca de documentos reutilizables (certificación ISO 9001, procedimientos, disposiciones,
// habilitantes) — se cargan una vez acá y se adjuntan desde cualquier presupuesto (ROL-01/ROL-06).
export default function DocumentosInstitucionalesTab() {
  const [documentos, setDocumentos] = useState<DocumentoInstitucional[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [modalSubir, setModalSubir] = useState(false)

  const cargar = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const res = await apiFetch('/api/documentos-institucionales')
      if (!res.ok) {
        setDocumentos([])
        setError(`No se pudieron cargar los documentos (error ${res.status}).`)
        return
      }
      setDocumentos(await res.json())
    } catch {
      setDocumentos([])
      setError('No se pudo conectar con el servidor.')
    } finally {
      setCargando(false)
    }
  }, [])

  useEffect(() => { cargar() }, [cargar])

  async function eliminar(d: DocumentoInstitucional) {
    if (!window.confirm(`¿Eliminar "${d.nombre}"? No se puede deshacer.`)) return
    setError('')
    const res = await apiFetch(`/api/documentos-institucionales/${d.id}`, { method: 'DELETE' })
    if (!res.ok) {
      setError('No se pudo eliminar el documento.')
      return
    }
    cargar()
  }

  function descargar(d: DocumentoInstitucional) {
    apiFetch(`/api/documentos-institucionales/${d.id}`)
      .then(r => r.blob())
      .then(blob => {
        const url = URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = d.nombreOriginal
        a.click()
        URL.revokeObjectURL(url)
      })
  }

  return (
    <>
      <div className={styles.barra}>
        <p className={styles.ayuda} style={{ flex: 1 }}>
          Certificación ISO 9001, procedimientos, disposiciones y habilitantes — se cargan una vez acá
          y se pueden adjuntar directamente desde cualquier presupuesto.
        </p>
        <button className={styles.btnPrimario} onClick={() => setModalSubir(true)}>+ Nuevo documento</button>
      </div>

      {error && <div className={styles.error}>{error}</div>}

      {cargando ? (
        <div className={styles.vacio}>Cargando...</div>
      ) : documentos.length === 0 ? (
        <div className={styles.vacio}>Todavía no hay documentos cargados en la biblioteca.</div>
      ) : (
        <div className={styles.lista}>
          {documentos.map(d => (
            <div key={d.id} className={styles.fila}>
              <div className={styles.filaPrincipal}>
                <span className={styles.nombre}>{d.nombre}</span>
                <span className={styles.meta}>{d.nombreOriginal} · {formatBytes(d.tamañoBytes)}</span>
              </div>
              <div className={styles.filaEstado}>
                <span className={styles.meta}>Subido por {d.subidoPor} · {fechaHora(d.subidoEn)}</span>
              </div>
              <div className={styles.acciones}>
                <button className={styles.btnSecundario} onClick={() => descargar(d)}>Descargar</button>
                <button className={styles.btnPeligro} onClick={() => eliminar(d)}>Eliminar</button>
              </div>
            </div>
          ))}
        </div>
      )}

      {modalSubir && (
        <ModalSubirDocumento
          onCerrar={() => setModalSubir(false)}
          onSubido={() => { setModalSubir(false); cargar() }}
        />
      )}
    </>
  )
}

function ModalSubirDocumento({ onCerrar, onSubido }: { onCerrar: () => void; onSubido: () => void }) {
  const [nombre, setNombre] = useState('')
  const [archivo, setArchivo] = useState<File | null>(null)
  const [error, setError] = useState('')
  const [guardando, setGuardando] = useState(false)
  const inputRef = useRef<HTMLInputElement>(null)

  async function guardar() {
    setError('')
    if (!nombre.trim()) { setError('Ingresá un nombre para identificar el documento.'); return }
    if (!archivo) { setError('Seleccioná un archivo.'); return }
    setGuardando(true)
    try {
      const form = new FormData()
      form.append('nombre', nombre.trim())
      form.append('archivo', archivo)
      const res = await apiFetch('/api/documentos-institucionales', { method: 'POST', body: form, headers: {} })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo subir el documento.')
        return
      }
      onSubido()
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className={styles.overlay} onClick={onCerrar}>
      <div className={styles.modal} onClick={e => e.stopPropagation()}>
        <h2 className={styles.modalTitulo}>Nuevo documento institucional</h2>

        <label className={styles.campo}>
          <span>Nombre (para identificarlo en la biblioteca)</span>
          <input value={nombre} onChange={e => setNombre(e.target.value)} placeholder="Certificación ISO 9001" />
        </label>

        <label className={styles.campo}>
          <span>Archivo</span>
          <input
            ref={inputRef}
            type="file"
            accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg"
            onChange={e => setArchivo(e.target.files?.[0] ?? null)}
          />
        </label>

        {error && <div className={styles.error}>{error}</div>}

        <div className={styles.modalAcciones}>
          <button type="button" className={styles.btnSecundario} onClick={onCerrar} disabled={guardando}>Cancelar</button>
          <button type="button" className={styles.btnPrimario} onClick={guardar} disabled={guardando}>
            {guardando ? 'Subiendo…' : 'Subir documento'}
          </button>
        </div>
      </div>
    </div>
  )
}
