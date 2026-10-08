import { useState, type FormEvent } from 'react'
import { Button, Paper, PasswordInput, Stack, Text, TextInput, Title } from '@mantine/core'
import { IconAlertCircle, IconLogin2, IconUserCircle, IconUserShield } from '@tabler/icons-react'
import type { SesionUsuario } from '../types'
import styles from './Login.module.css'

interface Props {
  onLogin: (sesion: SesionUsuario) => void
}

export default function Login({ onLogin }: Props) {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [cargando, setCargando] = useState(false)
  // Ingreso de clientes externos al portal (cuenta separada del personal)
  const [modoCliente, setModoCliente] = useState(false)

  function cambiarModo() {
    setModoCliente(m => !m)
    setError('')
    setPassword('')
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError('')
    setCargando(true)

    try {
      const res = await fetch(modoCliente ? '/api/portal/login' : '/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password }),
      })

      if (!res.ok) {
        const data = await res.json().catch(() => ({}))
        setError(data.mensaje ?? 'Error al iniciar sesión.')
        return
      }

      const data: SesionUsuario = await res.json()
      onLogin(data)
    } catch {
      setError('No se pudo conectar con el servidor. Verificá tu conexión.')
    } finally {
      setCargando(false)
    }
  }

  return (
    <div className={styles.pagina}>
      <aside className={styles.panelMarca}>
        <div className={styles.marcaContenido}>
          <div className={styles.logoTarjeta}>
            <img
              src="/logo-Laboratorio-Graffigna-editado_droid-serif.png"
              alt="Laboratorio Control de Calidad Dr. Alberto Graffigna — UCCuyo"
              className={styles.logo}
            />
          </div>
          <Title order={1} className={styles.titulo}>SAL</Title>
          <Text className={styles.subtitulo}>Sistema de Seguimiento de Análisis de Laboratorio</Text>
          <Text className={styles.pie}>Universidad Católica de Cuyo</Text>
        </div>
      </aside>

      <main className={styles.panelFormulario}>
        <Paper className={styles.tarjeta} radius="lg" p={36} withBorder={false}>
          <Stack gap={4} mb="xl">
            <Title order={2} className={styles.bienvenida}>
              {modoCliente ? 'Portal de resultados' : 'Iniciar sesión'}
            </Title>
            <Text c="dimmed" size="md">
              {modoCliente
                ? 'Ingresá con el correo y la contraseña que te entregó el laboratorio.'
                : 'Ingresá con tu correo institucional y tu contraseña.'}
            </Text>
          </Stack>

          <form onSubmit={handleSubmit} noValidate>
            <Stack gap="lg">
              <TextInput
                label="Correo electrónico"
                type="email"
                autoComplete="email"
                placeholder="usuario@ejemplo.com"
                value={email}
                onChange={e => setEmail(e.currentTarget.value)}
                withAsterisk={false}
                disabled={cargando}
                size="md"
              />
              <PasswordInput
                label="Contraseña"
                autoComplete="current-password"
                placeholder="••••••••"
                value={password}
                onChange={e => setPassword(e.currentTarget.value)}
                withAsterisk={false}
                disabled={cargando}
                size="md"
              />

              {error && (
                <div className={styles.error} role="alert">
                  <IconAlertCircle size={18} />
                  <span>{error}</span>
                </div>
              )}

              <Button
                type="submit"
                size="md"
                fullWidth
                loading={cargando}
                leftSection={<IconLogin2 size={18} />}
                className={styles.boton}
              >
                Iniciar sesión
              </Button>
            </Stack>
          </form>

          <div className={styles.separador} />

          <Button
            variant="subtle"
            color="teal"
            fullWidth
            disabled={cargando}
            onClick={cambiarModo}
            leftSection={modoCliente ? <IconUserShield size={18} /> : <IconUserCircle size={18} />}
          >
            {modoCliente ? 'Soy del personal' : 'Ingreso para clientes'}
          </Button>
        </Paper>

        <Text size="sm" c="dimmed" className={styles.derechos}>
          © Laboratorio Control de Calidad Dr. Alberto Graffigna
        </Text>
      </main>
    </div>
  )
}
