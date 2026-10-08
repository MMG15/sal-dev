import { useEffect, useState, type FormEvent } from 'react'
import { apiFetch } from '../../lib/api'
import { useSesion } from '../../context/SesionContext'
import styles from './PortalPage.module.css'

// ─── Tipos ────────────────────────────────────────────────────────────────────

interface Pedido {
  idMuestra: number
  numeroRotulo: string
  codigoSse: string | null
  fechaRecepcion: string
  estado: 'en_analisis' | 'disponible' | 'rechazada'
  informe: { idInforme: number; codigo: string; fechaGeneracion: string } | null
}

interface ResultadoInforme {
  area: string
  analisis: string
  valor: string | null
  unidad: string | null
  referencia: string | null
}

interface DetalleInforme {
  idInforme: number
  codigo: string
  fecha: string
  rotulo: string
  codigoSse: string | null
  resultados: ResultadoInforme[]
}

const ESTADO_LABEL: Record<string, string> = {
  en_analisis: 'En análisis',
  disponible: 'Disponible',
  rechazada: 'Muestra rechazada',
}

const ESTADO_BADGE: Record<string, string> = {
  en_analisis: styles.badgeEnCurso,
  disponible: styles.badgeDisponible,
  rechazada: styles.badgeRechazada,
}

function fecha(iso: string) {
  return new Date(iso).toLocaleDateString('es-AR')
}

// ─── Página ───────────────────────────────────────────────────────────────────

export default function PortalPage() {
  const { sesion, cerrarSesion } = useSesion()

  if (sesion.forzarCambioPwd) {
    return <CambioPasswordPortal />
  }

  return (
    <div className={styles.fondo}>
      <header className={styles.cabecera}>
        <div className={styles.marca}>
          <img src="/logo-Laboratorio-Graffigna-editado_droid-serif.png" alt="Laboratorio Graffigna" className={styles.logo} />
          <div>
            <div className={styles.tituloPortal}>Portal de resultados</div>
            <div className={styles.saludo}>{sesion.nombre} {sesion.apellido}</div>
          </div>
        </div>
        <button className={styles.btnSecundario} onClick={cerrarSesion}>Salir</button>
      </header>

      <main className={styles.contenido}>
        <PedidosPortal />
      </main>
    </div>
  )
}

// ─── Cambio de contraseña del primer ingreso ──────────────────────────────────

function CambioPasswordPortal() {
  const { sesion, cerrarSesion } = useSesion()
  const [actual, setActual] = useState('')
  const [nueva, setNueva] = useState('')
  const [repetir, setRepetir] = useState('')
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)

  async function guardar(e: FormEvent) {
    e.preventDefault()
    setError('')
    if (nueva.length < 8) return setError('La nueva contraseña debe tener al menos 8 caracteres.')
    if (nueva !== repetir) return setError('Las contraseñas nuevas no coinciden.')

    setEnviando(true)
    try {
      const res = await apiFetch('/api/portal/cambiar-password', {
        method: 'POST',
        body: JSON.stringify({ passwordActual: actual, passwordNueva: nueva }),
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo cambiar la contraseña.')
        return
      }
      // Actualiza la sesión guardada para no volver a pedir el cambio
      try {
        const raw = localStorage.getItem('ssal_sesion')
        if (raw) localStorage.setItem('ssal_sesion', JSON.stringify({ ...JSON.parse(raw), forzarCambioPwd: false }))
      } catch { /* sin acceso a localStorage: se pedirá de nuevo al recargar */ }
      window.location.reload()
    } finally {
      setEnviando(false)
    }
  }

  return (
    <div className={styles.fondo}>
      <header className={styles.cabecera}>
        <div className={styles.marca}>
          <div>
            <div className={styles.tituloPortal}>Portal de resultados</div>
            <div className={styles.saludo}>{sesion.nombre} {sesion.apellido}</div>
          </div>
        </div>
        <button className={styles.btnSecundario} onClick={cerrarSesion}>Salir</button>
      </header>

      <main className={styles.contenido}>
        <form className={styles.tarjetaForm} onSubmit={guardar}>
          <h2 className={styles.tituloSeccion}>Cambiá tu contraseña</h2>
          <p className={styles.ayuda}>Por seguridad, tenés que reemplazar la contraseña temporal antes de ver tus resultados.</p>

          <label className={styles.campo}>
            <span>Contraseña temporal</span>
            <input type="password" value={actual} onChange={e => setActual(e.target.value)} required />
          </label>
          <label className={styles.campo}>
            <span>Nueva contraseña (mínimo 8 caracteres)</span>
            <input type="password" value={nueva} onChange={e => setNueva(e.target.value)} required />
          </label>
          <label className={styles.campo}>
            <span>Repetir nueva contraseña</span>
            <input type="password" value={repetir} onChange={e => setRepetir(e.target.value)} required />
          </label>

          {error && <div className={styles.error} role="alert">{error}</div>}

          <button type="submit" className={styles.btnPrimario} disabled={enviando}>
            {enviando ? 'Guardando…' : 'Guardar contraseña'}
          </button>
        </form>
      </main>
    </div>
  )
}

