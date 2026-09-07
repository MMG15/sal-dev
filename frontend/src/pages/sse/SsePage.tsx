import { useEffect, useState, useCallback } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './SsePage.module.css'

// ─── Tipos ────────────────────────────────────────────────────────────────────

interface SseResumen {
  idSse: number
  codigo: string | null
  codigoRpo: string | null
  area: string
  estado: string
  fecha: string
  importeDolares: number
  importePesos: number
  cliente: { nombre: string; apellido: string } | null
  empresa: { razonSocial: string } | null
  rotulo: { idRotulo: number; numeroUnico: string; estado: string } | null
}

interface SseDetalle {
  idSse: number
  codigo: string | null
  codigoRpo: string | null
  area: string
  estado: string
  fecha: string
  formaPago: string | null
  importeDolares: number
  importePesos: number
  creadoPor: { nombre: string; apellido: string }
  cliente: {
    idCliente: number; nombre: string; apellido: string
    email: string | null; telefono: string | null
    empresa: { razonSocial: string } | null
  } | null
  presupuesto: {
    idPresupuesto: number; codigoRpo: string | null
    importeDolares: number; importePesos: number
    analisis: { nombre: string; area: string }[]
  } | null
  rotulo: {
    idRotulo: number; numeroUnico: string; descripcion: string | null
    estado: string; fechaAsignacion: string
    asignadoPor: { nombre: string; apellido: string }
    muestra: {
      idMuestra: number; tipo: string | null; estado: string
      observacion: string | null; fechaRecepcion: string
      recibidoPor: { nombre: string; apellido: string }
    } | null
    auditorias: {
      idAuditoria: number; campo: string; valorAnterior: string | null; valorNuevo: string | null
      motivo: string | null; fechaCambio: string
      usuario: { nombre: string; apellido: string }
    }[]
  } | null
}

interface PresupuestoOpcion {
  idPresupuesto: number
  codigoRpo: string | null
  importeDolares: number
  consultaCodigo: string | null
  cliente: { nombre: string; apellido: string } | null
  empresa: { razonSocial: string } | null
}

// ─── Constantes ───────────────────────────────────────────────────────────────

const ESTADOS_SSE = ['activa', 'analizando', 'completada', 'cerrada', 'rechazada']
const AREAS = ['MIC', 'FQ', 'AMBAS']
const AREAS_SELECCIONABLES = ['MIC', 'FQ']

const BADGE_ESTADO: Record<string, string> = {
  activa: styles.badgeActiva,
  analizando: styles.badgeEnAnalisis,
  completada: styles.badgeCompletada,
  cerrada: styles.badgeCerrada,
  activo: styles.badgeActiva,
  cancelado: styles.badgeRechazado,
  en_proceso: styles.badgeEnAnalisis,
  rechazada: styles.badgeRechazado,
}

const AREA_BADGE: Record<string, string> = {
  MIC: styles.areaMic,
  FQ: styles.areaFq,
  AMBAS: styles.areaAmbas,
}

const CAMPO_AUDITORIA_LABEL: Record<string, string> = {
  numero_unico: 'el número',
  descripcion: 'la descripción',
  estado: 'el estado',
}

// ─── Componente principal ─────────────────────────────────────────────────────

