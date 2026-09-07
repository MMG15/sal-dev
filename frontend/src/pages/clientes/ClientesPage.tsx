import { useState, useEffect, type FormEvent } from 'react'
import { apiFetch } from '../../lib/api'
import styles from './ClientesPage.module.css'

interface Empresa {
  idEmpresa: number
  razonSocial: string
  cuit: string | null
  condicionIva: string | null
  email: string | null
  telefono: string | null
  direccion: string | null
  estado: string
  cantidadClientes: number
}

interface Cliente {
  idCliente: number
  nombre: string
  apellido: string
  email: string | null
  telefono: string | null
  cuit: string | null
  condicionIva: string | null
  usuarioWeb: string | null
  estado: string
  idEmpresa: number | null
  nombreEmpresa: string | null
  createdAt: string
}

type Tab = 'clientes' | 'empresas'

/* Condiciones frente al IVA (AFIP) — comunes a clientes particulares y empresas */
const CONDICIONES_IVA = [
  'Responsable Inscripto',
  'Monotributista',
  'Exento',
  'Consumidor Final',
]

/* ── CLIENTES ── */
const clienteVacio = () => ({
  idEmpresa: null as number | null,
  nombre: '',
  apellido: '',
  email: '',
  telefono: '',
  cuit: '',
  condicionIva: '',
  usuarioWeb: '',
})

/* ── EMPRESAS ── */
const empresaVacia = () => ({
  razonSocial: '',
  cuit: '',
  condicionIva: '',
  email: '',
  telefono: '',
  direccion: '',
})

