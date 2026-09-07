import { useEffect, useState, useCallback } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './ConsultasPage.module.css'

// ─── Tipos ────────────────────────────────────────────────────────────────────

interface ConsultaResumen {
  idConsulta: number
  codigo: string | null
  canal: string
  estado: string
  descripcion: string | null
  fecha: string
  createdAt: string
  cliente: { idCliente: number; nombre: string; apellido: string; email: string | null } | null
  empresa: { idEmpresa: number; razonSocial: string } | null
  responsable: { idUsuario: number; nombre: string; apellido: string } | null
  tienePresupuesto: boolean
  estadoPresupuesto: string | null
  diasSinResponder: number
  pendiente: boolean
}

interface ItemPresupuesto {
  id: number
  idAnalisis: number
  precioUsdSnapshot: number
  codigo: string
  nombre: string
  area: string
  grupoNombre: string
}

interface PresupuestoDetalle {
  idPresupuesto: number
  codigoRpo: string | null
  estado: string
  importeDolares: number
  importePesos: number
  cotizacion: number
  adicionalAsesoramiento: number
  tipoAsesoramiento: string | null
  fecha: string
  items: ItemPresupuesto[]
}

interface ConsultaDetalle {
  idConsulta: number
  codigo: string | null
  canal: string
  estado: string
  descripcion: string | null
  fecha: string
  createdAt: string
  cliente: {
    idCliente: number; nombre: string; apellido: string
    email: string | null; telefono: string | null
    empresa: { idEmpresa: number; razonSocial: string } | null
  } | null
  empresa: { idEmpresa: number; razonSocial: string } | null
  responsable: { idUsuario: number; nombre: string; apellido: string } | null
  presupuesto: PresupuestoDetalle | null
}

interface GrupoAnalisis {
  idGrupo: number
  codigoAgrupador: string
  descripcion: string | null
  area: string
  items: { idAnalisis: number; codigo: string; nombre: string; area: string; precioUsd: number }[]
}

// ─── Constantes ───────────────────────────────────────────────────────────────

/** Debe coincidir con DiasParaPendiente en ConsultasController.cs */
const DIAS_PENDIENTE = 2

const TIPOS_ASESORAMIENTO: { value: string; label: string }[] = [
  { value: 'consulta', label: 'Solo consulta' },
  { value: 'terreno', label: 'Visita de terreno' },
  { value: 'proceso_completo', label: 'Proceso completo (consulta + muestreo + entrega)' },
]

const TIPO_ASESORAMIENTO_LABEL: Record<string, string> =
  Object.fromEntries(TIPOS_ASESORAMIENTO.map(t => [t.value, t.label]))

const ESTADOS_CONSULTA = ['recibida', 'respondida', 'aceptada', 'tercerizada', 'eliminada']

const BADGE_ESTADO: Record<string, string> = {
  recibida: styles.badgeRecibida,
  respondida: styles.badgeRespondida,
  aceptada: styles.badgeAceptada,
  aceptado: styles.badgeAceptada,
  tercerizada: styles.badgeTercerizada,
  eliminada: styles.badgeRechazada,
  rechazado: styles.badgeRechazada,
  borrador: styles.badgeBorrador,
  enviado: styles.badgeEnviado,
}

/* Transiciones manuales de estado disponibles desde el panel de detalle */
const TRANSICIONES_CONSULTA: Record<string, { label: string; siguienteEstado: string }[]> = {
  recibida: [
    { label: 'Marcar como respondida', siguienteEstado: 'respondida' },
    { label: 'Tercerizar', siguienteEstado: 'tercerizada' },
    { label: 'Eliminar', siguienteEstado: 'eliminada' },
  ],
  respondida: [
    { label: 'Tercerizar', siguienteEstado: 'tercerizada' },
    { label: 'Eliminar', siguienteEstado: 'eliminada' },
  ],
  aceptada: [],
  tercerizada: [{ label: 'Reabrir como recibida', siguienteEstado: 'recibida' }],
  eliminada: [{ label: 'Reabrir como recibida', siguienteEstado: 'recibida' }],
}

