import { Link, Outlet, useLocation, useNavigate } from 'react-router-dom'
import { AppShell, Avatar, Box, Breadcrumbs, Burger, Group, Kbd, Menu, Modal, ScrollArea, Text, UnstyledButton } from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import { Spotlight, spotlight } from '@mantine/spotlight'
import { IconChevronUp, IconKey, IconLogout, IconSearch } from '@tabler/icons-react'
import { useSesion } from '../context/SesionContext'
import { modulosParaRol } from '../config/modulos'
import { GRUPOS_NAVEGACION, ICONO_MODULO, PANTALLAS_COMPLETAS } from '../config/navegacion'
import FormCambiarPassword from '../components/FormCambiarPassword'
import styles from './AppLayout.module.css'

function iniciales(nombre: string, apellido: string) {
  return `${nombre.charAt(0)}${apellido.charAt(0)}`.toUpperCase()
}

export default function AppLayout() {
  const { sesion, cerrarSesion } = useSesion()
  const navigate = useNavigate()
  const location = useLocation()
  const [navAbierto, { toggle: alternarNav }] = useDisclosure(true)
  const [cuentaAbierta, { open: abrirCuenta, close: cerrarCuenta }] = useDisclosure(false)

  const modulos = modulosParaRol(sesion.rolCodigo)
  const moduloActual = modulos.find(m => location.pathname.startsWith(m.path))
  const grupoActual = GRUPOS_NAVEGACION.find(g => moduloActual && g.moduloIds.includes(moduloActual.id))
  const pantallaCompleta = PANTALLAS_COMPLETAS.some(p => location.pathname.startsWith(p))

  function handleLogout() {
    cerrarSesion()
    navigate('/login', { replace: true })
  }

  // Acciones de la búsqueda global (Ctrl+K): por ahora, navegar a cada módulo
  const acciones = modulos.map(m => ({
    id: m.id,
    label: m.label,
    description: m.descripcion,
    leftSection: ICONO_MODULO[m.id],
    onClick: () => navigate(m.path),
  }))

  return (
    <>
      <AppShell
        navbar={{ width: 264, breakpoint: 'md', collapsed: { mobile: !navAbierto, desktop: !navAbierto } }}
        padding={0}
      >
        <AppShell.Navbar className={styles.sidebar} p="md">
          <Box className={styles.marca}>
            <img
              src="/logo-Laboratorio-Graffigna-editado_droid-serif.png"
              alt="Laboratorio Control de Calidad Dr. Alberto Graffigna — UCCuyo"
              className={styles.logo}
            />
          </Box>

          <ScrollArea type="auto" style={{ flex: 1 }} scrollbarSize={6}>
            {GRUPOS_NAVEGACION.map(grupo => {
              const items = modulos.filter(m => grupo.moduloIds.includes(m.id))
              if (items.length === 0) return null
              return (
                <div key={grupo.titulo ?? 'principal'} className={styles.grupo}>
                  {grupo.titulo && <p className={styles.grupoTitulo}>{grupo.titulo}</p>}
                  {items.map(m => {
                    const activo = location.pathname.startsWith(m.path)
                    return (
                      <Link
                        key={m.id}
                        to={m.path}
                        className={`${styles.item} ${activo ? styles.itemActivo : ''}`}
                        aria-current={activo ? 'page' : undefined}
                      >
                        <span className={styles.iconoItem}>{ICONO_MODULO[m.id]}</span>
                        {m.label}
                      </Link>
                    )
                  })}
                </div>
              )
            })}
          </ScrollArea>

          <Menu position="top-start" width={240} shadow="md" withArrow>
            <Menu.Target>
              <UnstyledButton className={styles.usuarioBtn} aria-label="Menú de usuario">
                <Avatar color="teal" radius="xl" size="md">{iniciales(sesion.nombre, sesion.apellido)}</Avatar>
                <Box style={{ flex: 1, minWidth: 0 }}>
                  <Text className={styles.usuarioNombre} truncate>{sesion.nombre} {sesion.apellido}</Text>
                  <Text className={styles.usuarioRol} truncate>{sesion.rolNombre}</Text>
                </Box>
                <IconChevronUp size={16} />
              </UnstyledButton>
            </Menu.Target>
            <Menu.Dropdown>
              <Menu.Label>{sesion.email}</Menu.Label>
              <Menu.Item leftSection={<IconKey size={16} />} onClick={abrirCuenta}>
                Mi cuenta
              </Menu.Item>
              <Menu.Divider />
              <Menu.Item color="red" leftSection={<IconLogout size={16} />} onClick={handleLogout}>
                Salir
              </Menu.Item>
            </Menu.Dropdown>
          </Menu>
        </AppShell.Navbar>

        <AppShell.Main>
          <Box style={{ height: '100vh', display: 'flex', flexDirection: 'column', background: 'var(--fondo)' }}>
            <Group className={styles.barra} wrap="nowrap">
              <Group gap="sm" wrap="nowrap" style={{ minWidth: 0 }}>
                <Burger opened={navAbierto} onClick={alternarNav} size="sm" aria-label="Mostrar u ocultar el menú" />
                <Breadcrumbs separator="›" c="dimmed" style={{ minWidth: 0 }}>
                  {grupoActual?.titulo && <Text size="sm">{grupoActual.titulo}</Text>}
                  <Text size="sm" fw={600} c="dark.8">{moduloActual?.label ?? 'SAL'}</Text>
                </Breadcrumbs>
              </Group>
              <UnstyledButton className={styles.buscar} onClick={() => spotlight.open()} visibleFrom="sm">
                <IconSearch size={16} />
                <span style={{ flex: 1 }}>Buscar</span>
                <Kbd size="xs">Ctrl K</Kbd>
              </UnstyledButton>
            </Group>

            <Box style={{ flex: 1, minHeight: 0, padding: pantallaCompleta ? 0 : 'var(--mantine-spacing-md)', overflow: 'hidden' }}>
              <Outlet />
            </Box>
          </Box>
        </AppShell.Main>
      </AppShell>

      <Spotlight
        actions={acciones}
        shortcut={['mod + K']}
        nothingFound="No hay módulos con ese nombre."
        highlightQuery
        searchProps={{ placeholder: 'Ir a un módulo…', leftSection: <IconSearch size={18} /> }}
      />

      <Modal opened={cuentaAbierta} onClose={cerrarCuenta} title="Mi cuenta" centered>
        <FormCambiarPassword
          titulo="Cambiar mi contraseña"
          onListo={() => { cerrarCuenta(); window.alert('Contraseña actualizada.') }}
          onCancelar={cerrarCuenta}
        />
      </Modal>
    </>
  )
}
