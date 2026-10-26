import { useEffect, useState, useCallback, type FormEvent } from 'react'
import { apiFetch } from '../../lib/api'
import { useSesion } from '../../context/SesionContext'
import CuentasCorreoTab from './CuentasCorreoTab'
import DocumentosInstitucionalesTab from './DocumentosInstitucionalesTab'
import styles from './UsuariosPage.module.css'

// ─── Tipos ────────────────────────────────────────────────────────────────────

interface Usuario {
  idUsuario: number
  nombre: string
  apellido: string
  email: string
  idRol: number
  rolCodigo: string
  rolNombre: string
  activo: boolean
  ultimoAcceso: string | null
  forzarCambioPwd: boolean
}

interface RolAsignable {
  idRol: number
  codigo: string
  nombre: string
  descripcion: string
}

interface Temporal {
  titulo: string
  email: string
  passwordTemporal: string
}

interface EntradaAuditoria {
  id: number
  fecha: string
  actor: string | null
  accion: string
  objetivo: string | null
  detalle: string | null
}

const ACCION_LABEL: Record<string, string> = {
  alta_usuario: 'Alta de usuario',
  modificar_usuario: 'Modificación de datos',
  cambio_rol: 'Cambio de rol',
  desactivar_usuario: 'Desactivación',
  activar_usuario: 'Activación',
  reset_password: 'Restablecimiento de contraseña',
  cambio_password: 'Cambio de contraseña',
  login_ok: 'Ingreso',
  login_fallido: 'Ingreso fallido',
}

function fechaHora(iso: string) {
  return new Date(iso).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' })
}

// ─── Página ───────────────────────────────────────────────────────────────────

export default function UsuariosPage() {
  const { sesion } = useSesion()
  // ROL-01 gestiona usuarios. ROL-01 y ROL-06 administran las cuentas de correo
  // (es, en los hechos, una integración externa) y consultan la auditoría.
  const puedeGestionarUsuarios = sesion.rolCodigo === 'ROL-01'
  const puedeGestionarCorreo = sesion.rolCodigo === 'ROL-01' || sesion.rolCodigo === 'ROL-06'
  const puedeGestionarDocumentos = sesion.rolCodigo === 'ROL-01' || sesion.rolCodigo === 'ROL-06'
  const [pestana, setPestana] = useState<'usuarios' | 'correos' | 'documentos' | 'auditoria'>(
    puedeGestionarUsuarios ? 'usuarios' : puedeGestionarCorreo ? 'correos' : 'auditoria'
  )

  return (
    <div className={styles.root}>
      <div className={styles.pestanas}>
        {puedeGestionarUsuarios && (
          <button className={`${styles.pestana} ${pestana === 'usuarios' ? styles.pestanaActiva : ''}`} onClick={() => setPestana('usuarios')}>
            Usuarios
          </button>
        )}
        {puedeGestionarCorreo && (
          <button className={`${styles.pestana} ${pestana === 'correos' ? styles.pestanaActiva : ''}`} onClick={() => setPestana('correos')}>
            Cuentas de correo
          </button>
        )}
        {puedeGestionarDocumentos && (
          <button className={`${styles.pestana} ${pestana === 'documentos' ? styles.pestanaActiva : ''}`} onClick={() => setPestana('documentos')}>
            Documentos institucionales
          </button>
        )}
        <button className={`${styles.pestana} ${pestana === 'auditoria' ? styles.pestanaActiva : ''}`} onClick={() => setPestana('auditoria')}>
          Registro de auditoría
        </button>
      </div>

      {pestana === 'usuarios' && puedeGestionarUsuarios && <TablaUsuarios />}
      {pestana === 'correos' && puedeGestionarCorreo && <CuentasCorreoTab />}
      {pestana === 'documentos' && puedeGestionarDocumentos && <DocumentosInstitucionalesTab />}
      {pestana === 'auditoria' && <RegistroAuditoria />}
    </div>
  )
}

// ─── Usuarios ─────────────────────────────────────────────────────────────────

