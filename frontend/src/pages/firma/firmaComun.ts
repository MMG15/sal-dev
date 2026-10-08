// Tipos y utilidades compartidas del módulo Firma digital

export interface DocumentoLista {
  tipo: 'informe' | 'presupuesto'
  id: number
  codigo: string
  destinatario: string
  sse: string | null
  rotulo: string | null
  fecha: string
  areas: string[]
  estado: string
  areasFirmadas: string[]
}

export interface Permisos {
  firmar: boolean
  aprobar: boolean
  rechazar: boolean
  reenviar: boolean
}

export interface EventoFirma {
  idFirma: number
  tipoFirma: 'responsable' | 'aprobacion' | 'rechazo' | 'reenvio'
  area: string | null
  estado: string
  motivoRechazo: string | null
  fechaFirma: string
  firmante: string
}

export interface DetalleDocumento {
  documento: DocumentoLista
  estado: string
  areasFirmadas: string[]
  permisos: Permisos
  historial: EventoFirma[]
}

export const ESTADO_FIRMA_LABEL: Record<string, string> = {
  pendiente_firma: 'Pendiente de firma',
  firmado: 'Pendiente de aprobación',
  rechazado: 'Rechazado',
  aprobado: 'Aprobado',
}

export const ESTADO_FIRMA_COLOR: Record<string, string> = {
  pendiente_firma: 'orange',
  firmado: 'blue',
  rechazado: 'red',
  aprobado: 'teal',
}

export const TIPO_LABEL: Record<string, string> = {
  informe: 'Informe de resultados',
  presupuesto: 'Presupuesto',
}

export const TIPO_EVENTO: Record<string, string> = {
  responsable: 'Firma del responsable',
  aprobacion: 'Aprobación final',
  rechazo: 'Rechazo',
  reenvio: 'Reenvío para firma',
}

export function fechaCorta(iso: string) {
  return new Date(iso).toLocaleDateString('es-AR')
}

export function fechaHora(iso: string) {
  return new Date(iso).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' })
}
