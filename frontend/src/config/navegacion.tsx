import type { ReactNode } from 'react'
import {
  IconLayoutDashboard,
  IconUsers,
  IconMessages,
  IconFileSpreadsheet,
  IconTag,
  IconFlask,
  IconMicroscope,
  IconReceipt,
  IconSignature,
  IconUserShield,
} from '@tabler/icons-react'

// Grupos del menú lateral. El orden de los grupos es el orden en pantalla.
export interface GrupoNavegacion {
  titulo: string | null
  moduloIds: string[]
}

export const GRUPOS_NAVEGACION: GrupoNavegacion[] = [
  { titulo: null, moduloIds: ['inicio'] },
  { titulo: 'Atención al cliente', moduloIds: ['clientes', 'consultas', 'presupuestos', 'sse'] },
  { titulo: 'Laboratorio', moduloIds: ['analisis', 'resultados', 'firma'] },
  { titulo: 'Administración', moduloIds: ['facturacion', 'usuarios'] },
]

// Ícono de cada módulo del menú
export const ICONO_MODULO: Record<string, ReactNode> = {
  inicio: <IconLayoutDashboard size={18} stroke={1.6} />,
  clientes: <IconUsers size={18} stroke={1.6} />,
  consultas: <IconMessages size={18} stroke={1.6} />,
  presupuestos: <IconFileSpreadsheet size={18} stroke={1.6} />,
  sse: <IconTag size={18} stroke={1.6} />,
  analisis: <IconFlask size={18} stroke={1.6} />,
  resultados: <IconMicroscope size={18} stroke={1.6} />,
  firma: <IconSignature size={18} stroke={1.6} />,
  facturacion: <IconReceipt size={18} stroke={1.6} />,
  usuarios: <IconUserShield size={18} stroke={1.6} />,
}

// Pantallas que ocupan todo el ancho sin relleno (paneles divididos o inicio con su propio diseño)
export const PANTALLAS_COMPLETAS = ['/inicio']