function TablaUsuarios() {
  const { sesion } = useSesion()
  const [usuarios, setUsuarios] = useState<Usuario[]>([])
  const [roles, setRoles] = useState<RolAsignable[]>([])
  const [busqueda, setBusqueda] = useState('')
  const [estado, setEstado] = useState('')
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [editando, setEditando] = useState<Usuario | 'nuevo' | null>(null)
  const [temporal, setTemporal] = useState<Temporal | null>(null)

  const cargar = useCallback(async () => {
    setCargando(true)
    try {
      const params = new URLSearchParams()
      if (busqueda) params.set('busqueda', busqueda)
      if (estado) params.set('estado', estado)
      const res = await apiFetch(`/api/usuarios?${params}`)
      const data = await res.json().catch(() => null)
      if (!res.ok || !Array.isArray(data)) {
        setUsuarios([])
        setError(`No se pudieron cargar los usuarios (error ${res.status}).`)
        return
      }
      setError('')
      setUsuarios(data)
    } finally {
      setCargando(false)
    }
  }, [busqueda, estado])

  useEffect(() => { cargar() }, [cargar])

  useEffect(() => {
    apiFetch('/api/usuarios/roles')
      .then(r => r.json())
      .then(setRoles)
      .catch(() => setRoles([]))
  }, [])

  async function accion(u: Usuario, ruta: 'activar' | 'desactivar' | 'restablecer-password') {
    if (ruta === 'desactivar' && !window.confirm(`¿Desactivar a ${u.nombre} ${u.apellido}? No podrá ingresar hasta que lo actives.`)) return
    if (ruta === 'restablecer-password' && !window.confirm(`¿Restablecer la contraseña de ${u.nombre} ${u.apellido}? La actual dejará de funcionar.`)) return

    setError('')
    const res = await apiFetch(`/api/usuarios/${u.idUsuario}/${ruta}`, { method: 'POST' })
    if (!res.ok) {
      const d = await res.json().catch(() => ({}))
      setError(d.error ?? 'No se pudo completar la acción.')
      return
    }
    if (ruta === 'restablecer-password') {
      const d = await res.json()
      setTemporal({ titulo: `Nueva contraseña temporal de ${u.nombre} ${u.apellido}`, email: d.email, passwordTemporal: d.passwordTemporal })
    }
    cargar()
  }

  return (
    <>
      <div className={styles.barra}>
        <input
          className={styles.inputBusqueda}
          placeholder="Buscar por nombre o correo..."
          value={busqueda}
          onChange={e => setBusqueda(e.target.value)}
        />
        <select className={styles.select} value={estado} onChange={e => setEstado(e.target.value)}>
          <option value="">Todos</option>
          <option value="activos">Activos</option>
          <option value="inactivos">Inactivos</option>
        </select>
        <button className={styles.btnPrimario} onClick={() => setEditando('nuevo')}>+ Nuevo usuario</button>
      </div>

      {error && <div className={styles.error}>{error}</div>}

      {temporal && (
        <TemporalPanel temporal={temporal} onCerrar={() => setTemporal(null)} />
      )}

      {cargando ? (
        <div className={styles.vacio}>Cargando...</div>
      ) : usuarios.length === 0 ? (
        <div className={styles.vacio}>No hay usuarios con esos filtros.</div>
      ) : (
        <div className={styles.lista}>
          {usuarios.map(u => {
            const esYo = u.idUsuario === sesion.idUsuario
            return (
              <div key={u.idUsuario} className={`${styles.fila} ${!u.activo ? styles.filaInactiva : ''}`}>
                <div className={styles.filaPrincipal}>
                  <span className={styles.nombre}>{u.nombre} {u.apellido}{esYo ? ' (vos)' : ''}</span>
                  <span className={styles.meta}>{u.email}</span>
                </div>
                <div className={styles.filaEstado}>
                  <span className={styles.badgeRol}>{u.rolCodigo} · {u.rolNombre}</span>
                  <span className={`${styles.badge} ${u.activo ? styles.badgeActivo : styles.badgeInactivo}`}>
                    {u.activo ? 'Activo' : 'Inactivo'}
                  </span>
                  {u.forzarCambioPwd && <span className={styles.meta}>Debe cambiar la contraseña</span>}
                  <span className={styles.meta}>Último ingreso: {u.ultimoAcceso ? fechaHora(u.ultimoAcceso) : 'nunca'}</span>
                </div>
                <div className={styles.acciones}>
                  <button className={styles.btnSecundario} onClick={() => setEditando(u)}>Editar</button>
                  <button className={styles.btnSecundario} onClick={() => accion(u, 'restablecer-password')}>Restablecer contraseña</button>
                  {u.activo ? (
                    <button className={styles.btnPeligro} disabled={esYo} title={esYo ? 'No podés desactivar tu propia cuenta' : ''} onClick={() => accion(u, 'desactivar')}>Desactivar</button>
                  ) : (
                    <button className={styles.btnSecundario} onClick={() => accion(u, 'activar')}>Activar</button>
                  )}
                </div>
              </div>
            )
          })}
        </div>
      )}

      {editando && (
        <ModalUsuario
          usuario={editando === 'nuevo' ? null : editando}
          roles={roles}
          onCerrar={() => setEditando(null)}
          onGuardado={(t) => {
            setEditando(null)
            if (t) setTemporal(t)
            cargar()
          }}
        />
      )}
    </>
  )
}