export default function SsePage() {
  const [lista, setLista] = useState<SseResumen[]>([])
  const [total, setTotal] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [filtroEstado, setFiltroEstado] = useState('')
  const [filtroArea, setFiltroArea] = useState('')
  const [cargando, setCargando] = useState(true)

  const [seleccionada, setSeleccionada] = useState<SseDetalle | null>(null)
  const [cargandoDetalle, setCargandoDetalle] = useState(false)

  const [modalNueva, setModalNueva] = useState(false)
  const [modalRotulo, setModalRotulo] = useState(false)
  const [modalMuestra, setModalMuestra] = useState(false)
  const [modalCorregirRotulo, setModalCorregirRotulo] = useState(false)

  const POR_PAGINA = 20

  const cargarLista = useCallback(async () => {
    setCargando(true)
    try {
      const params = new URLSearchParams({ pagina: String(pagina), porPagina: String(POR_PAGINA) })
      if (busqueda) params.set('busqueda', busqueda)
      if (filtroEstado) params.set('estado', filtroEstado)
      if (filtroArea) params.set('area', filtroArea)
      const res = await apiFetch(`/api/sses?${params}`)
      const data = await res.json()
      setLista(data.items)
      setTotal(data.total)
    } finally {
      setCargando(false)
    }
  }, [pagina, busqueda, filtroEstado, filtroArea])

  useEffect(() => { cargarLista() }, [cargarLista])

  async function abrirDetalle(id: number) {
    setSeleccionada(null)
    setCargandoDetalle(true)
    const res = await apiFetch(`/api/sses/${id}`)
    setSeleccionada(await res.json())
    setCargandoDetalle(false)
  }

  const nombreCliente = (s: SseResumen | SseDetalle) => {
    if ('cliente' in s && s.cliente) return `${s.cliente.nombre} ${s.cliente.apellido}`
    if ('empresa' in s && s.empresa) return s.empresa.razonSocial
    return '—'
  }

  return (
    <div className={styles.root}>
      {/* ── Lista ── */}
      <div className={styles.panelLista}>
        <div className={styles.listaHeader}>
          <h1 className={styles.titulo}>SSE / Rótulos</h1>
          <button className={styles.btnPrimario} onClick={() => setModalNueva(true)}>+ Nueva SSE</button>
        </div>

        <div className={styles.filtros}>
          <input
            className={styles.inputBusqueda}
            placeholder="Código SSE, rótulo, cliente..."
            value={busqueda}
            onChange={e => { setBusqueda(e.target.value); setPagina(1) }}
          />
          <select className={styles.selectFiltro} value={filtroEstado} onChange={e => { setFiltroEstado(e.target.value); setPagina(1) }}>
            <option value="">Todos los estados</option>
            {ESTADOS_SSE.map(e => <option key={e} value={e}>{e}</option>)}
          </select>
          <select className={styles.selectFiltro} value={filtroArea} onChange={e => { setFiltroArea(e.target.value); setPagina(1) }}>
            <option value="">Todas las áreas</option>
            {AREAS.map(a => <option key={a} value={a}>{a}</option>)}
          </select>
        </div>

        {cargando ? (
          <div className={styles.cargando}>Cargando...</div>
        ) : lista.length === 0 ? (
          <div className={styles.vacio}>
            {busqueda || filtroEstado || filtroArea
              ? 'Sin resultados.'
              : 'No hay SSEs registradas. Creá la primera con el botón de arriba.'}
          </div>
        ) : (
          <div className={styles.tabla}>
            {lista.map(s => (
              <div
                key={s.idSse}
                className={`${styles.fila} ${seleccionada?.idSse === s.idSse ? styles.filaActiva : ''}`}
                onClick={() => abrirDetalle(s.idSse)}
              >
                <div className={styles.filaCabeza}>
                  <span className={styles.codigo}>{s.codigo ?? '—'}</span>
                  <span className={`${styles.areaBadge} ${AREA_BADGE[s.area] ?? ''}`}>{s.area}</span>
                  <span className={`${styles.badge} ${BADGE_ESTADO[s.estado] ?? ''}`}>{s.estado}</span>
                </div>
                <div className={styles.filaCliente}>{nombreCliente(s)}</div>
                <div className={styles.filaInfo}>
                  {s.rotulo
                    ? <span className={styles.rotuloBadge}>🏷 {s.rotulo.numeroUnico}</span>
                    : <span className={styles.sinRotulo}>Sin rótulo</span>}
                  <span className={styles.fechaChip}>{new Date(s.fecha).toLocaleDateString('es-AR')}</span>
                </div>
              </div>
            ))}
          </div>
        )}

        {total > POR_PAGINA && (
          <div className={styles.paginacion}>
            <button disabled={pagina === 1} onClick={() => setPagina(p => p - 1)}>Anterior</button>
            <span>Pág. {pagina} / {Math.ceil(total / POR_PAGINA)}</span>
            <button disabled={pagina * POR_PAGINA >= total} onClick={() => setPagina(p => p + 1)}>Siguiente</button>
          </div>
        )}
      </div>

      {/* ── Detalle ── */}
      {(seleccionada !== null || cargandoDetalle) && (
        <div className={styles.panelDetalle}>
          {cargandoDetalle ? (
            <div className={styles.cargando}>Cargando...</div>
          ) : seleccionada && (
            <>
              <div className={styles.detalleHeader}>
                <div className={styles.detalleTitulo}>
                  <span className={styles.codigoGrande}>{seleccionada.codigo ?? '—'}</span>
                  <span className={`${styles.areaBadge} ${AREA_BADGE[seleccionada.area] ?? ''}`}>{seleccionada.area}</span>
                  <span className={`${styles.badge} ${BADGE_ESTADO[seleccionada.estado] ?? ''}`}>{seleccionada.estado}</span>
                </div>
                <button className={styles.btnCerrar} onClick={() => setSeleccionada(null)}>✕</button>
              </div>

              <div className={styles.detalleBody}>
                {/* Datos del servicio */}
                <Section titulo="Datos del servicio">
                  {seleccionada.codigoRpo && <Campo label="Presupuesto" valor={seleccionada.codigoRpo} />}
                  <Campo label="Área" valor={seleccionada.area} />
                  <Campo label="Forma de pago" valor={seleccionada.formaPago ?? '—'} />
                  <div className={styles.montosRow}>
                    <div className={styles.montoCard}>
                      <span>Total USD</span>
                      <strong>USD {seleccionada.importeDolares.toFixed(2)}</strong>
                    </div>
                    <div className={styles.montoCard}>
                      <span>Total ARS</span>
                      <strong>$ {seleccionada.importePesos.toFixed(2)}</strong>
                    </div>
                  </div>
                  <Campo label="Registrada" valor={new Date(seleccionada.fecha).toLocaleDateString('es-AR')} />
                  <Campo label="Creada por" valor={`${seleccionada.creadoPor.nombre} ${seleccionada.creadoPor.apellido}`} />
                </Section>

                {/* Cliente */}
                {seleccionada.cliente && (
                  <Section titulo="Cliente">
                    <Campo label="Nombre" valor={`${seleccionada.cliente.nombre} ${seleccionada.cliente.apellido}`} />
                    {seleccionada.cliente.email && <Campo label="Email" valor={seleccionada.cliente.email} />}
                    {seleccionada.cliente.telefono && <Campo label="Teléfono" valor={seleccionada.cliente.telefono} />}
                    {seleccionada.cliente.empresa && <Campo label="Empresa" valor={seleccionada.cliente.empresa.razonSocial} />}
                  </Section>
                )}

                {/* Análisis del presupuesto */}
                {seleccionada.presupuesto && seleccionada.presupuesto.analisis.length > 0 && (
                  <Section titulo="Análisis solicitados">
                    {seleccionada.presupuesto.analisis.map((a, i) => (
                      <div key={i} className={styles.analisisItem}>
                        <span className={`${styles.areaBadge} ${AREA_BADGE[a.area] ?? ''}`}>{a.area}</span>
                        <span>{a.nombre}</span>
                      </div>
                    ))}
                  </Section>
                )}

                {/* Rótulo Interno */}
                <Section titulo="Rótulo Interno">
                  {seleccionada.rotulo ? (
                    <>
                      <div className={styles.rotuloCabeza}>
                        <span className={styles.rotuloNumero}>🏷 {seleccionada.rotulo.numeroUnico}</span>
                        <span className={`${styles.badge} ${BADGE_ESTADO[seleccionada.rotulo.estado] ?? ''}`}>
                          {seleccionada.rotulo.estado}
                        </span>
                        <button className={styles.btnCorregir} onClick={() => setModalCorregirRotulo(true)}>
                          Corregir
                        </button>
                      </div>
                      {seleccionada.rotulo.descripcion && (
                        <p className={styles.rotuloDesc}>{seleccionada.rotulo.descripcion}</p>
                      )}
                      <p className={styles.metaDato}>
                        Asignado por {seleccionada.rotulo.asignadoPor.nombre} {seleccionada.rotulo.asignadoPor.apellido}
                        {' · '}{new Date(seleccionada.rotulo.fechaAsignacion).toLocaleDateString('es-AR')}
                      </p>

                      {/* Muestra */}
                      <div className={styles.muestraBloque}>
                        <span className={styles.muestraTitulo}>Muestra</span>
                        {seleccionada.rotulo.muestra ? (
                          <div className={styles.muestraDetalle}>
                            <Campo label="Tipo" valor={seleccionada.rotulo.muestra.tipo ?? '—'} />
                            <Campo label="Estado" valor={seleccionada.rotulo.muestra.estado} />
                            {seleccionada.rotulo.muestra.observacion && (
                              <Campo label="Observación" valor={seleccionada.rotulo.muestra.observacion} />
                            )}
                            <Campo label="Recibida" valor={new Date(seleccionada.rotulo.muestra.fechaRecepcion).toLocaleDateString('es-AR')} />
                            <Campo label="Recibida por" valor={`${seleccionada.rotulo.muestra.recibidoPor.nombre} ${seleccionada.rotulo.muestra.recibidoPor.apellido}`} />
                          </div>
                        ) : (
                          <div className={styles.sinMuestra}>
                            <span>Sin muestra registrada.</span>
                            <button className={styles.btnSecundario} onClick={() => setModalMuestra(true)}>
                              Registrar muestra
                            </button>
                          </div>
                        )}
                      </div>

                      {/* Auditoría */}
                      {seleccionada.rotulo.auditorias.length > 0 && (
                        <div className={styles.auditoriaBloque}>
                          <span className={styles.muestraTitulo}>Historial de cambios</span>
                          {seleccionada.rotulo.auditorias.map(a => (
                            <div key={a.idAuditoria} className={styles.auditoriaItem}>
                              <span className={styles.auditoriaFecha}>{new Date(a.fechaCambio).toLocaleDateString('es-AR')}</span>
                              <span>
                                {a.usuario.nombre} {a.usuario.apellido} cambió {CAMPO_AUDITORIA_LABEL[a.campo] ?? a.campo}:
                                {' '}<s>{a.valorAnterior || '—'}</s> → <strong>{a.valorNuevo || '—'}</strong>
                              </span>
                              {a.motivo && <span className={styles.auditoriaMotivo}>"{a.motivo}"</span>}
                            </div>
                          ))}
                        </div>
                      )}
                    </>
                  ) : (
                    <div className={styles.sinRotuloDetalle}>
                      <p>No hay rótulo asignado a esta SSE.</p>
                      <button className={styles.btnPrimario} onClick={() => setModalRotulo(true)}>
                        Asignar rótulo
                      </button>
                    </div>
                  )}
                </Section>
              </div>
            </>
          )}
        </div>
      )}

      {/* ── Modales ── */}
      {modalNueva && (
        <ModalNuevaSse
          onCreada={async (id) => { setModalNueva(false); await cargarLista(); await abrirDetalle(id) }}
          onCerrar={() => setModalNueva(false)}
        />
      )}

      {modalRotulo && seleccionada && (
        <ModalRotulo
          idSse={seleccionada.idSse}
          onGuardado={async () => { setModalRotulo(false); await abrirDetalle(seleccionada.idSse); cargarLista() }}
          onCerrar={() => setModalRotulo(false)}
        />
      )}

      {modalMuestra && seleccionada && (
        <ModalMuestra
          idSse={seleccionada.idSse}
          onGuardado={async () => { setModalMuestra(false); await abrirDetalle(seleccionada.idSse); cargarLista() }}
          onCerrar={() => setModalMuestra(false)}
        />
      )}

      {modalCorregirRotulo && seleccionada?.rotulo && (
        <ModalCorregirRotulo
          idSse={seleccionada.idSse}
          rotulo={seleccionada.rotulo}
          onGuardado={async () => { setModalCorregirRotulo(false); await abrirDetalle(seleccionada.idSse); cargarLista() }}
          onCerrar={() => setModalCorregirRotulo(false)}
        />
      )}
    </div>
  )
}

