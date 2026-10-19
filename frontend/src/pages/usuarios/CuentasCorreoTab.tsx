import { useEffect, useState, useCallback, type FormEvent } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './UsuariosPage.module.css'

interface CuentaCorreo {
  idCuenta: number
  nombre: string
  servidor: string
  puerto: number
  usuario: string
  habilitada: boolean
  intervaloMinutos: number
  ultimaRevision: string | null
  ultimoError: string | null
}

function fechaHora(iso: string) {
  return new Date(iso).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' })
}

// Administración de las casillas desde las que se importan consultas (ROL-01 y ROL-06).
export default function CuentasCorreoTab() {
  const [cuentas, setCuentas] = useState<CuentaCorreo[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [editando, setEditando] = useState<CuentaCorreo | 'nueva' | null>(null)
  const [revisando, setRevisando] = useState<number | null>(null)
  const [resultadoRevision, setResultadoRevision] = useState<{ id: number; texto: string } | null>(null)

  const cargar = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const res = await apiFetch('/api/cuentas-correo')
      const data = await res.json().catch(() => null)
      if (!res.ok || !Array.isArray(data)) {
        setCuentas([])
        setError(`No se pudieron cargar las cuentas de correo (error ${res.status}).`)
        return
      }
      setCuentas(data)
    } finally {
      setCargando(false)
    }
  }, [])

  useEffect(() => { cargar() }, [cargar])

  async function alternarHabilitada(cuenta: CuentaCorreo) {
    setError('')
    const res = await apiFetch(`/api/cuentas-correo/${cuenta.idCuenta}`, {
      method: 'PUT',
      body: JSON.stringify({ nombre: cuenta.nombre, servidor: cuenta.servidor, puerto: cuenta.puerto, usuario: cuenta.usuario, habilitada: !cuenta.habilitada, intervaloMinutos: cuenta.intervaloMinutos }),
    })
    if (!res.ok) {
      const d = await res.json().catch(() => ({}))
      setError(d.error ?? 'No se pudo actualizar la cuenta.')
      return
    }
    cargar()
  }

  async function eliminar(cuenta: CuentaCorreo) {
    if (!window.confirm(`¿Eliminar "${cuenta.nombre}" (${cuenta.usuario})? No se puede deshacer.`)) return
    setError('')
    const res = await apiFetch(`/api/cuentas-correo/${cuenta.idCuenta}`, { method: 'DELETE' })
    if (!res.ok) {
      const d = await res.json().catch(() => ({}))
      setError(d.error ?? 'No se pudo eliminar la cuenta.')
      return
    }
    cargar()
  }

  async function revisarAhora(cuenta: CuentaCorreo) {
    setRevisando(cuenta.idCuenta)
    setResultadoRevision(null)
    setError('')
    try {
      const res = await apiFetch(`/api/cuentas-correo/${cuenta.idCuenta}/revisar-ahora`, { method: 'POST' })
      const d = await res.json().catch(() => ({}))
      if (!res.ok) {
        setResultadoRevision({ id: cuenta.idCuenta, texto: `No se pudo conectar: ${d.error ?? 'error desconocido'}` })
      } else {
        setResultadoRevision({ id: cuenta.idCuenta, texto: d.importados > 0 ? `Se importaron ${d.importados} correo(s) nuevo(s).` : 'Revisado: no había correos nuevos.' })
      }
      await cargar()
    } finally {
      setRevisando(null)
    }
  }

  return (
    <>
      <div className={styles.barra}>
        <p className={styles.ayuda} style={{ flex: 1 }}>
          Casillas desde las que se importan consultas a la bandeja de "Correo entrante".
          La contraseña es la "contraseña de aplicación" de Gmail, no la contraseña normal de la cuenta.
        </p>
        <button className={styles.btnPrimario} onClick={() => setEditando('nueva')}>+ Nueva cuenta</button>
      </div>

      {error && <div className={styles.error}>{error}</div>}

      {cargando ? (
        <div className={styles.vacio}>Cargando...</div>
      ) : cuentas.length === 0 ? (
        <div className={styles.vacio}>Todavía no hay ninguna cuenta de correo configurada.</div>
      ) : (
        <div className={styles.lista}>
          {cuentas.map(c => (
            <div key={c.idCuenta} className={`${styles.fila} ${!c.habilitada ? styles.filaInactiva : ''}`}>
              <div className={styles.filaPrincipal}>
                <span className={styles.nombre}>{c.nombre}</span>
                <span className={styles.meta}>{c.usuario}</span>
                <span className={styles.meta}>{c.servidor}:{c.puerto} · revisión cada {c.intervaloMinutos} min</span>
              </div>
              <div className={styles.filaEstado}>
                <span className={`${styles.badge} ${c.habilitada ? styles.badgeActivo : styles.badgeInactivo}`}>
                  {c.habilitada ? 'Habilitada' : 'Deshabilitada'}
                </span>
                <span className={styles.meta}>Última revisión: {c.ultimaRevision ? fechaHora(c.ultimaRevision) : 'todavía no se revisó'}</span>
                {c.ultimoError && <span className={styles.error} style={{ padding: '4px 8px', fontSize: 12 }}>Último error: {c.ultimoError}</span>}
                {resultadoRevision?.id === c.idCuenta && <span className={styles.meta}>{resultadoRevision.texto}</span>}
              </div>
              <div className={styles.acciones}>
                <button className={styles.btnSecundario} onClick={() => setEditando(c)}>Editar</button>
                <button className={styles.btnSecundario} disabled={revisando === c.idCuenta} onClick={() => revisarAhora(c)}>
                  {revisando === c.idCuenta ? 'Revisando...' : 'Revisar ahora'}
                </button>
                <button className={styles.btnSecundario} onClick={() => alternarHabilitada(c)}>
                  {c.habilitada ? 'Deshabilitar' : 'Habilitar'}
                </button>
                <button className={styles.btnPeligro} onClick={() => eliminar(c)}>Eliminar</button>
              </div>
            </div>
          ))}
        </div>
      )}

      {editando && (
        <ModalCuentaCorreo
          cuenta={editando === 'nueva' ? null : editando}
          onCerrar={() => setEditando(null)}
          onGuardado={() => { setEditando(null); cargar() }}
        />
      )}
    </>
  )
}

