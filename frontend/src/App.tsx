import { useState, type ReactNode } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import type { SesionUsuario } from './types'
import { SesionContext } from './context/SesionContext'
import { MODULOS } from './config/modulos'

import Login from './pages/Login'
import AppLayout from './layout/AppLayout'
import InicioPage from './pages/InicioPage'
import ClientesPage from './pages/clientes/ClientesPage'
import ConsultasPage from './pages/consultas/ConsultasPage'
import PresupuestosPage from './pages/presupuestos/PresupuestosPage'
import SsePage from './pages/sse/SsePage'
import AnalisisPage from './pages/analisis/AnalisisPage'
import ResultadosPage from './pages/resultados/ResultadosPage'
import FacturacionPage from './pages/facturacion/FacturacionPage'
import FirmaPage from './pages/firma/FirmaPage'
import UsuariosPage from './pages/usuarios/UsuariosPage'
import PortalPage from './pages/portal/PortalPage'
import AccesoDenegado from './pages/AccesoDenegado'

const SESION_KEY = 'ssal_sesion'

const PAGINAS: Record<string, ReactNode> = {
  inicio: <InicioPage />,
  clientes: <ClientesPage />,
  consultas: <ConsultasPage />,
  presupuestos: <PresupuestosPage />,
  sse: <SsePage />,
  analisis: <AnalisisPage />,
  resultados: <ResultadosPage />,
  facturacion: <FacturacionPage />,
  firma: <FirmaPage />,
  usuarios: <UsuariosPage />,
  portal: <PortalPage />,
}

function RolGuard({ roles, children }: { roles: string[]; children: ReactNode }) {
  const sesionRaw = localStorage.getItem(SESION_KEY)
  const sesion: SesionUsuario | null = sesionRaw ? JSON.parse(sesionRaw) : null
  if (!sesion || !roles.includes(sesion.rolCodigo)) {
    return <Navigate to="/acceso-denegado" replace />
  }
  return <>{children}</>
}

export default function App() {
  const [sesion, setSesion] = useState<SesionUsuario | null>(() => {
    try {
      const raw = localStorage.getItem(SESION_KEY)
      return raw ? (JSON.parse(raw) as SesionUsuario) : null
    } catch {
      return null
    }
  })

  function iniciarSesion(datos: SesionUsuario) {
    localStorage.setItem(SESION_KEY, JSON.stringify(datos))
    setSesion(datos)
  }

  function cerrarSesion() {
    localStorage.removeItem(SESION_KEY)
    setSesion(null)
  }

  if (!sesion) {
    return (
      <Routes>
        <Route path="*" element={<Login onLogin={iniciarSesion} />} />
      </Routes>
    )
  }

  return (
    <SesionContext.Provider value={{ sesion, cerrarSesion }}>
      <Routes>
        <Route path="/" element={<Navigate to="/inicio" replace />} />
        <Route element={<AppLayout />}>
          {MODULOS.map(m => (
            <Route
              key={m.id}
              path={m.path}
              element={
                <RolGuard roles={m.roles}>
                  {PAGINAS[m.id]}
                </RolGuard>
              }
            />
          ))}
        </Route>
        <Route path="/acceso-denegado" element={<AccesoDenegado />} />
        <Route path="*" element={<Navigate to="/inicio" replace />} />
      </Routes>
    </SesionContext.Provider>
  )
}