function TemporalPanel({ temporal, onCerrar }: { temporal: Temporal; onCerrar: () => void }) {
  return (
    <div className={styles.temporal}>
      <strong>{temporal.titulo}</strong>
      <p>Usuario: <strong>{temporal.email}</strong></p>
      <p>Contraseña temporal: <code>{temporal.passwordTemporal}</code></p>
      <p className={styles.ayuda}>Se muestra solo esta vez. El usuario la cambia en su primer ingreso.</p>
      <button className={styles.btnSecundario} onClick={onCerrar}>Entendido</button>
    </div>
  )
}

function ModalUsuario({
  usuario,
  roles,
  onCerrar,
  onGuardado,
}: {
  usuario: Usuario | null
  roles: RolAsignable[]
  onCerrar: () => void
  onGuardado: (temporal: Temporal | null) => void
}) {
  const { sesion } = useSesion()
  const esYo = usuario?.idUsuario === sesion.idUsuario
  const [nombre, setNombre] = useState(usuario?.nombre ?? '')
  const [apellido, setApellido] = useState(usuario?.apellido ?? '')
  const [email, setEmail] = useState(usuario?.email ?? '')
  const [idRol, setIdRol] = useState(usuario ? String(usuario.idRol) : '')
  const [error, setError] = useState('')
  const [guardando, setGuardando] = useState(false)

  async function guardar(e: FormEvent) {
    e.preventDefault()
    setError('')
    if (!idRol) return setError('Elegí un rol.')

    const cuerpo = JSON.stringify({ nombre, apellido, email, idRol: Number(idRol) })
    setGuardando(true)
    try {
      const res = usuario
        ? await apiFetch(`/api/usuarios/${usuario.idUsuario}`, { method: 'PUT', body: cuerpo })
        : await apiFetch('/api/usuarios', { method: 'POST', body: cuerpo })

      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo guardar.')
        return
      }
      if (usuario) {
        onGuardado(null)
      } else {
        const d = await res.json()
        onGuardado({ titulo: `Usuario creado: ${nombre} ${apellido}`, email: d.email, passwordTemporal: d.passwordTemporal })
      }
    } finally {
      setGuardando(false)
    }
  }

  return (
    <div className={styles.overlay} onClick={onCerrar}>
      <form className={styles.modal} onClick={e => e.stopPropagation()} onSubmit={guardar}>
        <h2 className={styles.modalTitulo}>{usuario ? 'Editar usuario' : 'Nuevo usuario'}</h2>

        <label className={styles.campo}>
          <span>Nombre</span>
          <input value={nombre} onChange={e => setNombre(e.target.value)} required />
        </label>
        <label className={styles.campo}>
          <span>Apellido</span>
          <input value={apellido} onChange={e => setApellido(e.target.value)} required />
        </label>
        <label className={styles.campo}>
          <span>Correo electrónico (usuario de ingreso)</span>
          <input type="email" value={email} onChange={e => setEmail(e.target.value)} required />
        </label>
        <label className={styles.campo}>
          <span>Rol</span>
          <select value={idRol} onChange={e => setIdRol(e.target.value)} disabled={esYo}>
            <option value="">Elegir rol...</option>
            {roles.map(r => (
              <option key={r.idRol} value={r.idRol}>{r.codigo} · {r.nombre}</option>
            ))}
          </select>
        </label>
        {esYo && <p className={styles.ayuda}>No podés cambiar tu propio rol.</p>}

        {error && <div className={styles.error}>{error}</div>}

        <div className={styles.modalAcciones}>
          <button type="button" className={styles.btnSecundario} onClick={onCerrar} disabled={guardando}>Cancelar</button>
          <button type="submit" className={styles.btnPrimario} disabled={guardando}>
            {guardando ? 'Guardando…' : usuario ? 'Guardar cambios' : 'Crear usuario'}
          </button>
        </div>
      </form>
    </div>
  )
}

