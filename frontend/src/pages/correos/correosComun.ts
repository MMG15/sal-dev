// Tipos y constantes compartidas del módulo de correo entrante

export interface CorreoResumen {
  idCorreo: number
  remitente: string
  remitenteNombre: string | null
  asunto: string | null
  resumen: string | null
  fechaRecibido: string
  estado: 'pendiente' | 'respondido' | 'convertido' | 'descartado'
  idConsultaGenerada: number | null
  idConsultaRelacionada: number | null
}

export interface CorreoDetalle {
  idCorreo: number
  remitente: string
  remitenteNombre: string | null
  asunto: string | null
  cuerpo: string | null
  fechaRecibido: string
  estado: 'pendiente' | 'respondido' | 'convertido' | 'descartado'
  usuarioAccion: { nombre: string; apellido: string } | null
  fechaAccion: string | null
  consultaGenerada: { idConsulta: number; codigo: string | null } | null
  consultaRelacionada: {
    idConsulta: number
    codigo: string | null
    presupuesto: { idPresupuesto: number; codigoRpo: string | null; estado: string } | null
  } | null
}

export const ESTADOS_CORREO = ['pendiente', 'respondido', 'convertido', 'descartado']

export const ESTADO_CORREO_LABEL: Record<string, string> = {
  pendiente: 'Pendiente',
  respondido: 'Respondido',
  convertido: 'Convertido en consulta',
  descartado: 'Descartado',
}

export const ESTADO_CORREO_COLOR: Record<string, string> = {
  pendiente: 'orange',
  respondido: 'blue',
  convertido: 'teal',
  descartado: 'gray',
}