export default function ClientesPage() {
  const [tab, setTab] = useState<Tab>('clientes')
  const [clientes, setClientes] = useState<Cliente[]>([])
  const [empresas, setEmpresas] = useState<Empresa[]>([])
  const [busqueda, setBusqueda] = useState('')
  const [cargando, setCargando] = useState(true)

  /* modales */
  const [modalCliente, setModalCliente] = useState(false)
  const [modalEmpresa, setModalEmpresa] = useState(false)
  const [clienteForm, setClienteForm] = useState(clienteVacio())
  const [empresaForm, setEmpresaForm] = useState(empresaVacia())
  const [editandoCliente, setEditandoCliente] = useState<number | null>(null)
  const [editandoEmpresa, setEditandoEmpresa] = useState<number | null>(null)
  const [guardando, setGuardando] = useState(false)
  const [errorModal, setErrorModal] = useState('')

  useEffect(() => {
    cargarDatos()
  }, [])

  async function cargarDatos() {
    setCargando(true)
    const [resC, resE] = await Promise.all([
      apiFetch('/api/clientes'),
      apiFetch('/api/empresas'),
    ])
    if (resC.ok) setClientes(await resC.json())
    if (resE.ok) setEmpresas(await resE.json())
    setCargando(false)
  }

  const clientesFiltrados = clientes.filter(c => {
    if (!busqueda) return true
    const b = busqueda.toLowerCase()
    return (
      c.nombre.toLowerCase().includes(b) ||
      c.apellido.toLowerCase().includes(b) ||
      (c.email ?? '').toLowerCase().includes(b) ||
      (c.nombreEmpresa ?? '').toLowerCase().includes(b)
    )
  })

  const empresasFiltradas = empresas.filter(e => {
    if (!busqueda) return true
    const b = busqueda.toLowerCase()
    return (
      e.razonSocial.toLowerCase().includes(b) ||
      (e.cuit ?? '').includes(b)
    )
  })

  /* ── CLIENTE: abrir modal ── */
  function abrirNuevoCliente() {
    setClienteForm(clienteVacio())
    setEditandoCliente(null)
    setErrorModal('')
    setModalCliente(true)
  }

  function abrirEditarCliente(c: Cliente) {
    setClienteForm({
      idEmpresa: c.idEmpresa,
      nombre: c.nombre,
      apellido: c.apellido,
      email: c.email ?? '',
      telefono: c.telefono ?? '',
      cuit: c.cuit ?? '',
      condicionIva: c.condicionIva ?? '',
      usuarioWeb: c.usuarioWeb ?? '',
    })
    setEditandoCliente(c.idCliente)
    setErrorModal('')
    setModalCliente(true)
  }

  async function guardarCliente(e: FormEvent) {
    e.preventDefault()
    setGuardando(true)
    setErrorModal('')
    const body = {
      idEmpresa: clienteForm.idEmpresa || null,
      nombre: clienteForm.nombre.trim(),
      apellido: clienteForm.apellido.trim(),
      email: clienteForm.email.trim() || null,
      telefono: clienteForm.telefono.trim() || null,
      cuit: clienteForm.cuit.trim() || null,
      condicionIva: clienteForm.condicionIva || null,
      usuarioWeb: clienteForm.usuarioWeb.trim() || null,
    }
    const res = editandoCliente
      ? await apiFetch(`/api/clientes/${editandoCliente}`, { method: 'PUT', body: JSON.stringify(body) })
      : await apiFetch('/api/clientes', { method: 'POST', body: JSON.stringify(body) })

    if (res.ok) {
      setModalCliente(false)
      cargarDatos()
    } else {
      const data = await res.json().catch(() => ({}))
      setErrorModal(data.error ?? data.title ?? 'Error al guardar.')
    }
    setGuardando(false)
  }

  /* ── EMPRESA: abrir modal ── */
  function abrirNuevaEmpresa() {
    setEmpresaForm(empresaVacia())
    setEditandoEmpresa(null)
    setErrorModal('')
    setModalEmpresa(true)
  }

  function abrirEditarEmpresa(e: Empresa) {
    setEmpresaForm({
      razonSocial: e.razonSocial,
      cuit: e.cuit ?? '',
      condicionIva: e.condicionIva ?? '',
      email: e.email ?? '',
      telefono: e.telefono ?? '',
      direccion: e.direccion ?? '',
    })
    setEditandoEmpresa(e.idEmpresa)
    setErrorModal('')
    setModalEmpresa(true)
  }

  async function guardarEmpresa(e: FormEvent) {
    e.preventDefault()
    setGuardando(true)
    setErrorModal('')
    const body = {
      razonSocial: empresaForm.razonSocial.trim(),
      cuit: empresaForm.cuit.trim() || null,
      condicionIva: empresaForm.condicionIva || null,
      email: empresaForm.email.trim() || null,
      telefono: empresaForm.telefono.trim() || null,
      direccion: empresaForm.direccion.trim() || null,
    }
    const res = editandoEmpresa
      ? await apiFetch(`/api/empresas/${editandoEmpresa}`, { method: 'PUT', body: JSON.stringify(body) })
      : await apiFetch('/api/empresas', { method: 'POST', body: JSON.stringify(body) })

    if (res.ok) {
      setModalEmpresa(false)
      cargarDatos()
    } else {
      const data = await res.json().catch(() => ({}))
      setErrorModal(data.error ?? data.title ?? 'Error al guardar.')
    }
    setGuardando(false)
  }

  return (
    <div>
      {/* Cabecera */}
      <div className={styles.cabecera}>
        <h2 className={styles.titulo}>Clientes y empresas</h2>
        <div className={styles.accionesCabecera}>
          <button
            className={tab === 'clientes' ? styles.btnPrimario : styles.btnSecundario}
            onClick={abrirNuevoCliente}
          >
            + Nuevo cliente
          </button>
          <button
            className={tab === 'empresas' ? styles.btnPrimario : styles.btnSecundario}
            onClick={abrirNuevaEmpresa}
          >
            + Nueva empresa
          </button>
        </div>
      </div>

      {/* Tabs + búsqueda */}
      <div className={styles.controles}>
        <div className={styles.tabs}>
          <button
            className={`${styles.tab} ${tab === 'clientes' ? styles.tabActivo : ''}`}
            onClick={() => { setTab('clientes'); setBusqueda('') }}
          >
            Clientes ({clientes.length})
          </button>
          <button
            className={`${styles.tab} ${tab === 'empresas' ? styles.tabActivo : ''}`}
            onClick={() => { setTab('empresas'); setBusqueda('') }}
          >
            Empresas ({empresas.length})
          </button>
        </div>
        <input
          className={styles.buscador}
          type="search"
          placeholder={tab === 'clientes' ? 'Buscar por nombre, email o empresa…' : 'Buscar por razón social o CUIT…'}
          value={busqueda}
          onChange={e => setBusqueda(e.target.value)}
        />
      </div>

      {/* Tabla */}
      {cargando ? (
        <p className={styles.cargando}>Cargando…</p>
      ) : tab === 'clientes' ? (
        <TablaClientes
          clientes={clientesFiltrados}
          onEditar={abrirEditarCliente}
        />
      ) : (
        <TablaEmpresas
          empresas={empresasFiltradas}
          onEditar={abrirEditarEmpresa}
        />
      )}

      {/* Modal cliente */}
      {modalCliente && (
        <Modal titulo={editandoCliente ? 'Editar cliente' : 'Nuevo cliente'} onCerrar={() => setModalCliente(false)}>
          <form onSubmit={guardarCliente} className={styles.form}>
            <div className={styles.fila2}>
              <Campo label="Nombre *">
                <input required value={clienteForm.nombre} onChange={e => setClienteForm(f => ({ ...f, nombre: e.target.value }))} />
              </Campo>
              <Campo label="Apellido *">
                <input required value={clienteForm.apellido} onChange={e => setClienteForm(f => ({ ...f, apellido: e.target.value }))} />
              </Campo>
            </div>
            <div className={styles.fila2}>
              <Campo label="Email">
                <input type="email" value={clienteForm.email} onChange={e => setClienteForm(f => ({ ...f, email: e.target.value }))} />
              </Campo>
              <Campo label="Teléfono">
                <input value={clienteForm.telefono} onChange={e => setClienteForm(f => ({ ...f, telefono: e.target.value }))} />
              </Campo>
            </div>
            <div className={styles.fila2}>
              <Campo label="Empresa">
                <select
                  value={clienteForm.idEmpresa ?? ''}
                  onChange={e => setClienteForm(f => ({ ...f, idEmpresa: e.target.value ? Number(e.target.value) : null }))}
                >
                  <option value="">— Sin empresa —</option>
                  {empresas.map(e => (
                    <option key={e.idEmpresa} value={e.idEmpresa}>{e.razonSocial}</option>
                  ))}
                </select>
              </Campo>
              <Campo label="Usuario web (portal)">
                <input value={clienteForm.usuarioWeb} onChange={e => setClienteForm(f => ({ ...f, usuarioWeb: e.target.value }))} placeholder="email o usuario" />
              </Campo>
            </div>
            <div className={styles.fila2}>
              <Campo label="CUIT / DNI">
                <input value={clienteForm.cuit} placeholder="Para facturar" onChange={e => setClienteForm(f => ({ ...f, cuit: e.target.value }))} />
              </Campo>
              <Campo label="Condición frente al IVA">
                <select
                  value={clienteForm.condicionIva}
                  onChange={e => setClienteForm(f => ({ ...f, condicionIva: e.target.value }))}
                >
                  <option value="">— Sin especificar —</option>
                  {CONDICIONES_IVA.map(c => <option key={c} value={c}>{c}</option>)}
                </select>
              </Campo>
            </div>
            {errorModal && <p className={styles.errorModal}>{errorModal}</p>}
            <div className={styles.accionesModal}>
              <button type="button" className={styles.btnSecundario} onClick={() => setModalCliente(false)}>Cancelar</button>
              <button type="submit" className={styles.btnPrimario} disabled={guardando}>
                {guardando ? 'Guardando…' : 'Guardar'}
              </button>
            </div>
          </form>
        </Modal>
      )}

      {/* Modal empresa */}
      {modalEmpresa && (
        <Modal titulo={editandoEmpresa ? 'Editar empresa' : 'Nueva empresa'} onCerrar={() => setModalEmpresa(false)}>
          <form onSubmit={guardarEmpresa} className={styles.form}>
            <Campo label="Razón social *">
              <input required value={empresaForm.razonSocial} onChange={e => setEmpresaForm(f => ({ ...f, razonSocial: e.target.value }))} />
            </Campo>
            <div className={styles.fila2}>
              <Campo label="CUIT">
                <input value={empresaForm.cuit} placeholder="XX-XXXXXXXX-X" onChange={e => setEmpresaForm(f => ({ ...f, cuit: e.target.value }))} />
              </Campo>
              <Campo label="Condición frente al IVA">
                <select
                  value={empresaForm.condicionIva}
                  onChange={e => setEmpresaForm(f => ({ ...f, condicionIva: e.target.value }))}
                >
                  <option value="">— Sin especificar —</option>
                  {CONDICIONES_IVA.map(c => <option key={c} value={c}>{c}</option>)}
                </select>
              </Campo>
            </div>
            <div className={styles.fila2}>
              <Campo label="Email">
                <input type="email" value={empresaForm.email} onChange={e => setEmpresaForm(f => ({ ...f, email: e.target.value }))} />
              </Campo>
              <Campo label="Teléfono">
                <input value={empresaForm.telefono} onChange={e => setEmpresaForm(f => ({ ...f, telefono: e.target.value }))} />
              </Campo>
            </div>
            <Campo label="Dirección">
              <input value={empresaForm.direccion} onChange={e => setEmpresaForm(f => ({ ...f, direccion: e.target.value }))} />
            </Campo>
            {errorModal && <p className={styles.errorModal}>{errorModal}</p>}
            <div className={styles.accionesModal}>
              <button type="button" className={styles.btnSecundario} onClick={() => setModalEmpresa(false)}>Cancelar</button>
              <button type="submit" className={styles.btnPrimario} disabled={guardando}>
                {guardando ? 'Guardando…' : 'Guardar'}
              </button>
            </div>
          </form>
        </Modal>
      )}
    </div>
  )
}