// ─── Registro de auditoría ────────────────────────────────────────────────────

function RegistroAuditoria() {
  const [items, setItems] = useState<EntradaAuditoria[]>([])
  const [total, setTotal] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [accion, setAccion] = useState('')
  const [cargando, setCargando] = useState(true)
  const POR_PAGINA = 50

  const cargar = useCallback(async () => {
    setCargando(true)
    try {
      const params = new URLSearchParams({ pagina: String(pagina), porPagina: String(POR_PAGINA) })
      if (busqueda) params.set('busqueda', busqueda)
      if (accion) params.set('accion', accion)
      const res = await apiFetch(`/api/auditoria?${params}`)
      const data = await res.json().catch(() => null)
      if (!res.ok || !data?.items) {
        setItems([])
        setTotal(0)
        return
      }
      setItems(data.items)
      setTotal(data.total)
    } finally {
      setCargando(false)
    }
  }, [pagina, busqueda, accion])

  useEffect(() => { cargar() }, [cargar])

  return (
    <>
      <div className={styles.barra}>
        <input
          className={styles.inputBusqueda}
          placeholder="Buscar por usuario o correo..."
          value={busqueda}
          onChange={e => { setBusqueda(e.target.value); setPagina(1) }}
        />
        <select className={styles.select} value={accion} onChange={e => { setAccion(e.target.value); setPagina(1) }}>
          <option value="">Todas las acciones</option>
          {Object.entries(ACCION_LABEL).map(([k, v]) => (
            <option key={k} value={k}>{v}</option>
          ))}
        </select>
      </div>

      {cargando ? (
        <div className={styles.vacio}>Cargando...</div>
      ) : items.length === 0 ? (
        <div className={styles.vacio}>No hay registros con esos filtros.</div>
      ) : (
        <div className={styles.tablaAuditoria}>
          <table className={styles.tabla}>
            <thead>
              <tr><th>Fecha</th><th>Quién</th><th>Acción</th><th>Cuenta</th><th>Detalle</th></tr>
            </thead>
            <tbody>
              {items.map(e => (
                <tr key={e.id}>
                  <td className={styles.nowrap}>{fechaHora(e.fecha)}</td>
                  <td>{e.actor ?? '—'}</td>
                  <td>{ACCION_LABEL[e.accion] ?? e.accion}</td>
                  <td>{e.objetivo ?? '—'}</td>
                  <td>{e.detalle ?? ''}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {total > POR_PAGINA && (
        <div className={styles.paginacion}>
          <button disabled={pagina === 1} onClick={() => setPagina(p => p - 1)}>Anterior</button>
          <span>Pág. {pagina} de {Math.ceil(total / POR_PAGINA)}</span>
          <button disabled={pagina * POR_PAGINA >= total} onClick={() => setPagina(p => p + 1)}>Siguiente</button>
        </div>
      )}
    </>
  )
}