// ─── Sub-componentes ──────────────────────────────────────────────────────────

function Section({ titulo, children }: { titulo: string; children: React.ReactNode }) {
  return (
    <div className={styles.section}>
      <h3 className={styles.sectionTitulo}>{titulo}</h3>
      {children}
    </div>
  )
}

function Campo({ label, valor }: { label: string; valor: string }) {
  return (
    <div className={styles.campo}>
      <span className={styles.campoLabel}>{label}</span>
      <span>{valor}</span>
    </div>
  )
}

// ─── Modal: Nueva SSE ─────────────────────────────────────────────────────────

function ModalNuevaSse({ onCreada, onCerrar }: {
  onCreada: (id: number) => void
  onCerrar: () => void
}) {
  const [presupuestos, setPresupuestos] = useState<PresupuestoOpcion[]>([])
  const [idPresupuesto, setIdPresupuesto] = useState<number | ''>('')
  const [areasSel, setAreasSel] = useState<string[]>(['MIC'])
  const [formaPago, setFormaPago] = useState('transferencia')
  // Se guardan como texto y arrancan vacíos (no en "0") para que el usuario pueda
  // escribir libremente sin que un cero inicial quede pegado a lo que va tipeando.
  const [importeDolares, setImporteDolares] = useState('')
  const [importePesos, setImportePesos] = useState('')
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    apiFetch('/api/presupuestos?estado=aceptado&porPagina=100')
      .then(r => r.json())
      .then(data => setPresupuestos(data.items ?? []))
  }, [])

  function onPresupuestoChange(id: number) {
    setIdPresupuesto(id)
    const p = presupuestos.find(x => x.idPresupuesto === id)
    if (p) {
      setImporteDolares(String(p.importeDolares))
    }
  }

  function toggleArea(a: string) {
    setAreasSel(prev => {
      if (prev.includes(a)) {
        // No permitir dejar el formulario sin ninguna área seleccionada
        return prev.length > 1 ? prev.filter(x => x !== a) : prev
      }
      return [...prev, a]
    })
  }

  async function guardar() {
    setGuardando(true)
    setError('')
    try {
      const area = areasSel.length === AREAS_SELECCIONABLES.length ? 'AMBAS' : areasSel[0]
      const res = await apiFetch('/api/sses', {
        method: 'POST',
        body: JSON.stringify({
          idPresupuesto: idPresupuesto !== '' ? idPresupuesto : null,
          idCliente: null,
          area,
          formaPago,
          importeDolares: parseFloat(importeDolares) || 0,
          importePesos: parseFloat(importePesos) || 0
        })
      })
      if (!res.ok) { setError('Error al crear la SSE.'); return }
      const data = await res.json()
      onCreada(data.idSse)
    } finally {
      setGuardando(false)
    }
  }

  const presupuestoSel = presupuestos.find(p => p.idPresupuesto === idPresupuesto)

  return (
    <div className={styles.overlay} onClick={onCerrar}>
      <div className={styles.modal} onClick={e => e.stopPropagation()}>
        <div className={styles.modalHeader}>
          <h2>Nueva SSE</h2>
          <button className={styles.btnCerrar} onClick={onCerrar}>✕</button>
        </div>
        <div className={styles.modalBody}>
          <label className={styles.formLabel}>Presupuesto vinculado (opcional)</label>
          <select
            className={styles.formInput}
            value={idPresupuesto}
            onChange={e => e.target.value ? onPresupuestoChange(Number(e.target.value)) : setIdPresupuesto('')}
          >
            <option value="">— Sin presupuesto —</option>
            {presupuestos.map(p => (
              <option key={p.idPresupuesto} value={p.idPresupuesto}>
                {p.codigoRpo} — {p.cliente ? `${p.cliente.nombre} ${p.cliente.apellido}` : p.empresa?.razonSocial ?? '?'}
              </option>
            ))}
          </select>

          {presupuestoSel && (
            <p className={styles.infoPresupuesto}>
              USD {presupuestoSel.importeDolares.toFixed(2)}
            </p>
          )}

          <label className={styles.formLabel}>Área (podés seleccionar ambas)</label>
          <div className={styles.radioGroup}>
            {AREAS_SELECCIONABLES.map(a => (
              <label key={a} className={`${styles.radioOpt} ${areasSel.includes(a) ? styles.radioActivo : ''}`}>
                <input type="checkbox" checked={areasSel.includes(a)} onChange={() => toggleArea(a)} />
                {a}
              </label>
            ))}
          </div>

          <label className={styles.formLabel}>Forma de pago</label>
          <select className={styles.formInput} value={formaPago} onChange={e => setFormaPago(e.target.value)}>
            <option value="transferencia">Transferencia bancaria</option>
            <option value="efectivo">Efectivo</option>
            <option value="cheque">Cheque</option>
            <option value="cuenta_corriente">Cuenta corriente</option>
          </select>

          <div className={styles.montosForm}>
            <div>
              <label className={styles.formLabel}>Importe USD</label>
              <input type="number" className={styles.formInput} min={0} step={0.01} placeholder="0.00"
                value={importeDolares} onChange={e => setImporteDolares(e.target.value)} />
            </div>
            <div>
              <label className={styles.formLabel}>Importe ARS</label>
              <input type="number" className={styles.formInput} min={0} step={1} placeholder="0"
                value={importePesos} onChange={e => setImportePesos(e.target.value)} />
            </div>
          </div>

          {error && <p className={styles.error}>{error}</p>}
        </div>
        <div className={styles.modalFooter}>
          <button className={styles.btnSecundario} onClick={onCerrar}>Cancelar</button>
          <button className={styles.btnPrimario} onClick={guardar} disabled={guardando}>
            {guardando ? 'Creando...' : 'Crear SSE'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ─── Modal: Asignar Rótulo ────────────────────────────────────────────────────

function ModalRotulo({ idSse, onGuardado, onCerrar }: {
  idSse: number
  onGuardado: () => void
  onCerrar: () => void
}) {
  const [numeroUnico, setNumeroUnico] = useState('')
  const [descripcion, setDescripcion] = useState('')
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  async function guardar() {
    setGuardando(true)
    setError('')
    try {
      const res = await apiFetch(`/api/sses/${idSse}/rotulo`, {
        method: 'POST',
        body: JSON.stringify({ numeroUnico: numeroUnico.trim() || null, descripcion: descripcion || null })
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'Error al asignar el rótulo.')
        return
      }
      onGuardado()
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className={styles.overlay} onClick={onCerrar}>
      <div className={styles.modal} onClick={e => e.stopPropagation()}>
        <div className={styles.modalHeader}>
          <h2>Asignar Rótulo Interno</h2>
          <button className={styles.btnCerrar} onClick={onCerrar}>✕</button>
        </div>
        <div className={styles.modalBody}>
          <label className={styles.formLabel}>Número de rótulo</label>
          <input
            className={styles.formInput}
            placeholder="ROT-YYYYMM-NNNN (dejar vacío para auto-generar)"
            value={numeroUnico}
            onChange={e => setNumeroUnico(e.target.value)}
          />
          <p className={styles.inputHint}>Si lo dejás vacío, se genera automáticamente con formato ROT-YYYYMM-NNNN</p>

          <label className={styles.formLabel}>Descripción / observaciones</label>
          <textarea
            className={styles.formTextarea}
            rows={3}
            value={descripcion}
            onChange={e => setDescripcion(e.target.value)}
            placeholder="Descripción de la muestra o notas del rótulo..."
          />
          {error && <p className={styles.error}>{error}</p>}
        </div>
        <div className={styles.modalFooter}>
          <button className={styles.btnSecundario} onClick={onCerrar}>Cancelar</button>
          <button className={styles.btnPrimario} onClick={guardar} disabled={guardando}>
            {guardando ? 'Asignando...' : 'Asignar rótulo'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ─── Modal: Corregir Rótulo (RF-13/RF-14: confirmación explícita + auditoría) ──

const ESTADOS_ROTULO = ['activo', 'anulado']

function ModalCorregirRotulo({ idSse, rotulo, onGuardado, onCerrar }: {
  idSse: number
  rotulo: { numeroUnico: string; descripcion: string | null; estado: string }
  onGuardado: () => void
  onCerrar: () => void
}) {
  const [paso, setPaso] = useState<'editar' | 'confirmar'>('editar')
  const [numeroUnico, setNumeroUnico] = useState(rotulo.numeroUnico)
  const [descripcion, setDescripcion] = useState(rotulo.descripcion ?? '')
  const [estado, setEstado] = useState(rotulo.estado)
  const [motivo, setMotivo] = useState('')
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  const cambios = [
    numeroUnico.trim() !== rotulo.numeroUnico && { campo: 'Número', antes: rotulo.numeroUnico, despues: numeroUnico.trim() },
    descripcion.trim() !== (rotulo.descripcion ?? '') && { campo: 'Descripción', antes: rotulo.descripcion || '—', despues: descripcion.trim() || '—' },
    estado !== rotulo.estado && { campo: 'Estado', antes: rotulo.estado, despues: estado },
  ].filter((c): c is { campo: string; antes: string; despues: string } => Boolean(c))

  function continuar() {
    setError('')
    if (cambios.length === 0) { setError('No hay cambios para guardar.'); return }
    if (!numeroUnico.trim()) { setError('El número de rótulo no puede quedar vacío.'); return }
    setPaso('confirmar')
  }

  async function confirmar() {
    setGuardando(true)
    setError('')
    try {
      const res = await apiFetch(`/api/sses/${idSse}/rotulo`, {
        method: 'PUT',
        body: JSON.stringify({
          numeroUnico: numeroUnico.trim(),
          descripcion: descripcion.trim() || null,
          estado,
          motivo: motivo.trim()
        })
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'Error al guardar la corrección.')
        setPaso('editar')
        return
      }
      onGuardado()
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className={styles.overlay} onClick={onCerrar}>
      <div className={styles.modal} onClick={e => e.stopPropagation()}>
        <div className={styles.modalHeader}>
          <h2>Corregir Rótulo Interno</h2>
          <button className={styles.btnCerrar} onClick={onCerrar}>✕</button>
        </div>

        {paso === 'editar' ? (
          <>
            <div className={styles.modalBody}>
              <p className={styles.inputHint}>
                Esta es una operación crítica: un error en el número de rótulo puede generar inconsistencias en el historial. Todo cambio queda auditado.
              </p>

              <label className={styles.formLabel}>Número de rótulo</label>
              <input
                className={styles.formInput}
                value={numeroUnico}
                onChange={e => setNumeroUnico(e.target.value)}
              />

              <label className={styles.formLabel}>Descripción / observaciones</label>
              <textarea
                className={styles.formTextarea}
                rows={3}
                value={descripcion}
                onChange={e => setDescripcion(e.target.value)}
              />

              <label className={styles.formLabel}>Estado</label>
              <select className={styles.formInput} value={estado} onChange={e => setEstado(e.target.value)}>
                {ESTADOS_ROTULO.map(e => <option key={e} value={e}>{e}</option>)}
              </select>

              <label className={styles.formLabel}>Motivo de la corrección *</label>
              <textarea
                className={styles.formTextarea}
                rows={2}
                value={motivo}
                onChange={e => setMotivo(e.target.value)}
                placeholder="Explicá por qué se corrige este rótulo..."
              />
              {error && <p className={styles.error}>{error}</p>}
            </div>
            <div className={styles.modalFooter}>
              <button className={styles.btnSecundario} onClick={onCerrar}>Cancelar</button>
              <button className={styles.btnPrimario} onClick={continuar} disabled={!motivo.trim()}>
                Revisar cambios
              </button>
            </div>
          </>
        ) : (
          <>
            <div className={styles.modalBody}>
              <p className={styles.inputHint}>Confirmá que estos cambios son correctos antes de guardarlos.</p>
              {cambios.map(c => (
                <div key={c.campo} className={styles.confirmacionCambio}>
                  <span className={styles.confirmacionCampo}>{c.campo}</span>
                  <span><s>{c.antes}</s> → <strong>{c.despues}</strong></span>
                </div>
              ))}
              <p className={styles.confirmacionMotivo}>Motivo: "{motivo.trim()}"</p>
              {error && <p className={styles.error}>{error}</p>}
            </div>
            <div className={styles.modalFooter}>
              <button className={styles.btnSecundario} onClick={() => setPaso('editar')} disabled={guardando}>Volver</button>
              <button className={styles.btnAccionRojo} onClick={confirmar} disabled={guardando}>
                {guardando ? 'Guardando...' : 'Confirmar cambio'}
              </button>
            </div>
          </>
        )}
      </div>
    </div>
  )
}

// ─── Modal: Registrar Muestra ─────────────────────────────────────────────────

const CONDICIONES_MUESTRA: { value: string; label: string }[] = [
  { value: 'apta', label: 'En condiciones' },
  { value: 'no_apta_procesar', label: 'No en condiciones, pero el cliente pide procesarla igual' },
  { value: 'rechazada', label: 'No en condiciones — se rechaza' },
]

function ModalMuestra({ idSse, onGuardado, onCerrar }: {
  idSse: number
  onGuardado: () => void
  onCerrar: () => void
}) {
  const [tipo, setTipo] = useState('')
  const [observacion, setObservacion] = useState('')
  const [condicion, setCondicion] = useState('apta')
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  async function guardar() {
    setGuardando(true)
    setError('')
    if (condicion !== 'apta' && !observacion.trim()) {
      setError('Indicá el motivo en las observaciones.')
      setGuardando(false)
      return
    }
    try {
      const res = await apiFetch(`/api/sses/${idSse}/muestra`, {
        method: 'POST',
        body: JSON.stringify({ tipo: tipo || null, observacion: observacion || null, condicion })
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'Error al registrar la muestra.')
        return
      }
      onGuardado()
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className={styles.overlay} onClick={onCerrar}>
      <div className={styles.modal} onClick={e => e.stopPropagation()}>
        <div className={styles.modalHeader}>
          <h2>Registrar Muestra</h2>
          <button className={styles.btnCerrar} onClick={onCerrar}>✕</button>
        </div>
        <div className={styles.modalBody}>
          <label className={styles.formLabel}>Tipo de muestra</label>
          <input
            className={styles.formInput}
            placeholder="Ej: Alimento sólido, Agua potable, Cosmético..."
            value={tipo}
            onChange={e => setTipo(e.target.value)}
          />

          <label className={styles.formLabel}>Condición de la muestra</label>
          <select className={styles.formInput} value={condicion} onChange={e => setCondicion(e.target.value)}>
            {CONDICIONES_MUESTRA.map(c => <option key={c.value} value={c.value}>{c.label}</option>)}
          </select>
          {condicion === 'rechazada' && (
            <p className={styles.inputHint}>
              Al rechazarla, la SSE se cierra como rechazada y no continúa el proceso de análisis.
            </p>
          )}

          <label className={styles.formLabel}>Observaciones{condicion !== 'apta' ? ' *' : ''}</label>
          <textarea
            className={styles.formTextarea}
            rows={3}
            value={observacion}
            onChange={e => setObservacion(e.target.value)}
            placeholder="Condición de llegada, temperatura, cantidad, etc."
          />
          {error && <p className={styles.error}>{error}</p>}
        </div>
        <div className={styles.modalFooter}>
          <button className={styles.btnSecundario} onClick={onCerrar}>Cancelar</button>
          <button
            className={condicion === 'rechazada' ? styles.btnAccionRojo : styles.btnPrimario}
            onClick={guardar}
            disabled={guardando}
          >
            {guardando ? 'Registrando...' : condicion === 'rechazada' ? 'Rechazar y cerrar SSE' : 'Registrar muestra'}
          </button>
        </div>
      </div>
    </div>
  )
}
