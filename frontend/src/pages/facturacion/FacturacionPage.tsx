import { Component, useEffect, useState, useCallback } from 'react'
import type { ErrorInfo, ReactNode } from 'react'
import { apiFetch } from '../../lib/api'
import { etiquetaArea } from '../../lib/areas'
import { useSesion } from '../../context/SesionContext'
import styles from './FacturacionPage.module.css'

// Si algo falla al renderizar, muestra el error en vez de dejar la pantalla en blanco
class LimiteErrores extends Component<{ children: ReactNode }, { error: string | null }> {
  state = { error: null as string | null }

  static getDerivedStateFromError(e: Error) {
    return { error: e.message || String(e) }
  }

  componentDidCatch(e: Error, info: ErrorInfo) {
    console.error('Error en Facturación:', e, info.componentStack)
  }

  render() {
    if (this.state.error) {
      return (
        <div className={styles.root}>
          <div className={styles.error}>
            <strong>La sección Facturación no pudo mostrarse.</strong>
            <div>Detalle: {this.state.error}</div>
            <button className={styles.btnSecundario} onClick={() => window.location.reload()}>Recargar</button>
          </div>
        </div>
      )
    }
    return this.props.children
  }
}

// ─── Tipos ────────────────────────────────────────────────────────────────────

interface FacturaItem {
  idFactura: number
  numero: string
  estado: 'emitida' | 'pagada' | 'anulada'
  fecha: string
  fechaPago: string | null
  importePesos: number
  importeDolares: number
  sse: { idSse: number; codigo: string | null; area: string }
  cliente: { nombre: string; apellido: string } | null
  empresa: { razonSocial: string } | null
  rotulo: string | null
}

interface SseDisponible {
  idSse: number
  codigo: string | null
  area: string
  fecha: string
  importePesos: number
  importeDolares: number
  cliente: { nombre: string; apellido: string } | null
  empresa: { razonSocial: string } | null
  rotulo: string | null
}

// ─── Constantes ───────────────────────────────────────────────────────────────

// Mismos roles que FacturasController (RolesEmision / RolesFacturacion)
const ROLES_EMISION = ['ROL-01', 'ROL-08', 'ROL-09']

const ESTADO_LABEL: Record<string, string> = {
  emitida: 'Adeudada',
  pagada: 'Pagada',
  anulada: 'Anulada',
}

const ESTADO_BADGE: Record<string, string> = {
  emitida: styles.badgeEmitida,
  pagada: styles.badgePagada,
  anulada: styles.badgeAnulada,
}

const fmtPesos = new Intl.NumberFormat('es-AR', { style: 'currency', currency: 'ARS' })
const fmtDolares = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

function nombreCliente(f: { cliente: { nombre: string; apellido: string } | null; empresa: { razonSocial: string } | null }) {
  if (f.cliente) return `${f.cliente.nombre} ${f.cliente.apellido}`
  return f.empresa?.razonSocial ?? 'Sin cliente'
}

function hoyIso() {
  return new Date().toISOString().slice(0, 10)
}

// ─── Componente principal ─────────────────────────────────────────────────────

export default function FacturacionPage() {
  return (
    <LimiteErrores>
      <FacturacionContenido />
    </LimiteErrores>
  )
}

