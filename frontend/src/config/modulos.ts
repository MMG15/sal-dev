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
    roles: ['ROL-01', 'ROL-03'],
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
    descripcion: 'Facturación y cobranzas',
    path: '/facturacion',
    roles: ['ROL-01', 'ROL-04'],
    listo: false,
  },
  {
    id: 'firma',
    label: 'Firma digital',
    descripcion: 'Validación y firma de informes',
    path: '/firma',
    roles: ['ROL-01', 'ROL-08', 'ROL-09'],
    listo: false,
  },
  {
    id: 'usuarios',
    label: 'Usuarios',
    descripcion: 'Administración de usuarios y roles',
    path: '/usuarios',
    roles: ['ROL-01', 'ROL-06'],
    listo: false,
  },
  {
    id: 'portal',
    label: 'Mis resultados',
    descripcion: 'Portal de resultados para clientes',
    path: '/portal',
    roles: ['ROL-05'],
    listo: false,
  },
]

export function modulosParaRol(rolCodigo: string): ModuloConfig[] {
  return MODULOS.filter(m => m.roles.includes(rolCodigo))
}
