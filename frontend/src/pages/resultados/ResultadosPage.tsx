import { useEffect, useState, useCallback } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './ResultadosPage.module.css'

// ─── Tipos ────────────────────────────────────────────────────────────────────

interface MuestraResumen {
  idMuestra: number
  numeroRotulo: string
  codigoSse: string | null
  cliente: { nombre: string; apellido: string } | null
  empresa: { razonSocial: string } | null
  areas: string[]
  total: number
  pendientes: number
  porValidar: number
  validados: number
  fechaRecepcion: string
  estadoGeneral: 'pendiente' | 'por_validar' | 'completo'
}

interface ResultadoDetalle {
  idResultado: number
  valor: string | null
  estado: string
  fechaCarga: string | null
  motivoRechazo: string | null
  cargadoPor: { nombre: string; apellido: string } | null
  validadoPor: { nombre: string; apellido: string } | null
  fechaValidacion: string | null
  analisis: {
    idAnalisis: number
    codigo: string
    nombre: string
    area: string
    unidad: string | null
    rangoMin: number | null
    rangoMax: number | null
    valorEsperado: string | null
    grupoNombre: string
  }
  fueraDeRango: boolean | null
}

interface MuestraDetalle {
  idMuestra: number
  tipo: string | null
  estado: string
  fechaRecepcion: string
  numeroRotulo: string
  codigoSse: string | null
  cliente: { nombre: string; apellido: string } | null
  empresa: { razonSocial: string } | null
  informe: { idInforme: number; codigo: string; fechaGeneracion: string } | null
  todoValidado: boolean
  resultados: ResultadoDetalle[]
}

// ─── Constantes ───────────────────────────────────────────────────────────────

const ESTADO_GENERAL_LABEL: Record<string, string> = {
  pendiente: 'Pendiente de carga',
  por_validar: 'Por validar',
  completo: 'Completo',
}

const ESTADO_GENERAL_BADGE: Record<string, string> = {
  pendiente: styles.badgePendiente,
  por_validar: styles.badgePorValidar,
  completo: styles.badgeCompleto,
}

const ESTADO_RESULTADO_LABEL: Record<string, string> = {
  pendiente: 'Pendiente',
  cargado: 'Cargado',
  validado: 'Validado',
  rechazado: 'Rechazado',
}

const ESTADO_RESULTADO_BADGE: Record<string, string> = {
  pendiente: styles.badgePendiente,
  cargado: styles.badgePorValidar,
  validado: styles.badgeCompleto,
  rechazado: styles.badgeRechazado,
}

function referenciaTexto(a: ResultadoDetalle['analisis']) {
  if (a.valorEsperado) return `Esperado: ${a.valorEsperado}`
  if (a.rangoMin != null || a.rangoMax != null) {
    return `Rango: ${a.rangoMin ?? '—'} a ${a.rangoMax ?? '—'} ${a.unidad ?? ''}`.trim()
  }
  return null
}

// ─── Componente principal ─────────────────────────────────────────────────────

