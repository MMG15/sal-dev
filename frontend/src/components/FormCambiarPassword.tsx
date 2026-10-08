import { useState, type FormEvent } from 'react'
import { apiFetch } from '../lib/api'
import styles from './FormCambiarPassword.module.css'

interface Props {
  titulo: string
  ayuda?: string
  textoBoton?: string
  onListo: () => void
  onCancelar?: () => void
}

// Cambio de la contraseña propia (primer ingreso o desde "Mi cuenta")
export default function FormCambiarPassword({ titulo, ayuda, textoBoton = 'Guardar contraseña', onListo, onCancelar }: Props) {
  const [actual, setActual] = useState('')
  const [nueva, setNueva] = useState('')
  const [repetir, setRepetir] = useState('')
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)

  async function guardar(e: FormEvent) {
    e.preventDefault()
    setError('')
    if (nueva.length < 8) return setError('La nueva contraseña debe tener al menos 8 caracteres.')
    if (nueva !== repetir) return setError('Las contraseñas nuevas no coinciden.')

    setEnviando(true)
    try {
      const res = await apiFetch('/api/auth/cambiar-password', {
        method: 'POST',
        body: JSON.stringify({ passwordActual: actual, passwordNueva: nueva }),
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo cambiar la contraseña.')
        return
      }
      onListo()
    } finally {
      setEnviando(false)
    }
  }

  return (
    <form className={styles.form} onSubmit={guardar}>
      <h2 className={styles.titulo}>{titulo}</h2>
      {ayuda && <p className={styles.ayuda}>{ayuda}</p>}

      <label className={styles.campo}>
        <span>Contraseña actual</span>
        <input type="password" value={actual} onChange={e => setActual(e.target.value)} required />
      </label>
      <label className={styles.campo}>
        <span>Nueva contraseña (mínimo 8 caracteres)</span>
        <input type="password" value={nueva} onChange={e => setNueva(e.target.value)} required />
      </label>
      <label className={styles.campo}>
        <span>Repetir nueva contraseña</span>
        <input type="password" value={repetir} onChange={e => setRepetir(e.target.value)} required />
      </label>

      {error && <div className={styles.error} role="alert">{error}</div>}

      <div className={styles.acciones}>
        {onCancelar && (
          <button type="button" className={styles.btnSecundario} onClick={onCancelar} disabled={enviando}>Cancelar</button>
        )}
        <button type="submit" className={styles.btnPrimario} disabled={enviando}>
          {enviando ? 'Guardando…' : textoBoton}
        </button>
      </div>
    </form>
  )
}
