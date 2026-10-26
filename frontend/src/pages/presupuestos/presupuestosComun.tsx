// Tipos, constantes y utilidades compartidas del módulo Presupuestos



export interface PresupuestoResumen {
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

export interface Archivo {
  id: number
  nombreOriginal: string
  tipoMime: string
  tamañoBytes: number
  subidoEn: string
  subidoPor: string
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
  consulta: {
    idConsulta: number
    codigo: string | null
    canal: string
    descripcion: string | null
    cliente: {
      idCliente: number; nombre: string; apellido: string
      email: string | null; telefono: string | null
      empresa: { idEmpresa: number; razonSocial: string; email: string | null } | null
    } | null
    empresa: { idEmpresa: number; razonSocial: string; email: string | null } | null
  }
  creadoPor: { nombre: string; apellido: string }
  items: ItemPresupuesto[]
  paquetes: ItemPaquetePresupuesto[]
  metodos: ItemMetodoPresupuesto[]
}

// ─── Constantes ───────────────────────────────────────────────────────────────

export const ESTADOS = ['borrador', 'enviado', 'aceptado', 'rechazado']


export const TRANSICIONES: Record<string, { label: string; siguienteEstado: string; variante: 'primario' | 'verde' | 'rojo' }[]> = {
  borrador:  [{ label: 'Marcar como enviado', siguienteEstado: 'enviado', variante: 'primario' }],
  enviado:   [
    { label: 'Aceptado por el cliente', siguienteEstado: 'aceptado', variante: 'verde' },
    { label: 'Rechazado por el cliente', siguienteEstado: 'rechazado', variante: 'rojo' },
  ],
  aceptado:  [],
  rechazado: [{ label: 'Reabrir como borrador', siguienteEstado: 'borrador', variante: 'primario' }],
}

export const TIPO_ASESORAMIENTO_LABEL: Record<string, string> = {
  consulta: 'Solo consulta',
  terreno: 'Visita de terreno',
  proceso_completo: 'Proceso completo (consulta + muestreo + entrega)',
}

export function datosEnvioPresupuesto(p: PresupuestoDetalle, archivos: Archivo[]): { destinatario: string; asunto: string; cuerpo: string } {
    const cliente = p.consulta.cliente
    const empresa = p.consulta.empresa ?? p.consulta.cliente?.empresa
    const destinatario = cliente?.email ?? empresa?.email ?? ''

    const nombreDestinatario = cliente
      ? `${cliente.nombre} ${cliente.apellido}`
      : empresa?.razonSocial ?? 'cliente'

    const lineasPaquetes = p.paquetes
      .map(pq => `  • [${pq.area}] ${pq.nombre} (paquete — incluye ${pq.analisisIncluidos.join(', ')}): USD ${pq.precioUsdSnapshot.toFixed(2)}`)
      .join('\n')

    const lineasAnalisis = p.items
      .map(i => `  • [${i.area}] ${i.nombre}: USD ${i.precioUsdSnapshot.toFixed(2)}`)
      .join('\n')

    const lineasAnalisisCompleto = [lineasPaquetes, lineasAnalisis].filter(Boolean).join('\n')

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
      lineasAnalisisCompleto || '  (sin análisis)',
      '',
      ...(p.mostrarMoneda !== 'ars' ? [
        `Subtotal análisis:       USD ${(p.items.reduce((s, i) => s + i.precioUsdSnapshot, 0) + p.paquetes.reduce((s, pq) => s + pq.precioUsdSnapshot, 0)).toFixed(2)}`,
        p.adicionalAsesoramiento > 0
          ? `Asesoramiento técnico (${p.tipoAsesoramiento ? TIPO_ASESORAMIENTO_LABEL[p.tipoAsesoramiento] ?? p.tipoAsesoramiento : 'sin especificar'}): USD ${p.adicionalAsesoramiento.toFixed(2)}`
          : '',
        `TOTAL USD:               USD ${p.importeDolares.toFixed(2)}`,
      ] : []),
      ...(p.mostrarMoneda === 'ambos' ? [`Cotización USD/ARS:      $ ${p.cotizacion.toFixed(0)}`] : []),
      ...(p.mostrarMoneda !== 'usd' ? [`TOTAL ARS:               $ ${p.importePesos.toFixed(2)}`] : []),
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

    return { destinatario, asunto, cuerpo }
}

export function formatBytes(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

export function iconoMime(mime: string): string {
  if (mime === 'application/pdf') return '📄'
  if (mime.includes('word') || mime.includes('document')) return '📝'
  if (mime.includes('sheet') || mime.includes('excel')) return '📊'
  if (mime.startsWith('image/')) return '🖼'
  if (mime.includes('zip')) return '🗜'
  return '📎'
}