function FacturacionContenido() {
  const { sesion } = useSesion()
  const puedeRegistrar = ROLES_EMISION.includes(sesion.rolCodigo)

  const [facturas, setFacturas] = useState<FacturaItem[]>([])
  const [total, setTotal] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [filtroEstado, setFiltroEstado] = useState('emitida')
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')

  // Cobro: factura en la que se está registrando el pago
  const [cobrandoId, setCobrandoId] = useState<number | null>(null)
  const [fechaPago, setFechaPago] = useState(hoyIso())

  // Modal de registro de factura
  const [modalAbierto, setModalAbierto] = useState(false)

  const POR_PAGINA = 50

  const cargarLista = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const params = new URLSearchParams({ pagina: String(pagina), porPagina: String(POR_PAGINA) })
      if (busqueda) params.set('busqueda', busqueda)
      if (filtroEstado) params.set('estado', filtroEstado)
      const res = await apiFetch(`/api/facturas?${params}`)
      const data = await res.json().catch(() => null)
      if (!res.ok || !data?.items) {
        setFacturas([])
        setTotal(0)
        setError(`No se pudieron cargar las facturas (error ${res.status}). Probá recargar la página.`)
        return
      }
      setFacturas(data.items)
      setTotal(data.total)
    } catch {
      setFacturas([])
      setTotal(0)
      setError('No se pudo conectar con el servidor para cargar las facturas.')
    } finally {
      setCargando(false)
    }
  }, [pagina, busqueda, filtroEstado])

  useEffect(() => { cargarLista() }, [cargarLista])

  async function confirmarCobro(idFactura: number) {
    setError('')
    const res = await apiFetch(`/api/facturas/${idFactura}/pago`, {
      method: 'PUT',
      body: JSON.stringify({ fechaPago }),
    })
    if (!res.ok) {
      const d = await res.json().catch(() => ({}))
      setError(d.error ?? 'No se pudo registrar el pago.')
      return
    }
    setCobrandoId(null)
    cargarLista()
  }

  // Agrupa las facturas de la página por cliente (RF-26: adeudadas por cliente)
  const grupos = facturas.reduce<Record<string, FacturaItem[]>>((acc, f) => {
    const clave = nombreCliente(f)
    ;(acc[clave] ??= []).push(f)
    return acc
  }, {})

  return (
    <div className={styles.root}>
      <div className={styles.encabezado}>
        <div>
          <h1 className={styles.titulo}>Facturación</h1>
          <p className={styles.subtitulo}>
            Facturas emitidas en TANGO, vinculadas a su SSE. {total} en la lista.
          </p>
        </div>
        {puedeRegistrar && (
          <button className={styles.btnPrimario} onClick={() => setModalAbierto(true)}>
            + Registrar factura
          </button>
        )}
      </div>

      <div className={styles.filtros}>
        <input
          className={styles.inputBusqueda}
          placeholder="Buscar por número, SSE, rótulo o cliente..."
          value={busqueda}
          onChange={e => { setBusqueda(e.target.value); setPagina(1) }}
        />
        <select
          className={styles.select}
          value={filtroEstado}
          onChange={e => { setFiltroEstado(e.target.value); setPagina(1) }}
        >
          <option value="emitida">Adeudadas</option>
          <option value="pagada">Pagadas</option>
          <option value="">Todas</option>
        </select>
      </div>

      {error && <div className={styles.error}>{error}</div>}

      {cargando ? (
        <div className={styles.vacio}>Cargando...</div>
      ) : facturas.length === 0 ? (
        <div className={styles.vacio}>No hay facturas{busqueda || filtroEstado ? ' que coincidan' : ' registradas todavía'}.</div>
      ) : (
        Object.entries(grupos).map(([cliente, items]) => (
          <section key={cliente} className={styles.grupo}>
            <header className={styles.grupoHeader}>
              <span className={styles.grupoNombre}>{cliente}</span>
              <span className={styles.grupoTotal}>
                {items.length} {items.length === 1 ? 'factura' : 'facturas'}
                {' · '}
                {fmtDolares.format(items.reduce((s, f) => s + f.importeDolares, 0))}
                {' · '}
                {fmtPesos.format(items.reduce((s, f) => s + f.importePesos, 0))}
              </span>
            </header>

            <div className={styles.tabla}>
              {items.map(f => (
                <div key={f.idFactura} className={styles.fila}>
                  <div className={styles.filaPrincipal}>
                    <span className={styles.numero}>{f.numero}</span>
                    <span className={styles.meta}>
                      {new Date(f.fecha).toLocaleDateString('es-AR')}
                      {' · '}
                      {f.sse.codigo ?? `SSE ${f.sse.idSse}`}
                      {f.rotulo && ` · ${f.rotulo}`}
                    </span>
                  </div>
                  <div className={styles.filaImportes}>
                    <span>{fmtPesos.format(f.importePesos)}</span>
                    <span>{fmtDolares.format(f.importeDolares)}</span>
                  </div>
                  <div className={styles.filaEstado}>
                    <span className={`${styles.badge} ${ESTADO_BADGE[f.estado]}`}>{ESTADO_LABEL[f.estado]}</span>
                    {f.estado === 'pagada' && f.fechaPago && (
                      <span className={styles.meta}>Cobrada {new Date(f.fechaPago).toLocaleDateString('es-AR')}</span>
                    )}
                  </div>
                  <div className={styles.filaAcciones}>
                    {f.estado === 'emitida' && cobrandoId !== f.idFactura && (
                      <button className={styles.btnSecundario} onClick={() => { setCobrandoId(f.idFactura); setFechaPago(hoyIso()) }}>
                        Marcar pagada
                      </button>
                    )}
                    {f.estado === 'emitida' && cobrandoId === f.idFactura && (
                      <div className={styles.cobro}>
                        <input
                          type="date"
                          className={styles.inputFecha}
                          value={fechaPago}
                          onChange={e => setFechaPago(e.target.value)}
                        />
                        <button className={styles.btnPrimario} onClick={() => confirmarCobro(f.idFactura)}>Confirmar</button>
                        <button className={styles.btnSecundario} onClick={() => setCobrandoId(null)}>Cancelar</button>
                      </div>
                    )}
                    {/* Espacio reservado: la anulación con motivo (RF-25) queda pendiente de definir */}
                    <button className={styles.btnDeshabilitado} disabled title="Anulación pendiente de definir">
                      Anular
                    </button>
                  </div>
                </div>
              ))}
            </div>
          </section>
        ))
      )}

      {total > POR_PAGINA && (
        <div className={styles.paginacion}>
          <button disabled={pagina === 1} onClick={() => setPagina(p => p - 1)}>Anterior</button>
          <span>Pág. {pagina} de {Math.ceil(total / POR_PAGINA)}</span>
          <button disabled={pagina * POR_PAGINA >= total} onClick={() => setPagina(p => p + 1)}>Siguiente</button>
        </div>
      )}

      {modalAbierto && (
        <ModalRegistrarFactura
          onCerrar={() => setModalAbierto(false)}
          onRegistrada={() => { setModalAbierto(false); setPagina(1); cargarLista() }}
        />
      )}
    </div>
  )
}