export default function ResultadosPage() {
  const [muestras, setMuestras] = useState<MuestraResumen[]>([])
  const [total, setTotal] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [filtroEstado, setFiltroEstado] = useState('')
  const [filtroArea, setFiltroArea] = useState('')
  const [cargando, setCargando] = useState(true)

  const [seleccionada, setSeleccionada] = useState<MuestraDetalle | null>(null)
  const [cargandoDetalle, setCargandoDetalle] = useState(false)
  const [guardandoInforme, setGuardandoInforme] = useState(false)
  const [errorInforme, setErrorInforme] = useState('')

  const POR_PAGINA = 20

  const cargarLista = useCallback(async () => {
    setCargando(true)
    try {
      const params = new URLSearchParams({ pagina: String(pagina), porPagina: String(POR_PAGINA) })
      if (busqueda) params.set('busqueda', busqueda)
      if (filtroEstado) params.set('estado', filtroEstado)
      if (filtroArea) params.set('area', filtroArea)
      const res = await apiFetch(`/api/resultados/muestras?${params}`)
      const data = await res.json()
      setMuestras(data.items)
      setTotal(data.total)
    } finally {
      setCargando(false)
    }
  }, [pagina, busqueda, filtroEstado, filtroArea])

  useEffect(() => { cargarLista() }, [cargarLista])

  async function abrirDetalle(id: number) {
    setSeleccionada(null)
    setCargandoDetalle(true)
    const res = await apiFetch(`/api/resultados/muestras/${id}`)
    setSeleccionada(await res.json())
    setCargandoDetalle(false)
  }

  async function refrescarDetalle() {
    if (!seleccionada) return
    const res = await apiFetch(`/api/resultados/muestras/${seleccionada.idMuestra}`)
    setSeleccionada(await res.json())
    cargarLista()
  }

  async function guardarInforme() {
    if (!seleccionada) return
    setGuardandoInforme(true)
    setErrorInforme('')
    try {
      const res = await apiFetch(`/api/resultados/muestras/${seleccionada.idMuestra}/informe`, { method: 'POST' })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setErrorInforme(d.error ?? 'Error al guardar el informe.')
        return
      }
      await refrescarDetalle()
    } finally {
      setGuardandoInforme(false)
    }
  }

  function descargarPdf(idInforme: number, codigo: string) {
    apiFetch(`/api/resultados/informes/${idInforme}/pdf`)
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

  return (
    <div className={styles.root}>
      {/* ── Panel izquierdo: lista ─── */}
      <div className={styles.panelLista}>
        <div className={styles.listaHeader}>
          <h1 className={styles.titulo}>Resultados</h1>
          <span className={styles.totalBadge}>{total} muestras</span>
        </div>

        <div className={styles.filtros}>
          <input
            className={styles.inputBusqueda}
            placeholder="Buscar por rótulo, SSE, cliente..."
            value={busqueda}
            onChange={e => { setBusqueda(e.target.value); setPagina(1) }}
          />
          <select
            className={styles.selectFiltro}
            value={filtroEstado}
            onChange={e => { setFiltroEstado(e.target.value); setPagina(1) }}
          >
            <option value="">Todos los estados</option>
            <option value="pendientes">Pendientes de carga</option>
            <option value="por_validar">Por validar</option>
            <option value="completos">Completos</option>
          </select>
          <select
            className={styles.selectFiltro}
            value={filtroArea}
            onChange={e => { setFiltroArea(e.target.value); setPagina(1) }}
          >
            <option value="">Todas las áreas</option>
            <option value="MIC">MIC</option>
            <option value="FQ">FQ</option>
          </select>
        </div>

        {cargando ? (
          <div className={styles.cargando}>Cargando...</div>
        ) : muestras.length === 0 ? (
          <div className={styles.vacio}>No hay muestras{busqueda || filtroEstado || filtroArea ? ' que coincidan' : ' con resultados todavía'}.</div>
        ) : (
          <div className={styles.tabla}>
            {muestras.map(m => (
              <div
                key={m.idMuestra}
                className={`${styles.fila} ${seleccionada?.idMuestra === m.idMuestra ? styles.filaActiva : ''}`}
                onClick={() => abrirDetalle(m.idMuestra)}
              >
                <div className={styles.filaCabeza}>
                  <span className={styles.rotuloNumero}>🏷 {m.numeroRotulo}</span>
                  <span className={`${styles.badge} ${ESTADO_GENERAL_BADGE[m.estadoGeneral]}`}>
                    {ESTADO_GENERAL_LABEL[m.estadoGeneral]}
                  </span>
                </div>
                <div className={styles.filaCliente}>
                  {m.cliente ? `${m.cliente.nombre} ${m.cliente.apellido}` : m.empresa?.razonSocial ?? <span className={styles.sinCliente}>Sin cliente</span>}
                </div>
                <div className={styles.filaInfo}>
                  <span className={styles.codigo}>{m.codigoSse ?? '—'}</span>
                  {m.areas.map(a => (
                    <span key={a} className={`${styles.areaTag} ${a === 'MIC' ? styles.areaMic : styles.areaFq}`}>{a}</span>
                  ))}
                  <span className={styles.avance}>{m.validados}/{m.total} validados</span>
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
                <div className={styles.detalleTitulo}>
                  <span className={styles.rotuloNumeroGrande}>🏷 {seleccionada.numeroRotulo}</span>
                  <span className={styles.codigo}>{seleccionada.codigoSse}</span>
                </div>
                <button className={styles.btnCerrar} onClick={() => setSeleccionada(null)}>✕</button>
              </div>

              <div className={styles.detalleBody}>
                <div className={styles.detalleMeta}>
                  <span>
                    {seleccionada.cliente ? `${seleccionada.cliente.nombre} ${seleccionada.cliente.apellido}` : seleccionada.empresa?.razonSocial ?? 'Sin cliente'}
                  </span>
                  {seleccionada.tipo && <span>· {seleccionada.tipo}</span>}
                  <span>· Recibida {new Date(seleccionada.fechaRecepcion).toLocaleDateString('es-AR')}</span>
                </div>

                {seleccionada.informe ? (
                  <div className={styles.informeBloque}>
                    <div>
                      <span className={styles.informeCodigo}>📄 {seleccionada.informe.codigo}</span>
                      <span className={styles.informeFecha}>
                        Guardado el {new Date(seleccionada.informe.fechaGeneracion).toLocaleDateString('es-AR')}
                      </span>
                    </div>
                    <button
                      className={styles.btnPrimario}
                      onClick={() => descargarPdf(seleccionada.informe!.idInforme, seleccionada.informe!.codigo)}
                    >
                      Descargar PDF
                    </button>
                  </div>
                ) : seleccionada.todoValidado ? (
                  <div className={styles.informeBloque}>
                    <span className={styles.informeFecha}>Todos los análisis están validados. Ya se puede guardar el informe.</span>
                    <button className={styles.btnPrimario} onClick={guardarInforme} disabled={guardandoInforme}>
                      {guardandoInforme ? 'Guardando...' : 'Guardar informe'}
                    </button>
                  </div>
                ) : null}
                {errorInforme && <p className={styles.error}>{errorInforme}</p>}

                {seleccionada.resultados.map(r => (
                  <FilaResultado key={r.idResultado} resultado={r} onCambio={refrescarDetalle} />
                ))}
              </div>
            </>
          )}
        </div>
      )}
    </div>
  )
}

// ─── Fila de resultado individual ──────────────────────────────────────────────

function FilaResultado({ resultado, onCambio }: { resultado: ResultadoDetalle; onCambio: () => void }) {
  const [editando, setEditando] = useState(false)
  const [valor, setValor] = useState(resultado.valor ?? '')
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')
  const [rechazando, setRechazando] = useState(false)
  const [motivoRechazo, setMotivoRechazo] = useState('')

  const puedeEditar = resultado.estado === 'pendiente' || resultado.estado === 'rechazado'
  const referencia = referenciaTexto(resultado.analisis)

  async function guardarValor() {
    if (!valor.trim()) { setError('Ingresá un valor.'); return }
    setGuardando(true)
    setError('')
    try {
      const res = await apiFetch(`/api/resultados/${resultado.idResultado}`, {
        method: 'PUT',
        body: JSON.stringify({ valor: valor.trim() })
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'Error al guardar.')
        return
      }
      setEditando(false)
      onCambio()
    } finally {
      setGuardando(false)
    }
  }

  async function validar(aprobado: boolean) {
    if (!aprobado && !motivoRechazo.trim()) { setError('Indicá el motivo del rechazo.'); return }
    setGuardando(true)
    setError('')
    try {
      const res = await apiFetch(`/api/resultados/${resultado.idResultado}/validar`, {
        method: 'PUT',
        body: JSON.stringify({ aprobado, motivoRechazo: motivoRechazo.trim() || null })
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'Error al validar.')
        return
      }
      setRechazando(false)
      setMotivoRechazo('')
      onCambio()
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className={styles.resultadoItem}>
      <div className={styles.resultadoCabeza}>
        <span className={`${styles.areaTag} ${resultado.analisis.area === 'MIC' ? styles.areaMic : styles.areaFq}`}>
          {resultado.analisis.area}
        </span>
        <span className={styles.resultadoNombre}>{resultado.analisis.nombre}</span>
        <span className={`${styles.badge} ${ESTADO_RESULTADO_BADGE[resultado.estado]}`}>
          {ESTADO_RESULTADO_LABEL[resultado.estado]}
        </span>
      </div>

      {referencia && <p className={styles.resultadoReferencia}>{referencia}</p>}

      {editando ? (
        <div className={styles.resultadoEdicion}>
          <input
            className={styles.formInput}
            value={valor}
            onChange={e => setValor(e.target.value)}
            placeholder={resultado.analisis.unidad ? `Valor en ${resultado.analisis.unidad}` : 'Valor obtenido'}
            autoFocus
          />
          <button className={styles.btnSecundario} onClick={() => { setEditando(false); setError('') }} disabled={guardando}>Cancelar</button>
          <button className={styles.btnPrimario} onClick={guardarValor} disabled={guardando}>
            {guardando ? 'Guardando...' : 'Guardar'}
          </button>
        </div>
      ) : (
        <div className={styles.resultadoValorRow}>
          <span className={styles.resultadoValor}>
            {resultado.valor ? `${resultado.valor} ${resultado.analisis.unidad ?? ''}` : <span className={styles.sinDato}>Sin cargar</span>}
          </span>
          {puedeEditar && (
            <button className={styles.btnEditar} onClick={() => setEditando(true)}>
              {resultado.estado === 'rechazado' ? 'Corregir' : 'Cargar'}
            </button>
          )}
        </div>
      )}

      {resultado.fueraDeRango && (
        <p className={styles.avisoFueraDeRango}>⚠ Valor fuera del rango de referencia.</p>
      )}

      {resultado.estado === 'rechazado' && resultado.motivoRechazo && (
        <p className={styles.motivoRechazo}>Rechazado: "{resultado.motivoRechazo}"</p>
      )}

      {resultado.valor && (
        <p className={styles.resultadoMeta}>
          {resultado.estado === 'validado' && resultado.validadoPor
            ? `Validado por ${resultado.validadoPor.nombre} ${resultado.validadoPor.apellido}`
            : resultado.cargadoPor
              ? `Cargado por ${resultado.cargadoPor.nombre} ${resultado.cargadoPor.apellido}`
              : null}
          {resultado.fechaCarga && ` · ${new Date(resultado.fechaCarga).toLocaleDateString('es-AR')}`}
        </p>
      )}

      {resultado.estado === 'cargado' && (
        <div className={styles.validacionBloque}>
          {rechazando ? (
            <div className={styles.rechazoForm}>
              <textarea
                className={styles.formTextarea}
                rows={2}
                value={motivoRechazo}
                onChange={e => setMotivoRechazo(e.target.value)}
                placeholder="Motivo del rechazo..."
                autoFocus
              />
              <div className={styles.validacionAcciones}>
                <button className={styles.btnSecundario} onClick={() => { setRechazando(false); setMotivoRechazo(''); setError('') }} disabled={guardando}>
                  Cancelar
                </button>
                <button className={styles.btnAccionRojo} onClick={() => validar(false)} disabled={guardando}>
                  {guardando ? 'Guardando...' : 'Confirmar rechazo'}
                </button>
              </div>
            </div>
          ) : (
            <div className={styles.validacionAcciones}>
              <button className={styles.btnSecundario} onClick={() => setRechazando(true)} disabled={guardando}>
                Rechazar
              </button>
              <button className={styles.btnAccionVerde} onClick={() => validar(true)} disabled={guardando}>
                {guardando ? 'Guardando...' : 'Validar'}
              </button>
            </div>
          )}
        </div>
      )}

      {error && <p className={styles.error}>{error}</p>}
    </div>
  )
}
