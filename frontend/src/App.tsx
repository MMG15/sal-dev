import { useState, type ReactNode } from 'react'
import { Routes, Route, Navigate } from 'react-router-dom'
import type { SesionUsuario } from './types'
import { SesionContext } from './context/SesionContext'
import { MODULOS } from './config/modulos'

import Login from './pages/Login'
import AppLayout from './layout/AppLayout'
import InicioPage from './pages/InicioPage'
import ClientesPage from './pages/clientes/ClientesPage'
import CorreosPage from './pages/correos/CorreosPage'
import CorreoDetallePage from './pages/correos/CorreoDetallePage'
import ConsultasPage from './pages/consultas/ConsultasPage'
import ConsultaDetallePage from './pages/consultas/ConsultaDetallePage'
import PresupuestoDetallePage from './pages/presupuestos/PresupuestoDetallePage'
import SseDetallePage from './pages/sse/SseDetallePage'
import ResultadoDetallePage from './pages/resultados/ResultadoDetallePage'
import FirmaDetallePage from './pages/firma/FirmaDetallePage'
import PresupuestosPage from './pages/presupuestos/PresupuestosPage'
import SsePage from './pages/sse/SsePage'
import AnalisisPage from './pages/analisis/AnalisisPage'
import ResultadosPage from './pages/resultados/ResultadosPage'
import FacturacionPage from './pages/facturacion/FacturacionPage'
import FirmaPage from './pages/firma/FirmaPage'
import UsuariosPage from './pages/usuarios/UsuariosPage'
import PortalPage from './pages/portal/PortalPage'
import FormCambiarPassword from './components/FormCambiarPassword'
import AccesoDenegado from './pages/AccesoDenegado'

const SESION_KEY = 'ssal_sesion'

const PAGINAS: Record<string, ReactNode> = {
  inicio: <InicioPage />,
  clientes: <ClientesPage />,
  correos: <CorreosPage />,
  consultas: <ConsultasPage />,
  presupuestos: <PresupuestosPage />,
  sse: <SsePage />,
  analisis: <AnalisisPage />,
  resultados: <ResultadosPage />,
  facturacion: <FacturacionPage />,
  firma: <FirmaPage />,
  usuarios: <UsuariosPage />,
}

// Vistas de detalle dentro de un módulo (ruta /modulo/:id)
const PAGINAS_DETALLE: Record<string, ReactNode> = {
  correos: <CorreoDetallePage />,
  consultas: <ConsultaDetallePage />,
  presupuestos: <PresupuestoDetallePage />,
  sse: <SseDetallePage />,
  resultados: <ResultadoDetallePage />,
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

  // Personal: contraseña temporal tras un alta o restablecimiento. Debe cambiarla antes de seguir.
  if (sesion.rolCodigo !== 'PORTAL' && sesion.forzarCambioPwd) {
    return (
      <SesionContext.Provider value={{ sesion, cerrarSesion }}>
        <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 12, padding: 16, background: 'var(--fondo)' }}>
          <FormCambiarPassword
            titulo="Cambiá tu contraseña"
            ayuda="Tu contraseña es temporal. Elegí una nueva para continuar."
            textoBoton="Continuar"
            onListo={() => iniciarSesion({ ...sesion, forzarCambioPwd: false })}
          />
          <button onClick={cerrarSesion} style={{ background: 'none', border: 'none', color: 'var(--teal)', cursor: 'pointer', fontSize: 16 }}>
            Salir
          </button>
        </div>
      </SesionContext.Provider>
    )
  }

  // Clientes externos: el portal es una aplicación aparte, sin menú interno
  if (sesion.rolCodigo === 'PORTAL') {
    return (
      <SesionContext.Provider value={{ sesion, cerrarSesion }}>
        <PortalPage />
      </SesionContext.Provider>
    )
  }

  return (
    <SesionContext.Provider value={{ sesion, cerrarSesion }}>
      <Routes>
        <Route path="/" element={<Navigate to="/inicio" replace />} />
        <Route element={<AppLayout />}>
          {MODULOS.map(m => (
            <Route key={m.id}>
              <Route
                path={m.path}
                element={
                  <RolGuard roles={m.roles}>
                    {PAGINAS[m.id]}
                  </RolGuard>
                }
              />
              {PAGINAS_DETALLE[m.id] && (
                <Route
                  path={`${m.path}/:id`}
                  element={
                    <RolGuard roles={m.roles}>
                      {PAGINAS_DETALLE[m.id]}
                    </RolGuard>
                  }
                />
              )}
            </Route>
          ))}
          <Route
            path="/firma/:tipo/:id"
            element={
              <RolGuard roles={MODULOS.find(m => m.id === 'firma')?.roles ?? []}>
                <FirmaDetallePage />
              </RolGuard>
            }
          />
        </Route>
        <Route path="/acceso-denegado" element={<AccesoDenegado />} />
        <Route path="*" element={<Navigate to="/inicio" replace />} />
      </Routes>
    </SesionContext.Provider>
  )
}