// ─── Modal: registrar factura emitida en TANGO ────────────────────────────────

function ModalRegistrarFactura({ onCerrar, onRegistrada }: { onCerrar: () => void; onRegistrada: () => void }) {
  const [sses, setSses] = useState<SseDisponible[]>([])
  const [cargandoSses, setCargandoSses] = useState(true)
  const [idSse, setIdSse] = useState('')
  const [numero, setNumero] = useState('')
  const [fecha, setFecha] = useState(hoyIso())
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    apiFetch('/api/facturas/sses-disponibles')
      .then(r => r.json())
      .then((data: SseDisponible[]) => setSses(data))
      .finally(() => setCargandoSses(false))
  }, [])

  const sseElegida = sses.find(s => String(s.idSse) === idSse) ?? null

  async function guardar() {
    setError('')
    if (!idSse) return setError('Elegí una SSE.')
    if (!numero.trim()) return setError('Ingresá el número de factura de TANGO.')
    setGuardando(true)
    try {
      const res = await apiFetch('/api/facturas', {
        method: 'POST',
        body: JSON.stringify({ idSse: Number(idSse), numero: numero.trim(), fecha }),
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo registrar la factura.')
        return
      }
      onRegistrada()
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className={styles.overlay} onClick={onCerrar}>
      <div className={styles.modal} onClick={e => e.stopPropagation()}>
        <h2 className={styles.modalTitulo}>Registrar factura emitida en TANGO</h2>

        <label className={styles.campo}>
          <span>SSE a facturar</span>
          <select className={styles.select} value={idSse} onChange={e => setIdSse(e.target.value)} disabled={cargandoSses}>
            <option value="">{cargandoSses ? 'Cargando...' : 'Elegir SSE...'}</option>
            {sses.map(s => (
              <option key={s.idSse} value={s.idSse}>
                {s.codigo ?? `SSE ${s.idSse}`} · {nombreCliente(s)}
              </option>
            ))}
          </select>
        </label>

        {sseElegida && (
          <div className={styles.resumenSse}>
            <div>Rótulo: <strong>{sseElegida.rotulo ?? '—'}</strong> · Área: <strong>{etiquetaArea(sseElegida.area)}</strong></div>
            <div>Importe: <strong>{fmtPesos.format(sseElegida.importePesos)}</strong> · <strong>{fmtDolares.format(sseElegida.importeDolares)}</strong></div>
          </div>
        )}

        <label className={styles.campo}>
          <span>Número de factura (TANGO)</span>
          <input className={styles.input} value={numero} onChange={e => setNumero(e.target.value)} maxLength={30} />
        </label>

        <label className={styles.campo}>
          <span>Fecha de emisión</span>
          <input type="date" className={styles.inputFecha} value={fecha} onChange={e => setFecha(e.target.value)} />
        </label>

        {error && <div className={styles.error}>{error}</div>}

        <div className={styles.modalAcciones}>
          <button className={styles.btnSecundario} onClick={onCerrar} disabled={guardando}>Cancelar</button>
          <button className={styles.btnPrimario} onClick={guardar} disabled={guardando}>
            {guardando ? 'Guardando...' : 'Registrar factura'}
          </button>
        </div>
      </div>
    </div>
  )
}
