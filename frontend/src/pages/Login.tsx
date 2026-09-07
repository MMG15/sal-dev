import { useState, type FormEvent } from 'react'
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

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError('')
    setCargando(true)

    try {
      const res = await fetch('/api/auth/login', {
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
    <div className={styles.fondo}>
      <div className={styles.tarjeta}>
        <div className={styles.encabezado}>
          <img
            src="/logo-Laboratorio-Graffigna-editado_droid-serif.png"
            alt="Laboratorio Control de Calidad Dr. Alberto Graffigna — UCCuyo"
            className={styles.logoImg}
          />
          <div className={styles.divisor} />
          <p className={styles.sistemaLabel}>Sistema de Seguimiento de Análisis de Laboratorio</p>
        </div>

        <form onSubmit={handleSubmit} className={styles.formulario} noValidate>
          <div className={styles.campo}>
            <label htmlFor="email">Correo electrónico</label>
            <input
              id="email"
              type="email"
              autoComplete="email"
              value={email}
              onChange={e => setEmail(e.target.value)}
              placeholder="usuario@ejemplo.com"
              required
              disabled={cargando}
            />
          </div>

          <div className={styles.campo}>
            <label htmlFor="password">Contraseña</label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder="••••••••"
              required
              disabled={cargando}
            />
          </div>

          {error && (
            <div className={styles.error} role="alert">
              {error}
            </div>
          )}

          <button type="submit" className={styles.boton} disabled={cargando}>
            {cargando ? 'Iniciando sesión…' : 'Iniciar sesión'}
          </button>
        </form>
      </div>
    </div>
  )
}
