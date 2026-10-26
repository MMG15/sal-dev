export interface ModuloConfig {
  id: string
  label: string
  descripcion: string
  path: string
  roles: string[]
  listo: boolean
}

const TODOS = ['ROL-01', 'ROL-02', 'ROL-03', 'ROL-04', 'ROL-05', 'ROL-06', 'ROL-07', 'ROL-08', 'ROL-09']

export const MODULOS: ModuloConfig[] = [
  {
    id: 'inicio',
    label: 'Inicio',
    descripcion: 'Panel general del sistema',
    path: '/inicio',
    roles: TODOS,
    listo: true,
  },
  {
    id: 'clientes',
    label: 'Clientes y empresas',
    descripcion: 'Datos de clientes, empresas y contactos',
    path: '/clientes',
    roles: ['ROL-01', 'ROL-02', 'ROL-07'],
    listo: true,
  },
  {
    id: 'correos',
    label: 'Correo entrante',
    descripcion: 'Bandeja del mail de consultas: filtrar y convertir en Consulta',
    path: '/correos',
    roles: ['ROL-01', 'ROL-02', 'ROL-03', 'ROL-07'],
    listo: true,
  },
  {
    id: 'consultas',
    label: 'Consultas',
    descripcion: 'Seguimiento de consultas y cotizaciones',
    path: '/consultas',
    roles: ['ROL-01', 'ROL-02', 'ROL-03', 'ROL-07'],
    listo: true,
  },
  {
    id: 'presupuestos',
    label: 'Presupuestos',
    descripcion: 'Presupuestos, análisis y archivos adjuntos',
    path: '/presupuestos',
    roles: ['ROL-01', 'ROL-02', 'ROL-03'],
    listo: true,
  },
  {
    id: 'sse',
    label: 'SSE / Rótulos',
    descripcion: 'Solicitudes de servicio y rótulos internos',
    path: '/sse',
    roles: ['ROL-01', 'ROL-02', 'ROL-07'],
    listo: true,
  },
  {
    id: 'analisis',
    label: 'Análisis',
    descripcion: 'Catálogo de análisis y grupos por área',
    path: '/analisis',
    roles: ['ROL-01', 'ROL-02', 'ROL-03', 'ROL-07', 'ROL-08', 'ROL-09'],
    listo: true,
  },
  {
    id: 'resultados',
    label: 'Resultados',
    descripcion: 'Carga y validación de resultados de análisis',
    path: '/resultados',
    roles: ['ROL-01', 'ROL-07', 'ROL-08', 'ROL-09'],
    listo: true,
  },
  {
    id: 'facturacion',
    label: 'Facturación',
    descripcion: 'Facturas emitidas en TANGO y control de cobros',
    path: '/facturacion',
    roles: ['ROL-01', 'ROL-04', 'ROL-08', 'ROL-09'],
    listo: true,
  },
  {
    id: 'firma',
    label: 'Firma digital',
    descripcion: 'Firma de responsables de área y aprobación final',
    path: '/firma',
    roles: ['ROL-01', 'ROL-08', 'ROL-09'],
    listo: true,
  },
  {
    id: 'usuarios',
    label: 'Usuarios',
    descripcion: 'Cuentas del personal, roles y registro de auditoría',
    path: '/usuarios',
    roles: ['ROL-01', 'ROL-06'],
    listo: true,
  },
]

export function modulosParaRol(rolCodigo: string): ModuloConfig[] {
  return MODULOS.filter(m => m.roles.includes(rolCodigo))
}