function ModalCuentaCorreo({
  cuenta,
  onCerrar,
  onGuardado,
}: {
  cuenta: CuentaCorreo | null
  onCerrar: () => void
  onGuardado: () => void
}) {
  const [nombre, setNombre] = useState(cuenta?.nombre ?? '')
  const [servidor, setServidor] = useState(cuenta?.servidor ?? 'imap.gmail.com')
  const [puerto, setPuerto] = useState(String(cuenta?.puerto ?? 993))
  const [usuario, setUsuario] = useState(cuenta?.usuario ?? '')
  const [password, setPassword] = useState('')
  const [mostrarPassword, setMostrarPassword] = useState(false)
  const [intervaloMinutos, setIntervaloMinutos] = useState(String(cuenta?.intervaloMinutos ?? 5))
  const [error, setError] = useState('')
  const [guardando, setGuardando] = useState(false)

  async function guardar(e: FormEvent) {
    e.preventDefault()
    setError('')
    // Nos quedamos solo con letras y números: descarta espacios, saltos de línea y cualquier
    // carácter invisible que se haya colado al copiar desde la página de Google.
    const passwordLimpia = password.replace(/[^a-zA-Z0-9]/g, '')
    if (!cuenta && !passwordLimpia) return setError('Ingresá la contraseña de aplicación.')

    const cuerpo = JSON.stringify({
      nombre, servidor, puerto: Number(puerto) || 993, usuario,
      password: passwordLimpia || undefined,
      habilitada: cuenta?.habilitada ?? true,
      intervaloMinutos: Number(intervaloMinutos) || 5,
    })
    setGuardando(true)
    try {
      const res = cuenta
        ? await apiFetch(`/api/cuentas-correo/${cuenta.idCuenta}`, { method: 'PUT', body: cuerpo })
        : await apiFetch('/api/cuentas-correo', { method: 'POST', body: cuerpo })

      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo guardar.')
        return
      }
      onGuardado()
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className={styles.overlay} onClick={onCerrar}>
      <form className={styles.modal} onClick={e => e.stopPropagation()} onSubmit={guardar}>
        <h2 className={styles.modalTitulo}>{cuenta ? 'Editar cuenta de correo' : 'Nueva cuenta de correo'}</h2>

        <label className={styles.campo}>
          <span>Nombre (para identificarla en la lista)</span>
          <input value={nombre} onChange={e => setNombre(e.target.value)} placeholder="Casilla principal de consultas" required />
        </label>
        <label className={styles.campo}>
          <span>Servidor IMAP</span>
          <input value={servidor} onChange={e => setServidor(e.target.value)} required />
        </label>
        <label className={styles.campo}>
          <span>Puerto</span>
          <input type="number" value={puerto} onChange={e => setPuerto(e.target.value)} required />
        </label>
        <label className={styles.campo}>
          <span>Dirección de correo</span>
          <input type="email" value={usuario} onChange={e => setUsuario(e.target.value)} autoComplete="off" required />
        </label>
        <label className={styles.campo}>
          <span>Contraseña de aplicación{cuenta ? ' (dejar vacío para no cambiarla)' : ''}</span>
          <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
            <input
              type={mostrarPassword ? 'text' : 'password'}
              value={password}
              onChange={e => setPassword(e.target.value)}
              placeholder={cuenta ? '••••••••••••••••' : ''}
              autoComplete="new-password"
              style={{ flex: 1 }}
            />
            <button type="button" className={styles.btnSecundario} onClick={() => setMostrarPassword(v => !v)}>
              {mostrarPassword ? 'Ocultar' : 'Mostrar'}
            </button>
          </div>
          {password && (
            <span className={styles.ayuda}>
              {password.replace(/[^a-zA-Z0-9]/g, '').length} caracteres válidos detectados
              {password.replace(/[^a-zA-Z0-9]/g, '').length !== 16 ? ' — una contraseña de aplicación de Google tiene 16' : ' ✓'}
            </span>
          )}
        </label>
        <label className={styles.campo}>
          <span>Revisar cada (minutos)</span>
          <input type="number" min={1} value={intervaloMinutos} onChange={e => setIntervaloMinutos(e.target.value)} required />
        </label>

        {error && <div className={styles.error}>{error}</div>}

        <div className={styles.modalAcciones}>
          <button type="button" className={styles.btnSecundario} onClick={onCerrar} disabled={guardando}>Cancelar</button>
          <button type="submit" className={styles.btnPrimario} disabled={guardando}>
            {guardando ? 'Guardando…' : cuenta ? 'Guardar cambios' : 'Crear cuenta'}
          </button>
        </div>
      </form>
    </div>
  )
}
