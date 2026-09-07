import { createContext, useContext } from 'react'
import type { SesionUsuario } from '../types'

interface SesionContextValue {
  sesion: SesionUsuario
  cerrarSesion: () => void
}

export const SesionContext = createContext<SesionContextValue | null>(null)

export function useSesion(): SesionContextValue {
  const ctx = useContext(SesionContext)
  if (!ctx) throw new Error('useSesion debe usarse dentro de un SesionProvider')
  return ctx
}