// ─── Componente principal ─────────────────────────────────────────────────────

export default function ConsultasPage() {
  const [consultas, setConsultas] = useState<ConsultaResumen[]>([])
  const [total, setTotal] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [filtroEstado, setFiltroEstado] = useState('')
  const [cargando, setCargando] = useState(true)

  const [seleccionada, setSeleccionada] = useState<ConsultaDetalle | null>(null)
  const [cargandoDetalle, setCargandoDetalle] = useState(false)

  const [modalNueva, setModalNueva] = useState(false)
  const [mostrarPresupuesto, setMostrarPresupuesto] = useState(false)
  const [cambiandoEstado, setCambiandoEstado] = useState(false)
  const [totalPendientes, setTotalPendientes] = useState(0)

  const POR_PAGINA = 20

  const cargarLista = useCallback(async () => {
    setCargando(true)
    try {
      const params = new URLSearchParams({ pagina: String(pagina), porPagina: String(POR_PAGINA) })
      if (busqueda) params.set('busqueda', busqueda)
      if (filtroEstado) params.set('estado', filtroEstado)
      const res = await apiFetch(`/api/consultas?${params}`)
      const data = await res.json()
      setConsultas(data.items)
      setTotal(data.total)
    } finally {
      setCargando(false)
    }
  }, [pagina, busqueda, filtroEstado])

  useEffect(() => { cargarLista() }, [cargarLista])

  useEffect(() => {
    apiFetch('/api/consultas?estado=pendiente&porPagina=1')
      .then(r => r.json())
      .then(d => setTotalPendientes(d.total))
      .catch(() => {})
  }, [cargarLista])

  async function abrirDetalle(id: number) {
    setSeleccionada(null)
    setMostrarPresupuesto(false)
    setCargandoDetalle(true)
    const res = await apiFetch(`/api/consultas/${id}`)
    const data = await res.json()
    setSeleccionada(data)
    setCargandoDetalle(false)
  }

  function cerrarDetalle() {
    setSeleccionada(null)
    setMostrarPresupuesto(false)
  }

  async function onPresupuestoGuardado() {
    setMostrarPresupuesto(false)
    if (seleccionada) await abrirDetalle(seleccionada.idConsulta)
    cargarLista()
  }

  async function cambiarEstado(id: number, nuevoEstado: string) {
    setCambiandoEstado(true)
    try {
      await apiFetch(`/api/consultas/${id}`, { method: 'PUT', body: JSON.stringify({ estado: nuevoEstado }) })
      await abrirDetalle(id)
      await cargarLista()
    } finally {
      setCambiandoEstado(false)
    }
  }

  return (
    <div className={styles.root}>
      {/* ── Panel izquierdo: lista ─── */}
      <div className={styles.panelLista}>
        <div className={styles.listaHeader}>
          <h1 className={styles.titulo}>Consultas</h1>
          <button className={styles.btnPrimario} onClick={() => setModalNueva(true)}>+ Nueva consulta</button>
        </div>

        {totalPendientes > 0 && (
          <button
            className={`${styles.avisoPendientes} ${filtroEstado === 'pendiente' ? styles.avisoPendientesActivo : ''}`}
            onClick={() => { setFiltroEstado(f => f === 'pendiente' ? '' : 'pendiente'); setPagina(1) }}
          >
            ⏱ {totalPendientes} consulta{totalPendientes === 1 ? '' : 's'} sin responder hace {DIAS_PENDIENTE}+ días
          </button>
        )}

        <div className={styles.filtros}>
          <input
            className={styles.inputBusqueda}
            placeholder="Buscar por código, cliente, empresa..."
            value={busqueda}
            onChange={e => { setBusqueda(e.target.value); setPagina(1) }}
          />
          <select
            className={styles.selectEstado}
            value={filtroEstado}
            onChange={e => { setFiltroEstado(e.target.value); setPagina(1) }}
          >
            <option value="">Todos los estados</option>
            <option value="pendiente">⏱ Pendientes (sin responder)</option>
            {ESTADOS_CONSULTA.map(e => <option key={e} value={e}>{e}</option>)}
          </select>
        </div>

        {cargando ? (
          <div className={styles.cargando}>Cargando...</div>
        ) : consultas.length === 0 ? (
          <div className={styles.vacio}>No hay consultas{busqueda || filtroEstado ? ' que coincidan' : ''}.</div>
        ) : (
          <div className={styles.tabla}>
            {consultas.map(c => (
              <div
                key={c.idConsulta}
                className={`${styles.fila} ${seleccionada?.idConsulta === c.idConsulta ? styles.filaActiva : ''}`}
                onClick={() => abrirDetalle(c.idConsulta)}
              >
                <div className={styles.filaCodigo}>
                  <span className={styles.codigo}>{c.codigo ?? '—'}</span>
                  <span className={`${styles.badge} ${BADGE_ESTADO[c.estado] ?? ''}`}>{c.estado}</span>
                  {c.pendiente && (
                    <span className={styles.badgePendiente} title="Sin responder">
                      ⏱ {c.diasSinResponder}d
                    </span>
                  )}
                </div>
                <div className={styles.filaCliente}>
                  {c.cliente
                    ? `${c.cliente.nombre} ${c.cliente.apellido}`
                    : c.empresa?.razonSocial ?? <span className={styles.sinCliente}>Sin cliente</span>}
                </div>
                <div className={styles.filaInfo}>
                  <span>{c.canal}</span>
                  <span>{new Date(c.createdAt).toLocaleDateString('es-AR')}</span>
                  {c.tienePresupuesto && (
                    <span className={`${styles.badge} ${BADGE_ESTADO[c.estadoPresupuesto ?? ''] ?? styles.badgeBorrador}`}>
                      RPO: {c.estadoPresupuesto}
                    </span>
                  )}
                </div>
              </div>
            ))}
          </div>
        )}

        {total > POR_PAGINA && (
          <div className={styles.paginacion}>
            <button disabled={pagina === 1} onClick={() => setPagina(p => p - 1)}>Anterior</button>
            <span>Pág. {pagina} de {Math.ceil(total / POR_PAGINA)}</span>
            <button disabled={pagina * POR_PAGINA >= total} onClick={() => setPagina(p => p + 1)}>Siguiente</button>
          </div>
        )}
      </div>

      {/* ── Panel derecho: detalle ─── */}
      {(seleccionada !== null || cargandoDetalle) && (
        <div className={styles.panelDetalle}>
          {cargandoDetalle ? (
            <div className={styles.cargando}>Cargando...</div>
          ) : seleccionada && (
            <>
              <div className={styles.detalleHeader}>
                <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                  <span className={styles.detallecodigo}>{seleccionada.codigo ?? '—'}</span>
                  <span className={`${styles.badge} ${BADGE_ESTADO[seleccionada.estado] ?? ''}`}>{seleccionada.estado}</span>
                </div>
                <button className={styles.btnCerrar} onClick={cerrarDetalle}>✕</button>
              </div>

              <div className={styles.detalleBody}>
                {(TRANSICIONES_CONSULTA[seleccionada.estado] ?? []).length > 0 && (
                  <div className={styles.accionesEstado}>
                    {TRANSICIONES_CONSULTA[seleccionada.estado].map(t => (
                      <button
                        key={t.siguienteEstado}
                        className={styles.btnSecundario}
                        disabled={cambiandoEstado}
                        onClick={() => cambiarEstado(seleccionada.idConsulta, t.siguienteEstado)}
                      >
                        {t.label}
                      </button>
                    ))}
                  </div>
                )}

                <Section titulo="Datos de la consulta">
                  <Campo label="Canal" valor={seleccionada.canal} />
                  <Campo label="Fecha" valor={new Date(seleccionada.fecha).toLocaleDateString('es-AR')} />
                  {seleccionada.descripcion && <Campo label="Descripción" valor={seleccionada.descripcion} />}
                </Section>

                <Section titulo="Cliente / Empresa">
                  {seleccionada.cliente ? (
                    <>
                      <Campo label="Nombre" valor={`${seleccionada.cliente.nombre} ${seleccionada.cliente.apellido}`} />
                      {seleccionada.cliente.email && <Campo label="Email" valor={seleccionada.cliente.email} />}
                      {seleccionada.cliente.telefono && <Campo label="Teléfono" valor={seleccionada.cliente.telefono} />}
                      {seleccionada.cliente.empresa && (
                        <Campo label="Empresa" valor={seleccionada.cliente.empresa.razonSocial} />
                      )}
                    </>
                  ) : seleccionada.empresa ? (
                    <Campo label="Empresa" valor={seleccionada.empresa.razonSocial} />
                  ) : (
                    <p className={styles.sinCliente}>Sin cliente asignado</p>
                  )}
                </Section>

                <Section titulo="Presupuesto básico">
                  {seleccionada.presupuesto ? (
                    <div className={styles.presupuestoResumen}>
                      <div className={styles.presupuestoHeader}>
                        <span className={styles.codigo}>{seleccionada.presupuesto.codigoRpo}</span>
                        <span className={`${styles.badge} ${BADGE_ESTADO[seleccionada.presupuesto.estado] ?? ''}`}>
                          {seleccionada.presupuesto.estado}
                        </span>
                        <button className={styles.btnSecundario} onClick={() => setMostrarPresupuesto(true)}>
                          Editar
                        </button>
                      </div>
                      <div className={styles.presupuestoTotales}>
                        <div className={styles.totalCard}>
                          <span>Total USD</span>
                          <strong>USD {seleccionada.presupuesto.importeDolares.toFixed(2)}</strong>
                        </div>
                        <div className={styles.totalCard}>
                          <span>Cotización</span>
                          <strong>$ {seleccionada.presupuesto.cotizacion.toFixed(0)}</strong>
                        </div>
                        <div className={styles.totalCard}>
                          <span>Total ARS</span>
                          <strong>$ {seleccionada.presupuesto.importePesos.toFixed(2)}</strong>
                        </div>
                      </div>
                      <div className={styles.analisisList}>
                        {seleccionada.presupuesto.items.map(i => (
                          <div key={i.id} className={styles.analisisItem}>
                            <span className={`${styles.areaTag} ${i.area === 'MIC' ? styles.areaMic : styles.areaFq}`}>{i.area}</span>
                            <span className={styles.analisisNombre}>{i.nombre}</span>
                            <span className={styles.analisisPrecio}>USD {i.precioUsdSnapshot.toFixed(2)}</span>
                          </div>
                        ))}
                        {seleccionada.presupuesto.adicionalAsesoramiento > 0 && (
                          <div className={styles.analisisItem}>
                            <span className={styles.analisisNombre}>
                              Asesoramiento — {TIPO_ASESORAMIENTO_LABEL[seleccionada.presupuesto.tipoAsesoramiento ?? ''] ?? seleccionada.presupuesto.tipoAsesoramiento}
                            </span>
                            <span className={styles.analisisPrecio}>USD {seleccionada.presupuesto.adicionalAsesoramiento.toFixed(2)}</span>
                          </div>
                        )}
                      </div>
                    </div>
                  ) : (
                    <div className={styles.sinPresupuesto}>
                      <p>No hay presupuesto para esta consulta.</p>
                      <button className={styles.btnPrimario} onClick={() => setMostrarPresupuesto(true)}>
                        Crear presupuesto
                      </button>
                    </div>
                  )}
                </Section>
              </div>
            </>
          )}
        </div>
      )}

      {/* ── Modal: nuevo presupuesto / editar ─── */}
      {mostrarPresupuesto && seleccionada && (
        <PresupuestoModal
          consulta={seleccionada}
          onGuardado={onPresupuestoGuardado}
          onCerrar={() => setMostrarPresupuesto(false)}
        />
      )}

      {/* ── Modal: nueva consulta ─── */}
      {modalNueva && (
        <NuevaConsultaModal
          onCreada={async (id) => {
            setModalNueva(false)
            await cargarLista()
            await abrirDetalle(id)
          }}
          onCerrar={() => setModalNueva(false)}
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
      <span className={styles.campoValor}>{valor}</span>
    </div>
  )
}

// ─── Modal: Nueva consulta ────────────────────────────────────────────────────

function NuevaConsultaModal({ onCreada, onCerrar }: {
  onCreada: (id: number) => void
  onCerrar: () => void
}) {
  const [canal, setCanal] = useState('mail')
  const [descripcion, setDescripcion] = useState('')
  const [idCliente, setIdCliente] = useState<number | ''>('')
  const [idEmpresa, setIdEmpresa] = useState<number | ''>('')
  const [clientes, setClientes] = useState<{ idCliente: number; nombre: string; apellido: string; idEmpresa: number | null }[]>([])
  const [empresas, setEmpresas] = useState<{ idEmpresa: number; razonSocial: string }[]>([])
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    apiFetch('/api/clientes').then(r => r.json()).then(setClientes)
    apiFetch('/api/empresas').then(r => r.json()).then(setEmpresas)
  }, [])

  function onClienteChange(id: number | '') {
    setIdCliente(id)
    if (id === '') return
    // Si el cliente tiene una empresa asociada, la preseleccionamos (se puede cambiar igual)
    const c = clientes.find(x => x.idCliente === id)
    if (c?.idEmpresa) setIdEmpresa(c.idEmpresa)
  }

  async function guardar() {
    setGuardando(true)
    setError('')
    try {
      const res = await apiFetch('/api/consultas', {
        method: 'POST',
        body: JSON.stringify({
          canal,
          descripcion: descripcion || null,
          idCliente: idCliente || null,
          idEmpresa: idEmpresa || null
        })
      })
      if (!res.ok) { setError('Error al crear la consulta'); return }
      const data = await res.json()
      onCreada(data.idConsulta)
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className={styles.overlay} onClick={onCerrar}>
      <div className={styles.modal} onClick={e => e.stopPropagation()}>
        <div className={styles.modalHeader}>
          <h2>Nueva consulta</h2>
          <button className={styles.btnCerrar} onClick={onCerrar}>✕</button>
        </div>
        <div className={styles.modalBody}>
          <label className={styles.formLabel}>Canal de ingreso</label>
          <select className={styles.formInput} value={canal} onChange={e => setCanal(e.target.value)}>
            <option value="mail">Mail</option>
            <option value="whatsapp">WhatsApp</option>
            <option value="telefono">Teléfono</option>
            <option value="presencial">Presencial</option>
          </select>

          <label className={styles.formLabel}>Cliente (opcional)</label>
          <select
            className={styles.formInput}
            value={idCliente}
            onChange={e => onClienteChange(e.target.value ? Number(e.target.value) : '')}
          >
            <option value="">— Sin cliente asignado —</option>
            {clientes.map(c => (
              <option key={c.idCliente} value={c.idCliente}>{c.apellido}, {c.nombre}</option>
            ))}
          </select>

          <label className={styles.formLabel}>Empresa responsable (opcional)</label>
          <select
            className={styles.formInput}
            value={idEmpresa}
            onChange={e => setIdEmpresa(e.target.value ? Number(e.target.value) : '')}
          >
            <option value="">— Sin empresa asignada —</option>
            {empresas.map(e => (
              <option key={e.idEmpresa} value={e.idEmpresa}>{e.razonSocial}</option>
            ))}
          </select>

          <label className={styles.formLabel}>Descripción / notas iniciales</label>
          <textarea
            className={styles.formTextarea}
            rows={4}
            value={descripcion}
            onChange={e => setDescripcion(e.target.value)}
            placeholder="Describe la solicitud del cliente..."
          />

          {error && <p className={styles.error}>{error}</p>}
        </div>
        <div className={styles.modalFooter}>
          <button className={styles.btnSecundario} onClick={onCerrar}>Cancelar</button>
          <button className={styles.btnPrimario} onClick={guardar} disabled={guardando}>
            {guardando ? 'Guardando...' : 'Crear consulta'}
          </button>
        </div>
      </div>
    </div>
  )
}

// ─── Modal: Presupuesto ───────────────────────────────────────────────────────

function PresupuestoModal({ consulta, onGuardado, onCerrar }: {
  consulta: ConsultaDetalle
  onGuardado: () => void
  onCerrar: () => void
}) {
  const [grupos, setGrupos] = useState<GrupoAnalisis[]>([])
  const [seleccionados, setSeleccionados] = useState<Set<number>>(new Set())
  // Se guardan como texto y arrancan vacíos (no en "0") para que el usuario pueda
  // escribir libremente sin que un cero inicial quede pegado a lo que va tipeando.
  const [cotizacion, setCotizacion] = useState(String(consulta.presupuesto?.cotizacion ?? 1300))
  const [adicional, setAdicional] = useState(consulta.presupuesto?.adicionalAsesoramiento ? String(consulta.presupuesto.adicionalAsesoramiento) : '')
  const cotizacionNum = parseFloat(cotizacion) || 1
  const adicionalNum = parseFloat(adicional) || 0
  const [tipoAsesoramiento, setTipoAsesoramiento] = useState(consulta.presupuesto?.tipoAsesoramiento ?? 'consulta')
  const [cargando, setCargando] = useState(true)
  const [guardando, setGuardando] = useState(false)
  const [filtroArea, setFiltroArea] = useState<'todos' | 'MIC' | 'FQ'>('todos')

  useEffect(() => {
    apiFetch('/api/analisis').then(r => r.json()).then((data: GrupoAnalisis[]) => {
      setGrupos(data)
      if (consulta.presupuesto) {
        setSeleccionados(new Set(consulta.presupuesto.items.map(i => i.idAnalisis)))
      }
      setCargando(false)
    })
  }, [consulta.presupuesto])

  function toggleAnalisis(idAnalisis: number) {
    setSeleccionados(prev => {
      const next = new Set(prev)
      if (next.has(idAnalisis)) next.delete(idAnalisis)
      else next.add(idAnalisis)
      return next
    })
  }

  const analisisMap = grupos.flatMap(g => g.items).reduce<Record<number, GrupoAnalisis['items'][0]>>((acc, a) => {
    acc[a.idAnalisis] = a
    return acc
  }, {})

  const subtotalAnalisis = Array.from(seleccionados).reduce((sum, id) => sum + (analisisMap[id]?.precioUsd ?? 0), 0)
  const totalUsd = subtotalAnalisis + adicionalNum
  const totalArs = totalUsd * cotizacionNum

  const gruposFiltrados = grupos.filter(g => filtroArea === 'todos' || g.area === filtroArea)

  async function guardar() {
    setGuardando(true)
    try {
      const res = await apiFetch('/api/presupuestos', {
        method: 'POST',
        body: JSON.stringify({
          idConsulta: consulta.idConsulta,
          items: Array.from(seleccionados).map(id => ({ idAnalisis: id })),
          cotizacion: cotizacionNum,
          adicionalAsesoramiento: adicionalNum,
          tipoAsesoramiento: adicionalNum > 0 ? tipoAsesoramiento : null
        })
      })
      if (res.ok) onGuardado()
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className={styles.overlay} onClick={onCerrar}>
      <div className={`${styles.modal} ${styles.modalGrande}`} onClick={e => e.stopPropagation()}>
        <div className={styles.modalHeader}>
          <h2>Presupuesto básico — {consulta.codigo}</h2>
          <button className={styles.btnCerrar} onClick={onCerrar}>✕</button>
        </div>

        <div className={styles.presupuestoLayout}>
          {/* Selector de análisis */}
          <div className={styles.selectorPanel}>
            <div className={styles.selectorFiltros}>
              {(['todos', 'MIC', 'FQ'] as const).map(a => (
                <button
                  key={a}
                  className={`${styles.tabArea} ${filtroArea === a ? styles.tabAreaActivo : ''}`}
                  onClick={() => setFiltroArea(a)}
                >
                  {a === 'todos' ? 'Todos' : a}
                </button>
              ))}
            </div>

            {cargando ? (
              <div className={styles.cargando}>Cargando análisis...</div>
            ) : (
              <div className={styles.gruposList}>
                {gruposFiltrados.map(grupo => (
                  <div key={grupo.idGrupo} className={styles.grupoBloque}>
                    <div className={styles.grupoTitulo}>
                      <span className={`${styles.areaTag} ${grupo.area === 'MIC' ? styles.areaMic : styles.areaFq}`}>
                        {grupo.area}
                      </span>
                      {grupo.codigoAgrupador}
                    </div>
                    {grupo.items.map(item => (
                      <label key={item.idAnalisis} className={styles.checkItem}>
                        <input
                          type="checkbox"
                          checked={seleccionados.has(item.idAnalisis)}
                          onChange={() => toggleAnalisis(item.idAnalisis)}
                        />
                        <span className={styles.checkNombre}>{item.nombre}</span>
                        <span className={styles.checkPrecio}>USD {item.precioUsd.toFixed(2)}</span>
                      </label>
                    ))}
                  </div>
                ))}
                {gruposFiltrados.length === 0 && (
                  <p className={styles.vacio}>Sin análisis cargados en el sistema.</p>
                )}
              </div>
            )}
          </div>

          {/* Resumen y cotización */}
          <div className={styles.resumenPanel}>
            <h3 className={styles.sectionTitulo}>Resumen</h3>

            <div className={styles.seleccionadosList}>
              {seleccionados.size === 0 ? (
                <p className={styles.sinCliente}>Seleccioná análisis de la lista</p>
              ) : (
                Array.from(seleccionados).map(id => {
                  const a = analisisMap[id]
                  if (!a) return null
                  return (
                    <div key={id} className={styles.seleccionadoItem}>
                      <button className={styles.btnRemover} onClick={() => toggleAnalisis(id)}>✕</button>
                      <span className={styles.checkNombre}>{a.nombre}</span>
                      <span className={styles.analisisPrecio}>USD {a.precioUsd.toFixed(2)}</span>
                    </div>
                  )
                })
              )}
            </div>

            <div className={styles.cotizacionForm}>
              <label className={styles.formLabel}>Adicional asesoramiento (USD)</label>
              <input
                type="number"
                className={styles.formInput}
                value={adicional}
                min={0}
                step={0.01}
                placeholder="0.00"
                onChange={e => setAdicional(e.target.value)}
              />

              {adicionalNum > 0 && (
                <>
                  <label className={styles.formLabel}>Tipo de asesoramiento</label>
                  <select
                    className={styles.formInput}
                    value={tipoAsesoramiento}
                    onChange={e => setTipoAsesoramiento(e.target.value)}
                  >
                    {TIPOS_ASESORAMIENTO.map(t => (
                      <option key={t.value} value={t.value}>{t.label}</option>
                    ))}
                  </select>
                </>
              )}

              <label className={styles.formLabel}>Cotización USD → ARS</label>
              <input
                type="number"
                className={styles.formInput}
                value={cotizacion}
                min={1}
                step={1}
                onChange={e => setCotizacion(e.target.value)}
              />
            </div>

            <div className={styles.totalesBox}>
              <div className={styles.totalRow}>
                <span>Subtotal análisis</span>
                <strong>USD {subtotalAnalisis.toFixed(2)}</strong>
              </div>
              {adicionalNum > 0 && (
                <div className={styles.totalRow}>
                  <span>Asesoramiento · {TIPO_ASESORAMIENTO_LABEL[tipoAsesoramiento] ?? tipoAsesoramiento}</span>
                  <strong>USD {adicionalNum.toFixed(2)}</strong>
                </div>
              )}
              <div className={`${styles.totalRow} ${styles.totalDestacado}`}>
                <span>Total USD</span>
                <strong>USD {totalUsd.toFixed(2)}</strong>
              </div>
              <div className={`${styles.totalRow} ${styles.totalDestacado}`}>
                <span>Total ARS</span>
                <strong>$ {totalArs.toFixed(2)}</strong>
              </div>
            </div>

            <button
              className={styles.btnPrimario}
              style={{ width: '100%', marginTop: '16px' }}
              onClick={guardar}
              disabled={guardando || seleccionados.size === 0}
            >
              {guardando ? 'Guardando...' : 'Guardar presupuesto'}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
