// Tipos y utilidades compartidas del módulo Firma digital

export interface DocumentoLista {
  tipo: 'informe' | 'presupuesto'
  id: number
  codigo: string
  destinatario: string
  destinatarioEmail: string | null
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

export function datosEnvioInforme(doc: DocumentoLista): { destinatario: string; asunto: string; cuerpo: string } {
  const cuerpo = [
    `Estimado/a ${doc.destinatario},`,
    '',
    `Nos comunicamos desde el Laboratorio Control de Calidad "Dr. Alberto Graffigna" - UCCuyo`,
    `para hacerle llegar el informe de resultados, ya aprobado y firmado, correspondiente a su SSE.`,
    '',
    `─────────────────────────────────────────`,
    `INFORME ${doc.codigo}`,
    doc.sse ? `SSE: ${doc.sse}` : undefined,
    doc.rotulo ? `Rótulo: ${doc.rotulo}` : undefined,
    `Fecha: ${fechaCorta(doc.fecha)}`,
    `─────────────────────────────────────────`,
    '',
    'El informe firmado va adjunto en PDF a este correo.',
    '',
    'Ante cualquier consulta, no dude en comunicarse.',
    '',
    'Saludos cordiales,',
    'Laboratorio Control de Calidad "Dr. Alberto Graffigna"',
    'UCCuyo — San Juan, Argentina',
  ].filter((l): l is string => l !== undefined).join('\n')

  return { destinatario: doc.destinatarioEmail ?? '', asunto: `Informe ${doc.codigo} — Laboratorio Graffigna UCCuyo`, cuerpo }
}

export function fechaCorta(iso: string) {
  return new Date(iso).toLocaleDateString('es-AR')
}

export function fechaHora(iso: string) {
  return new Date(iso).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' })
}