// ─── Lista de pedidos y detalle ───────────────────────────────────────────────

function PedidosPortal() {
  const [pedidos, setPedidos] = useState<Pedido[] | null>(null)
  const [error, setError] = useState('')
  const [abierto, setAbierto] = useState<DetalleInforme | null>(null)
  const [cargandoDetalle, setCargandoDetalle] = useState(false)

  useEffect(() => {
    apiFetch('/api/portal/pedidos')
      .then(async r => {
        if (!r.ok) throw new Error(String(r.status))
        setPedidos(await r.json())
      })
      .catch(() => setError('No se pudieron cargar tus pedidos. Probá de nuevo en unos minutos.'))
  }, [])

  async function abrirInforme(idInforme: number) {
    setCargandoDetalle(true)
    setError('')
    try {
      const res = await apiFetch(`/api/portal/informes/${idInforme}`)
      if (!res.ok) {
        setError('Este informe todavía no está disponible.')
        return
      }
      setAbierto(await res.json())
    } finally {
      setCargandoDetalle(false)
    }
  }

  function descargarPdf(idInforme: number, codigo: string) {
    apiFetch(`/api/portal/informes/${idInforme}/pdf`)
      .then(r => r.blob())
      .then(blob => {
        const url = URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = `${codigo}.pdf`
        a.click()
        URL.revokeObjectURL(url)
      })
  }

  if (error && !pedidos) return <div className={styles.error}>{error}</div>
  if (!pedidos) return <div className={styles.vacio}>Cargando tus pedidos…</div>

  if (abierto) {
    return (
      <section className={styles.tarjeta}>
        <button className={styles.btnSecundario} onClick={() => setAbierto(null)}>← Volver a mis pedidos</button>

        <div className={styles.detalleCabecera}>
          <div>
            <div className={styles.rotuloGrande}>Rótulo {abierto.rotulo}</div>
            <div className={styles.ayuda}>Informe {abierto.codigo}{abierto.codigoSse ? ` · SSE ${abierto.codigoSse}` : ''} · {fecha(abierto.fecha)}</div>
          </div>
          <button className={styles.btnPrimario} onClick={() => descargarPdf(abierto.idInforme, abierto.codigo)}>Descargar PDF</button>
        </div>

        <div className={styles.tablaWrap}>
          <table className={styles.tabla}>
            <thead>
              <tr><th>Área</th><th>Análisis</th><th>Resultado</th><th>Unidad</th><th>Referencia</th></tr>
            </thead>
            <tbody>
              {abierto.resultados.map((r, i) => (
                <tr key={i}>
                  <td>{r.area}</td>
                  <td>{r.analisis}</td>
                  <td className={styles.valor}>{r.valor ?? '—'}</td>
                  <td>{r.unidad ?? '—'}</td>
                  <td>{r.referencia ?? '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </section>
    )
  }

  if (pedidos.length === 0) {
    return <div className={styles.vacio}>Todavía no tenés pedidos registrados.</div>
  }

  return (
    <section className={styles.lista}>
      <h2 className={styles.tituloSeccion}>Mis pedidos</h2>
      {error && <div className={styles.error}>{error}</div>}
      {pedidos.map(p => (
        <div key={p.idMuestra} className={styles.pedido}>
          <div className={styles.pedidoCabeza}>
            <span className={styles.rotulo}>Rótulo {p.numeroRotulo}</span>
            <span className={`${styles.badge} ${ESTADO_BADGE[p.estado]}`}>{ESTADO_LABEL[p.estado]}</span>
          </div>
          <div className={styles.ayuda}>
            {p.codigoSse ? `SSE ${p.codigoSse} · ` : ''}Recibida el {fecha(p.fechaRecepcion)}
          </div>
          {p.informe && (
            <button
              className={styles.btnPrimario}
              disabled={cargandoDetalle}
              onClick={() => abrirInforme(p.informe!.idInforme)}
            >
              Ver resultados
            </button>
          )}
          {p.estado === 'en_analisis' && (
            <p className={styles.ayuda}>Los resultados se publican cuando el informe está completo y aprobado.</p>
          )}
        </div>
      ))}
    </section>
  )
}
