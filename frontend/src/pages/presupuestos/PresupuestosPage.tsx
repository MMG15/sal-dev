import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { apiFetch } from '../../lib/api'
import styles from './PresupuestosPage.module.css'

// ─── Tipos ────────────────────────────────────────────────────────────────────

interface PresupuestoResumen {
  idPresupuesto: number
  codigoRpo: string | null
  estado: string
  importeDolares: number
  importePesos: number
  cotizacion: number
  fecha: string
  consultaCodigo: string | null
  idConsulta: number
  cliente: { nombre: string; apellido: string; email: string | null } | null
  empresa: { razonSocial: string } | null
  creadoPor: { nombre: string; apellido: string }
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

interface Archivo {
  id: number
  nombreOriginal: string
  tipoMime: string
  tamañoBytes: number
  subidoEn: string
  subidoPor: string
}

interface PresupuestoDetalle {
  idPresupuesto: number
  codigoRpo: string | null
  estado: string
  importeDolares: number
  importePesos: number
  cotizacion: number
  adicionalAsesoramiento: number
  fecha: string
  consulta: {
    idConsulta: number
    codigo: string | null
    canal: string
    descripcion: string | null
    cliente: {
      idCliente: number; nombre: string; apellido: string
      email: string | null; telefono: string | null
      empresa: { idEmpresa: number; razonSocial: string } | null
    } | null
    empresa: { idEmpresa: number; razonSocial: string } | null
  }
  creadoPor: { nombre: string; apellido: string }
  items: ItemPresupuesto[]
}

// ─── Constantes ───────────────────────────────────────────────────────────────

const ESTADOS = ['borrador', 'enviado', 'aceptado', 'rechazado']

const BADGE: Record<string, string> = {
  borrador: styles.badgeBorrador,
  enviado: styles.badgeEnviado,
  aceptado: styles.badgeAceptado,
  rechazado: styles.badgeRechazado,
}

const TRANSICIONES: Record<string, { label: string; siguienteEstado: string; variante: 'primario' | 'verde' | 'rojo' }[]> = {
  borrador:  [{ label: 'Marcar como enviado', siguienteEstado: 'enviado', variante: 'primario' }],
  enviado:   [
    { label: 'Aceptado por el cliente', siguienteEstado: 'aceptado', variante: 'verde' },
    { label: 'Rechazado por el cliente', siguienteEstado: 'rechazado', variante: 'rojo' },
  ],
  aceptado:  [],
  rechazado: [{ label: 'Reabrir como borrador', siguienteEstado: 'borrador', variante: 'primario' }],
}

// ─── Componente principal ─────────────────────────────────────────────────────

export default function PresupuestosPage() {
  const navigate = useNavigate()
  const [lista, setLista] = useState<PresupuestoResumen[]>([])
  const [total, setTotal] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [filtroEstado, setFiltroEstado] = useState('')
  const [cargando, setCargando] = useState(true)

  const [seleccionado, setSeleccionado] = useState<PresupuestoDetalle | null>(null)
  const [cargandoDetalle, setCargandoDetalle] = useState(false)
  const [cambiandoEstado, setCambiandoEstado] = useState(false)
  const [archivos, setArchivos] = useState<Archivo[]>([])
  const [subiendoArchivo, setSubiendoArchivo] = useState(false)
  const [eliminandoArchivo, setEliminandoArchivo] = useState<number | null>(null)

  const POR_PAGINA = 20

  const cargarLista = useCallback(async () => {
    setCargando(true)
    try {
      const params = new URLSearchParams({ pagina: String(pagina), porPagina: String(POR_PAGINA) })
      if (busqueda) params.set('busqueda', busqueda)
      if (filtroEstado) params.set('estado', filtroEstado)
      const res = await apiFetch(`/api/presupuestos?${params}`)
      const data = await res.json()
      setLista(data.items)
      setTotal(data.total)
    } finally {
      setCargando(false)
    }
  }, [pagina, busqueda, filtroEstado])

  useEffect(() => { cargarLista() }, [cargarLista])

  async function abrirDetalle(id: number) {
    setSeleccionado(null)
    setArchivos([])
    setCargandoDetalle(true)
    const [resDetalle, resArchivos] = await Promise.all([
      apiFetch(`/api/presupuestos/${id}`),
      apiFetch(`/api/presupuestos/${id}/archivos`)
    ])
    setSeleccionado(await resDetalle.json())
    setArchivos(await resArchivos.json())
    setCargandoDetalle(false)
  }

  async function cargarArchivos(id: number) {
    const res = await apiFetch(`/api/presupuestos/${id}/archivos`)
    setArchivos(await res.json())
  }

  async function subirArchivo(id: number, file: File) {
    setSubiendoArchivo(true)
    try {
      const form = new FormData()
      form.append('archivo', file)
      await apiFetch(`/api/presupuestos/${id}/archivos`, { method: 'POST', body: form, headers: {} })
      await cargarArchivos(id)
    } finally {
      setSubiendoArchivo(false)
    }
  }

  async function eliminarArchivo(idPresupuesto: number, idArchivo: number) {
    setEliminandoArchivo(idArchivo)
    try {
      await apiFetch(`/api/presupuestos/${idPresupuesto}/archivos/${idArchivo}`, { method: 'DELETE' })
      setArchivos(prev => prev.filter(a => a.id !== idArchivo))
    } finally {
      setEliminandoArchivo(null)
    }
  }

  function descargarArchivo(idPresupuesto: number, idArchivo: number, nombre: string) {
    // Descarga autenticada: obtenemos el blob a través de apiFetch
    apiFetch(`/api/presupuestos/${idPresupuesto}/archivos/${idArchivo}`)
      .then(r => r.blob())
      .then(blob => {
        const url = URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = nombre
        a.click()
        URL.revokeObjectURL(url)
      })
  }

  async function cambiarEstado(id: number, nuevoEstado: string) {
    setCambiandoEstado(true)
    try {
      await apiFetch(`/api/presupuestos/${id}/estado`, {
        method: 'PUT',
        body: JSON.stringify({ estado: nuevoEstado })
      })
      await abrirDetalle(id)
      cargarLista()
    } finally {
      setCambiandoEstado(false)
    }
  }

  async function enviarPorEmail(p: PresupuestoDetalle) {
    // Si está en borrador, marcarlo como enviado antes de abrir el correo
    if (p.estado === 'borrador') {
      await cambiarEstado(p.idPresupuesto, 'enviado')
    }

    const cliente = p.consulta.cliente
    const empresa = p.consulta.empresa ?? p.consulta.cliente?.empresa
    const destinatario = cliente?.email ?? ''

    const nombreDestinatario = cliente
      ? `${cliente.nombre} ${cliente.apellido}`
      : empresa?.razonSocial ?? 'cliente'

    const lineasAnalisis = p.items
      .map(i => `  • [${i.area}] ${i.nombre}: USD ${i.precioUsdSnapshot.toFixed(2)}`)
      .join('\n')

    const lineasArchivos = archivos.length > 0
      ? `\nDocumentos adjuntos en el sistema (${archivos.length}):\n${archivos.map(a => `  • ${a.nombreOriginal}`).join('\n')}\n`
      : ''

    const cuerpo = [
      `Estimado/a ${nombreDestinatario},`,
      '',
      `Nos comunicamos desde el Laboratorio Control de Calidad "Dr. Alberto Graffigna" - UCCuyo`,
      `para hacerle llegar el presupuesto correspondiente a su consulta.`,
      '',
      `─────────────────────────────────────────`,
      `PRESUPUESTO ${p.codigoRpo ?? ''}`,
      `Consulta: ${p.consulta.codigo ?? '—'}`,
      `Fecha: ${new Date(p.fecha).toLocaleDateString('es-AR')}`,
      `─────────────────────────────────────────`,
      '',
      'ANÁLISIS INCLUIDOS:',
      lineasAnalisis || '  (sin análisis)',
      '',
      `Subtotal análisis:       USD ${p.items.reduce((s, i) => s + i.precioUsdSnapshot, 0).toFixed(2)}`,
      p.adicionalAsesoramiento > 0
        ? `Asesoramiento técnico:   USD ${p.adicionalAsesoramiento.toFixed(2)}`
        : '',
      `TOTAL USD:               USD ${p.importeDolares.toFixed(2)}`,
      `Cotización USD/ARS:      $ ${p.cotizacion.toFixed(0)}`,
      `TOTAL ARS:               $ ${p.importePesos.toFixed(2)}`,
      lineasArchivos,
      '─────────────────────────────────────────',
      '',
      'Ante cualquier consulta, no dude en comunicarse.',
      '',
      'Saludos cordiales,',
      'Laboratorio Control de Calidad "Dr. Alberto Graffigna"',
      'UCCuyo — San Juan, Argentina',
    ].filter(l => l !== undefined).join('\n')

    const asunto = `Presupuesto ${p.codigoRpo ?? ''} — Laboratorio Graffigna UCCuyo`

    const mailto = `mailto:${encodeURIComponent(destinatario)}?subject=${encodeURIComponent(asunto)}&body=${encodeURIComponent(cuerpo)}`
    window.location.href = mailto
  }

  const subtotalAnalisis = seleccionado
    ? seleccionado.items.reduce((s, i) => s + i.precioUsdSnapshot, 0)
    : 0

  return (
      <div className={styles.root}>
      {/* ── Lista ── */}
      <div className={styles.panelLista}>
        <div className={styles.listaHeader}>
          <h1 className={styles.titulo}>Presupuestos</h1>
          <div className={styles.listaHeaderAcciones}>
            <span className={styles.totalBadge}>{total} en total</span>
            <button className={styles.linkCatalogo} onClick={() => navigate('/analisis')}>
              Catálogo de análisis →
            </button>
          </div>
        </div>

        <div className={styles.filtros}>
          <input
            className={styles.inputBusqueda}
            placeholder="Código RPO, CF, cliente, empresa..."
            value={busqueda}
            onChange={e => { setBusqueda(e.target.value); setPagina(1) }}
          />
          <select
            className={styles.selectEstado}
            value={filtroEstado}
            onChange={e => { setFiltroEstado(e.target.value); setPagina(1) }}
          >
            <option value="">Todos</option>
            {ESTADOS.map(e => <option key={e} value={e}>{e}</option>)}
          </select>
        </div>

        {cargando ? (
          <div className={styles.cargando}>Cargando...</div>
        ) : lista.length === 0 ? (
          <div className={styles.vacio}>
            {busqueda || filtroEstado
              ? 'Sin resultados para esa búsqueda.'
              : 'No hay presupuestos cargados. Creá uno desde una Consulta.'}
          </div>
        ) : (
          <div className={styles.tabla}>
            {lista.map(p => (
              <div
                key={p.idPresupuesto}
                className={`${styles.fila} ${seleccionado?.idPresupuesto === p.idPresupuesto ? styles.filaActiva : ''}`}
                onClick={() => abrirDetalle(p.idPresupuesto)}
              >
                <div className={styles.filaCabeza}>
                  <span className={styles.codigo}>{p.codigoRpo ?? '—'}</span>
                  <span className={`${styles.badge} ${BADGE[p.estado] ?? ''}`}>{p.estado}</span>
                </div>
                <div className={styles.filaCliente}>
                  {p.cliente
                    ? `${p.cliente.nombre} ${p.cliente.apellido}`
                    : p.empresa?.razonSocial ?? <em className={styles.sinDato}>Sin cliente</em>}
                </div>
                <div className={styles.filaMontos}>
                  <span className={styles.montoUsd}>USD {p.importeDolares.toFixed(2)}</span>
                  <span className={styles.montoDivider}>·</span>
                  <span className={styles.montoArs}>$ {p.importePesos.toFixed(2)}</span>
                  <span className={styles.fechaChip}>{new Date(p.fecha).toLocaleDateString('es-AR')}</span>
                </div>
                <div className={styles.filaConsulta}>
                  <span className={styles.consultaRef}>CF: {p.consultaCodigo ?? '—'}</span>
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
      {(seleccionado !== null || cargandoDetalle) && (
        <div className={styles.panelDetalle}>
          {cargandoDetalle ? (
            <div className={styles.cargando}>Cargando...</div>
          ) : seleccionado && (
            <>
              <div className={styles.detalleHeader}>
                <div className={styles.detalleTitulo}>
                  <span className={styles.codigoGrande}>{seleccionado.codigoRpo ?? '—'}</span>
                  <span className={`${styles.badge} ${BADGE[seleccionado.estado] ?? ''}`}>{seleccionado.estado}</span>
                </div>
                <button className={styles.btnCerrar} onClick={() => setSeleccionado(null)}>✕</button>
              </div>

              <div className={styles.detalleBody}>
                {/* Acciones de estado + envío */}
                <div className={styles.accionesEstado}>
                  {(TRANSICIONES[seleccionado.estado] ?? []).map(t => (
                    <button
                      key={t.siguienteEstado}
                      disabled={cambiandoEstado}
                      className={`${styles.btnAccion} ${styles[`btnAccion_${t.variante}`]}`}
                      onClick={() => cambiarEstado(seleccionado.idPresupuesto, t.siguienteEstado)}
                    >
                      {t.label}
                    </button>
                  ))}
                  {(seleccionado.estado === 'borrador' || seleccionado.estado === 'enviado') && (
                    <button
                      className={`${styles.btnAccion} ${styles.btnAccion_email}`}
                      disabled={cambiandoEstado}
                      onClick={() => enviarPorEmail(seleccionado)}
                      title="Abre tu cliente de correo con el presupuesto completo"
                    >
                      ✉ Enviar por email
                    </button>
                  )}
                </div>

                {/* Totales destacados */}
                <div className={styles.totalesGrid}>
                  <div className={styles.totalCard}>
                    <span>Total USD</span>
                    <strong>USD {seleccionado.importeDolares.toFixed(2)}</strong>
                  </div>
                  <div className={styles.totalCard}>
                    <span>Cotización</span>
                    <strong>$ {seleccionado.cotizacion.toFixed(0)}</strong>
                  </div>
                  <div className={`${styles.totalCard} ${styles.totalCardDestacado}`}>
                    <span>Total ARS</span>
                    <strong>$ {seleccionado.importePesos.toFixed(2)}</strong>
                  </div>
                </div>

                {/* Análisis */}
                <div className={styles.section}>
                  <h3 className={styles.sectionTitulo}>Análisis incluidos</h3>
                  {seleccionado.items.length === 0 ? (
                    <p className={styles.sinDato}>Sin análisis.</p>
                  ) : (
                    <>
                      <div className={styles.analisisList}>
                        {seleccionado.items.map(i => (
                          <div key={i.id} className={styles.analisisItem}>
                            <span className={`${styles.areaTag} ${i.area === 'MIC' ? styles.areaMic : styles.areaFq}`}>{i.area}</span>
                            <span className={styles.analisisNombre}>{i.nombre}</span>
                            <span className={styles.analisisPrecio}>USD {i.precioUsdSnapshot.toFixed(2)}</span>
                          </div>
                        ))}
                      </div>
                      <div className={styles.subtotalesDetalle}>
                        <div className={styles.subtotalRow}>
                          <span>Subtotal análisis</span>
                          <span>USD {subtotalAnalisis.toFixed(2)}</span>
                        </div>
                        {seleccionado.adicionalAsesoramiento > 0 && (
                          <div className={styles.subtotalRow}>
                            <span>Asesoramiento</span>
                            <span>USD {seleccionado.adicionalAsesoramiento.toFixed(2)}</span>
                          </div>
                        )}
                      </div>
                    </>
                  )}
                </div>

                {/* Consulta asociada */}
                <div className={styles.section}>
                  <h3 className={styles.sectionTitulo}>Consulta asociada</h3>
                  <div className={styles.campo}>
                    <span className={styles.campoLabel}>Código</span>
                    <button
                      className={styles.linkBtn}
                      onClick={() => navigate(`/consultas`)}
                      title="Ver en módulo Consultas"
                    >
                      {seleccionado.consulta.codigo ?? '—'}
                    </button>
                  </div>
                  <div className={styles.campo}>
                    <span className={styles.campoLabel}>Canal</span>
                    <span>{seleccionado.consulta.canal}</span>
                  </div>
                  {seleccionado.consulta.descripcion && (
                    <div className={styles.campo}>
                      <span className={styles.campoLabel}>Descripción</span>
                      <span>{seleccionado.consulta.descripcion}</span>
                    </div>
                  )}
                </div>

                {/* Cliente / Empresa */}
                <div className={styles.section}>
                  <h3 className={styles.sectionTitulo}>Cliente / Empresa</h3>
                  {seleccionado.consulta.cliente ? (
                    <>
                      <div className={styles.campo}>
                        <span className={styles.campoLabel}>Nombre</span>
                        <span>{seleccionado.consulta.cliente.nombre} {seleccionado.consulta.cliente.apellido}</span>
                      </div>
                      {seleccionado.consulta.cliente.email && (
                        <div className={styles.campo}>
                          <span className={styles.campoLabel}>Email</span>
                          <span>{seleccionado.consulta.cliente.email}</span>
                        </div>
                      )}
                      {seleccionado.consulta.cliente.telefono && (
                        <div className={styles.campo}>
                          <span className={styles.campoLabel}>Teléfono</span>
                          <span>{seleccionado.consulta.cliente.telefono}</span>
                        </div>
                      )}
                      {seleccionado.consulta.cliente.empresa && (
                        <div className={styles.campo}>
                          <span className={styles.campoLabel}>Empresa</span>
                          <span>{seleccionado.consulta.cliente.empresa.razonSocial}</span>
                        </div>
                      )}
                    </>
                  ) : seleccionado.consulta.empresa ? (
                    <div className={styles.campo}>
                      <span className={styles.campoLabel}>Empresa</span>
                      <span>{seleccionado.consulta.empresa.razonSocial}</span>
                    </div>
                  ) : (
                    <p className={styles.sinDato}>Sin cliente asignado a la consulta.</p>
                  )}
                </div>

                {/* Archivos adjuntos */}
                <div className={styles.section}>
                  <div className={styles.archivosHeader}>
                    <h3 className={styles.sectionTitulo}>Documentos adjuntos</h3>
                    <label className={styles.btnAdjuntar} title="Adjuntar PDF, Word, Excel, imagen...">
                      {subiendoArchivo ? 'Subiendo...' : '+ Adjuntar'}
                      <input
                        type="file"
                        accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg,.zip"
                        style={{ display: 'none' }}
                        disabled={subiendoArchivo}
                        onChange={e => {
                          const file = e.target.files?.[0]
                          if (file) subirArchivo(seleccionado.idPresupuesto, file)
                          e.target.value = ''
                        }}
                      />
                    </label>
                  </div>

                  {archivos.length === 0 ? (
                    <p className={styles.sinDato}>Sin documentos adjuntos.</p>
                  ) : (
                    <div className={styles.archivosList}>
                      {archivos.map(a => (
                        <div key={a.id} className={styles.archivoItem}>
                          <span className={styles.archivoIcono}>{iconoMime(a.tipoMime)}</span>
                          <div className={styles.archivoInfo}>
                            <span className={styles.archivoNombre}>{a.nombreOriginal}</span>
                            <span className={styles.archivoMeta}>
                              {formatBytes(a.tamañoBytes)} · {new Date(a.subidoEn).toLocaleDateString('es-AR')}
                            </span>
                          </div>
                          <button
                            className={styles.btnDescargar}
                            onClick={() => descargarArchivo(seleccionado.idPresupuesto, a.id, a.nombreOriginal)}
                            title="Descargar"
                          >
                            ↓
                          </button>
                          <button
                            className={styles.btnEliminarArchivo}
                            onClick={() => eliminarArchivo(seleccionado.idPresupuesto, a.id)}
                            disabled={eliminandoArchivo === a.id}
                            title="Eliminar"
                          >
                            ✕
                          </button>
                        </div>
                      ))}
                    </div>
                  )}
                </div>

                <div className={styles.metaDato}>
                  Creado por {seleccionado.creadoPor.nombre} {seleccionado.creadoPor.apellido}
                  {' · '}{new Date(seleccionado.fecha).toLocaleDateString('es-AR')}
                </div>
              </div>
            </>
          )}
        </div>
      )}
      </div>
  )
}

// ─── Helpers ──────────────────────────────────────────────────────────────────

function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

function iconoMime(mime: string): string {
  if (mime === 'application/pdf') return '📄'
  if (mime.includes('word') || mime.includes('document')) return '📝'
  if (mime.includes('sheet') || mime.includes('excel')) return '📊'
  if (mime.startsWith('image/')) return '🖼'
  if (mime.includes('zip')) return '🗜'
  return '📎'
}
