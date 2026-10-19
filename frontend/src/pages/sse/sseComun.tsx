import { useEffect, useState } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './SsePage.module.css'

// Tipos, constantes y modales compartidos del módulo SSE / Rótulos

export interface SseResumen {
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

export interface SseDetalle {
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
  empresa: { idEmpresa: number; razonSocial: string; email: string | null; telefono: string | null } | null
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

export interface PresupuestoOpcion {
  idPresupuesto: number
  codigoRpo: string | null
  importeDolares: number
  consultaCodigo: string | null
  cliente: { nombre: string; apellido: string } | null
  empresa: { razonSocial: string } | null
}

export const ESTADOS_SSE = ['activa', 'analizando', 'completada', 'cerrada', 'rechazada']
export const AREAS = ['MIC', 'FQ', 'AMBAS']
export const AREAS_SELECCIONABLES = ['MIC', 'FQ']

export const CAMPO_AUDITORIA_LABEL: Record<string, string> = {
  numero_unico: 'el número',
  descripcion: 'la descripción',
  estado: 'el estado',
}

export function ModalNuevaSse({ onCreada, onCerrar }: {
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

export function ModalRotulo({ idSse, onGuardado, onCerrar }: {
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

export function ModalCorregirRotulo({ idSse, rotulo, onGuardado, onCerrar }: {
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

// ─── Modal: Asignar cliente y/o empresa ────────────────────────────────────────

export function ModalAsignarCliente({ idSse, idClienteActual, idEmpresaActual, onGuardado, onCerrar }: {
  idSse: number
  idClienteActual: number | null
  idEmpresaActual: number | null
  onGuardado: () => void
  onCerrar: () => void
}) {
  const [clientes, setClientes] = useState<{ idCliente: number; nombre: string; apellido: string }[]>([])
  const [empresas, setEmpresas] = useState<{ idEmpresa: number; razonSocial: string }[]>([])
  const [idCliente, setIdCliente] = useState<number | ''>(idClienteActual ?? '')
  const [idEmpresa, setIdEmpresa] = useState<number | ''>(idEmpresaActual ?? '')
  const [guardando, setGuardando] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    apiFetch('/api/clientes').then(r => r.json()).then(setClientes).catch(() => {})
    apiFetch('/api/empresas').then(r => r.json()).then(setEmpresas).catch(() => {})
  }, [])

  async function guardar() {
    setGuardando(true)
    setError('')
    try {
      const res = await apiFetch(`/api/sses/${idSse}/cliente`, {
        method: 'PUT',
        body: JSON.stringify({
          idCliente: idCliente !== '' ? idCliente : null,
          idEmpresa: idEmpresa !== '' ? idEmpresa : null,
        })
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'Error al asignar el cliente/empresa.')
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
          <h2>Asignar cliente / empresa</h2>
          <button className={styles.btnCerrar} onClick={onCerrar}>✕</button>
        </div>
        <div className={styles.modalBody}>
          <label className={styles.formLabel}>Cliente</label>
          <select
            className={styles.formInput}
            value={idCliente}
            onChange={e => setIdCliente(e.target.value ? Number(e.target.value) : '')}
          >
            <option value="">— Sin cliente —</option>
            {clientes.map(c => (
              <option key={c.idCliente} value={c.idCliente}>{c.apellido}, {c.nombre}</option>
            ))}
          </select>

          <label className={styles.formLabel}>Empresa</label>
          <select
            className={styles.formInput}
            value={idEmpresa}
            onChange={e => setIdEmpresa(e.target.value ? Number(e.target.value) : '')}
          >
            <option value="">— Sin empresa —</option>
            {empresas.map(e => (
              <option key={e.idEmpresa} value={e.idEmpresa}>{e.razonSocial}</option>
            ))}
          </select>

          <p className={styles.inputHint}>
            Podés asignar cliente, empresa, o ambos. La SSE es un comprobante que le queda tanto al laboratorio como al cliente: asignalos para poder descargar el PDF con esos datos.
          </p>
          {error && <p className={styles.error}>{error}</p>}
        </div>
        <div className={styles.modalFooter}>
          <button className={styles.btnSecundario} onClick={onCerrar}>Cancelar</button>
          <button className={styles.btnPrimario} onClick={guardar} disabled={guardando}>
            {guardando ? 'Guardando...' : 'Guardar'}
          </button>
        </div>
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

export function ModalMuestra({ idSse, onGuardado, onCerrar }: {
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