/* ── Tabla clientes ── */
function TablaClientes({ clientes, onEditar }: { clientes: Cliente[], onEditar: (c: Cliente) => void }) {
  if (!clientes.length) return <p className={styles.vacio}>No hay clientes registrados.</p>
  return (
    <div className={styles.tablaWrapper}>
      <table className={styles.tabla}>
        <thead>
          <tr>
            <th>Apellido y nombre</th>
            <th>Empresa</th>
            <th>Email</th>
            <th>Teléfono</th>
            <th>CUIT / DNI</th>
            <th>Portal</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {clientes.map(c => (
            <tr key={c.idCliente}>
              <td className={styles.tdNombre}>{c.apellido}, {c.nombre}</td>
              <td>{c.nombreEmpresa ?? <span className={styles.sinDato}>—</span>}</td>
              <td>{c.email ?? <span className={styles.sinDato}>—</span>}</td>
              <td>{c.telefono ?? <span className={styles.sinDato}>—</span>}</td>
              <td>{c.cuit ?? <span className={styles.sinDato}>—</span>}</td>
              <td>
                {c.usuarioWeb
                  ? <span className={styles.badgePortal}>Habilitado</span>
                  : <span className={styles.sinDato}>—</span>
                }
              </td>
              <td>
                <button className={styles.btnEditar} onClick={() => onEditar(c)}>Editar</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

/* ── Tabla empresas ── */
function TablaEmpresas({ empresas, onEditar }: { empresas: Empresa[], onEditar: (e: Empresa) => void }) {
  if (!empresas.length) return <p className={styles.vacio}>No hay empresas registradas.</p>
  return (
    <div className={styles.tablaWrapper}>
      <table className={styles.tabla}>
        <thead>
          <tr>
            <th>Razón social</th>
            <th>CUIT</th>
            <th>Email</th>
            <th>Teléfono</th>
            <th>Clientes</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          {empresas.map(e => (
            <tr key={e.idEmpresa}>
              <td className={styles.tdNombre}>{e.razonSocial}</td>
              <td>{e.cuit ?? <span className={styles.sinDato}>—</span>}</td>
              <td>{e.email ?? <span className={styles.sinDato}>—</span>}</td>
              <td>{e.telefono ?? <span className={styles.sinDato}>—</span>}</td>
              <td>{e.cantidadClientes}</td>
              <td>
                <button className={styles.btnEditar} onClick={() => onEditar(e)}>Editar</button>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

/* ── Modal genérico ── */
function Modal({ titulo, onCerrar, children }: { titulo: string, onCerrar: () => void, children: React.ReactNode }) {
  return (
    <div className={styles.overlay} onClick={e => { if (e.target === e.currentTarget) onCerrar() }}>
      <div className={styles.modal}>
        <div className={styles.modalHeader}>
          <h3 className={styles.modalTitulo}>{titulo}</h3>
          <button className={styles.btnCerrar} onClick={onCerrar} aria-label="Cerrar">✕</button>
        </div>
        <div className={styles.modalBody}>{children}</div>
      </div>
    </div>
  )
}

/* ── Campo de formulario ── */
function Campo({ label, children }: { label: string, children: React.ReactNode }) {
  return (
    <div className={styles.campo}>
      <label>{label}</label>
      {children}
    </div>
  )
}
