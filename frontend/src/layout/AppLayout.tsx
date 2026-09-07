import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useSesion } from '../context/SesionContext'
import { modulosParaRol } from '../config/modulos'
import { MODULO_ICONOS, MarcaLaboratorio } from './icons'
import styles from './AppLayout.module.css'

function iniciales(nombre: string, apellido: string) {
  return `${nombre.charAt(0)}${apellido.charAt(0)}`.toUpperCase()
}

export default function AppLayout() {
  const { sesion, cerrarSesion } = useSesion()
  const navigate = useNavigate()
  const location = useLocation()
  const modulos = modulosParaRol(sesion.rolCodigo)
  const moduloActual = modulos.find(m => location.pathname.startsWith(m.path))

  function handleLogout() {
    cerrarSesion()
    navigate('/login', { replace: true })
  }

  return (
    <div className={styles.layout}>
      <aside className={styles.sidebar}>
        <div className={styles.logoArea}>
          <MarcaLaboratorio className={styles.marca} />
          <div className={styles.logoTextos}>
            <span className={styles.logoTexto}>SSAL</span>
            <span className={styles.logoSub}>Graffigna</span>
          </div>
        </div>
        <nav className={styles.nav}>
          {modulos.map(m => (
            <NavLink
              key={m.id}
              to={m.path}
              className={({ isActive }) =>
                `${styles.navItem} ${isActive ? styles.activo : ''} ${!m.listo ? styles.pendiente : ''}`
              }
            >
              <span className={styles.icono}>{MODULO_ICONOS[m.id]}</span>
              <span className={styles.label}>{m.label}</span>
              {!m.listo && <span className={styles.badgePronto}>pronto</span>}
            </NavLink>
          ))}
        </nav>
      </aside>

      <div className={styles.panelPrincipal}>
        <header className={styles.header}>
          <div className={styles.headerTitulo}>
            <h1>{moduloActual?.label ?? 'SSAL'}</h1>
            {moduloActual && <span>{moduloActual.descripcion}</span>}
          </div>
          <div className={styles.headerUsuario}>
            <span className={styles.avatar}>{iniciales(sesion.nombre, sesion.apellido)}</span>
            <div className={styles.headerUsuarioTextos}>
              <span className={styles.nombreUsuario}>
                {sesion.nombre} {sesion.apellido}
              </span>
              <span className={styles.badgeRol}>{sesion.rolNombre}</span>
            </div>
            <button onClick={handleLogout} className={styles.btnSalir}>
              Salir
            </button>
          </div>
        </header>
        <main className={styles.contenido}>
          <Outlet />
        </main>
      </div>
    </div>
  )
}
