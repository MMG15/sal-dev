export interface SesionUsuario {
  token: string
  idUsuario: number
  nombre: string
  apellido: string
  email: string
  rolCodigo: string
  rolNombre: string
  forzarCambioPwd: boolean
}
