import { useEffect, useState } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './ConsultasPage.module.css'

// ─── Tipos ────────────────────────────────────────────────────────────────────

export interface ConsultaResumen {
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

export interface ItemPresupuesto {
  id: number
  idAnalisis: number
  precioUsdSnapshot: number
  codigo: string
  nombre: string
  area: string
  grupoNombre: string
}

export interface ItemPaquetePresupuesto {
  id: number
  idPaquete: number
  precioUsdSnapshot: number
  nombre: string
  area: string
  analisisIncluidos: string[]
}

export interface ItemMetodoPresupuesto {
  id: number
  idMetodo: number
  nombre: string
  descripcion: string | null
  area: string
}

export interface PresupuestoDetalle {
  idPresupuesto: number
  codigoRpo: string | null
  estado: string
  importeDolares: number
  importePesos: number
  cotizacion: number
  adicionalAsesoramiento: number
  tipoAsesoramiento: string | null
  mostrarMoneda: string
  fecha: string
  items: ItemPresupuesto[]
  paquetes: ItemPaquetePresupuesto[]
  metodos: ItemMetodoPresupuesto[]
}

export interface ConsultaDetalle {
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

export interface GrupoAnalisis {
  idGrupo: number
  codigoAgrupador: string
  descripcion: string | null
  area: string
  items: { idAnalisis: number; codigo: string; nombre: string; area: string; precioUsd: number }[]
}

export interface PaqueteCatalogo {
  idPaquete: number
  nombre: string
  area: string
  tipoMuestra: string | null
  norma: string | null
  cantidadMuestra: string | null
  precioUsd: number
  analisis: { idAnalisis: number; codigo: string; nombre: string; area: string; precioUsd: number }[]
}

export interface MetodoCatalogo {
  idMetodo: number
  nombre: string
  descripcion: string | null
  area: string
}

// ─── Constantes ───────────────────────────────────────────────────────────────

/** Debe coincidir con DiasParaPendiente en ConsultasController.cs */
export const DIAS_PENDIENTE = 2

/** Roles con acceso al módulo Presupuestos (debe coincidir con modulos.ts y con
 * el [Authorize(Roles=...)] de PresupuestosController) — determina si se puede
 * armar un presupuesto desde acá adentro. */
export const ROLES_PRESUPUESTOS = ['ROL-01', 'ROL-02', 'ROL-03']

export const TIPOS_ASESORAMIENTO: { value: string; label: string }[] = [
  { value: 'consulta', label: 'Solo consulta' },
  { value: 'terreno', label: 'Visita de terreno' },
  { value: 'proceso_completo', label: 'Proceso completo (consulta + muestreo + entrega)' },
]

export const TIPO_ASESORAMIENTO_LABEL: Record<string, string> =
  Object.fromEntries(TIPOS_ASESORAMIENTO.map(t => [t.value, t.label]))

export const ESTADOS_CONSULTA = ['recibida', 'respondida', 'aceptada', 'tercerizada', 'eliminada']

export const BADGE_ESTADO: Record<string, string> = {
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
export const TRANSICIONES_CONSULTA: Record<string, { label: string; siguienteEstado: string }[]> = {
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

// ─── Modal: Nueva consulta ────────────────────────────────────────────────────

export function NuevaConsultaModal({ onCreada, onCerrar }: {
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

export function PresupuestoModal({ consulta, onGuardado, onCerrar }: {
  consulta: ConsultaDetalle
  onGuardado: () => void
  onCerrar: () => void
}) {
  const [grupos, setGrupos] = useState<GrupoAnalisis[]>([])
  const [paquetes, setPaquetes] = useState<PaqueteCatalogo[]>([])
  const [metodosCatalogo, setMetodosCatalogo] = useState<MetodoCatalogo[]>([])
  const [seleccionados, setSeleccionados] = useState<Set<number>>(new Set())
  const [paquetesSeleccionados, setPaquetesSeleccionados] = useState<Set<number>>(new Set())
  const [metodosSeleccionados, setMetodosSeleccionados] = useState<Set<number>>(new Set())
  // Se guardan como texto y arrancan vacíos (no en "0") para que el usuario pueda
  // escribir libremente sin que un cero inicial quede pegado a lo que va tipeando.
  const [cotizacion, setCotizacion] = useState(String(consulta.presupuesto?.cotizacion ?? 1300))
  const [adicional, setAdicional] = useState(consulta.presupuesto?.adicionalAsesoramiento ? String(consulta.presupuesto.adicionalAsesoramiento) : '')
  const cotizacionNum = parseFloat(cotizacion) || 1
  const adicionalNum = parseFloat(adicional) || 0
  const [tipoAsesoramiento, setTipoAsesoramiento] = useState(consulta.presupuesto?.tipoAsesoramiento ?? 'consulta')
  const [mostrarMoneda, setMostrarMoneda] = useState(consulta.presupuesto?.mostrarMoneda ?? 'ambos')
  const [cargando, setCargando] = useState(true)
  const [guardando, setGuardando] = useState(false)
  const [filtroArea, setFiltroArea] = useState<'todos' | 'MIC' | 'FQ'>('todos')

  useEffect(() => {
    Promise.all([
      apiFetch('/api/analisis').then(r => r.json()),
      apiFetch('/api/analisis/paquetes').then(r => r.json()),
      apiFetch('/api/analisis/metodos').then(r => r.json()),
    ]).then(([gruposData, paquetesData, metodosData]: [GrupoAnalisis[], PaqueteCatalogo[], MetodoCatalogo[]]) => {
      setGrupos(gruposData)
      setPaquetes(paquetesData)
      setMetodosCatalogo(metodosData)
      if (consulta.presupuesto) {
        setSeleccionados(new Set(consulta.presupuesto.items.map(i => i.idAnalisis)))
        setPaquetesSeleccionados(new Set(consulta.presupuesto.paquetes.map(p => p.idPaquete)))
        setMetodosSeleccionados(new Set(consulta.presupuesto.metodos.map(m => m.idMetodo)))
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

  function togglePaquete(idPaquete: number) {
    setPaquetesSeleccionados(prev => {
      const next = new Set(prev)
      if (next.has(idPaquete)) next.delete(idPaquete)
      else next.add(idPaquete)
      return next
    })
  }

  function toggleMetodo(idMetodo: number) {
    setMetodosSeleccionados(prev => {
      const next = new Set(prev)
      if (next.has(idMetodo)) next.delete(idMetodo)
      else next.add(idMetodo)
      return next
    })
  }

  const analisisMap = grupos.flatMap(g => g.items).reduce<Record<number, GrupoAnalisis['items'][0]>>((acc, a) => {
    acc[a.idAnalisis] = a
    return acc
  }, {})

  const paquetesMap = paquetes.reduce<Record<number, PaqueteCatalogo>>((acc, p) => {
    acc[p.idPaquete] = p
    return acc
  }, {})

  const subtotalAnalisis = Array.from(seleccionados).reduce((sum, id) => sum + (analisisMap[id]?.precioUsd ?? 0), 0)
  const subtotalPaquetes = Array.from(paquetesSeleccionados).reduce((sum, id) => sum + (paquetesMap[id]?.precioUsd ?? 0), 0)
  const totalUsd = subtotalAnalisis + subtotalPaquetes + adicionalNum
  const totalArs = totalUsd * cotizacionNum

  const gruposFiltrados = grupos.filter(g => filtroArea === 'todos' || g.area === filtroArea)
  const paquetesFiltrados = paquetes.filter(p => filtroArea === 'todos' || p.area === filtroArea)

  async function guardar() {
    setGuardando(true)
    try {
      const res = await apiFetch('/api/presupuestos', {
        method: 'POST',
        body: JSON.stringify({
          idConsulta: consulta.idConsulta,
          items: Array.from(seleccionados).map(id => ({ idAnalisis: id })),
          paquetes: Array.from(paquetesSeleccionados).map(id => ({ idPaquete: id })),
          cotizacion: cotizacionNum,
          adicionalAsesoramiento: adicionalNum,
          tipoAsesoramiento: adicionalNum > 0 ? tipoAsesoramiento : null,
          mostrarMoneda,
          metodos: Array.from(metodosSeleccionados).map(id => ({ idMetodo: id }))
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
          <div style={{ display: 'flex', alignItems: 'center', gap: 14 }}>
            <a href="/analisis" target="_blank" rel="noopener noreferrer" className={styles.linkCatalogo}>
              Editar catálogo de análisis ↗
            </a>
            <button className={styles.btnCerrar} onClick={onCerrar}>✕</button>
          </div>
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
                {paquetesFiltrados.length > 0 && (
                  <div className={styles.grupoBloque}>
                    <div className={styles.grupoTitulo}>Paquetes</div>
                    {paquetesFiltrados.map(p => (
                      <label key={p.idPaquete} className={styles.checkItem}>
                        <input
                          type="checkbox"
                          checked={paquetesSeleccionados.has(p.idPaquete)}
                          onChange={() => togglePaquete(p.idPaquete)}
                        />
                        <span className={styles.checkNombre}>
                          <span className={`${styles.areaTag} ${p.area === 'MIC' ? styles.areaMic : styles.areaFq}`}>
                            {p.area}
                          </span>
                          {' '}{p.nombre}
                          <div className={styles.checkSubtexto}>Incluye: {p.analisis.map(a => a.nombre).join(', ')}</div>
                        </span>
                        <span className={styles.checkPrecio}>USD {p.precioUsd.toFixed(2)}</span>
                      </label>
                    ))}
                  </div>
                )}
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
              {seleccionados.size === 0 && paquetesSeleccionados.size === 0 ? (
                <p className={styles.sinCliente}>Seleccioná análisis o paquetes de la lista</p>
              ) : (
                <>
                  {Array.from(paquetesSeleccionados).map(id => {
                    const p = paquetesMap[id]
                    if (!p) return null
                    return (
                      <div key={`paquete-${id}`} className={styles.seleccionadoItem}>
                        <button className={styles.btnRemover} onClick={() => togglePaquete(id)}>✕</button>
                        <span className={styles.checkNombre}>{p.nombre} <em style={{ fontStyle: 'normal', color: 'var(--texto-suave)', fontSize: 13 }}>(paquete)</em></span>
                        <span className={styles.analisisPrecio}>USD {p.precioUsd.toFixed(2)}</span>
                      </div>
                    )
                  })}
                  {Array.from(seleccionados).map(id => {
                    const a = analisisMap[id]
                    if (!a) return null
                    return (
                      <div key={id} className={styles.seleccionadoItem}>
                        <button className={styles.btnRemover} onClick={() => toggleAnalisis(id)}>✕</button>
                        <span className={styles.checkNombre}>{a.nombre}</span>
                        <span className={styles.analisisPrecio}>USD {a.precioUsd.toFixed(2)}</span>
                      </div>
                    )
                  })}
                </>
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

              <label className={styles.formLabel}>Valor a mostrar en el PDF</label>
              <select
                className={styles.formInput}
                value={mostrarMoneda}
                onChange={e => setMostrarMoneda(e.target.value)}
              >
                <option value="ambos">Pesos y dólares</option>
                <option value="usd">Solo dólares</option>
                <option value="ars">Solo pesos</option>
              </select>

              <label className={styles.formLabel}>Métodos aplicados</label>
              {metodosCatalogo.length === 0 ? (
                <p className={styles.inputHint}>No hay métodos cargados todavía — se crean desde el catálogo de Análisis.</p>
              ) : (
                <div style={{ border: '1px solid var(--borde)', borderRadius: 6, maxHeight: 160, overflowY: 'auto' }}>
                  {metodosCatalogo.map(m => (
                    <label key={m.idMetodo} className={styles.checkItem}>
                      <input
                        type="checkbox"
                        checked={metodosSeleccionados.has(m.idMetodo)}
                        onChange={() => toggleMetodo(m.idMetodo)}
                      />
                      <span className={styles.checkNombre}>
                        <span className={`${styles.areaTag} ${m.area === 'MIC' ? styles.areaMic : m.area === 'FQ' ? styles.areaFq : ''}`}>
                          {m.area}
                        </span>
                        {' '}{m.nombre}
                      </span>
                    </label>
                  ))}
                </div>
              )}
            </div>

            <div className={styles.totalesBox}>
              {subtotalPaquetes > 0 && (
                <div className={styles.totalRow}>
                  <span>Subtotal paquetes</span>
                  <strong>USD {subtotalPaquetes.toFixed(2)}</strong>
                </div>
              )}
              <div className={styles.totalRow}>
                <span>Subtotal análisis sueltos</span>
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
              disabled={guardando || (seleccionados.size === 0 && paquetesSeleccionados.size === 0)}
            >
              {guardando ? 'Guardando...' : 'Guardar presupuesto'}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
