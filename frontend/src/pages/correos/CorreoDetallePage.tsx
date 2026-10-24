import { useEffect, useState, useCallback } from 'react'
import { useNavigate, useParams, Link } from 'react-router-dom'
import { Alert, Badge, Button, Divider, Group, Paper, Select, Stack, Text, Textarea, Title } from '@mantine/core'
import { IconArrowLeft, IconBan, IconCheck, IconFileSpreadsheet, IconMail, IconSend, IconX } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'
import { useSesion } from '../../context/SesionContext'
import { ROLES_PRESUPUESTOS } from '../consultas/consultasComun'
import { ESTADO_CORREO_COLOR, ESTADO_CORREO_LABEL, type CorreoDetalle } from './correosComun'

export default function CorreoDetallePage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { sesion } = useSesion()
  const puedeGestionarPresupuestos = ROLES_PRESUPUESTOS.includes(sesion.rolCodigo)
  const [correo, setCorreo] = useState<CorreoDetalle | null>(null)
  const [noEncontrado, setNoEncontrado] = useState(false)
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)
  const [mostrarConvertir, setMostrarConvertir] = useState(false)
  const [mostrarResponder, setMostrarResponder] = useState(false)
  const [cuerpoRespuesta, setCuerpoRespuesta] = useState('')
  const [cambiandoPresupuesto, setCambiandoPresupuesto] = useState(false)

  const [clientes, setClientes] = useState<{ idCliente: number; nombre: string; apellido: string }[]>([])
  const [empresas, setEmpresas] = useState<{ idEmpresa: number; razonSocial: string }[]>([])
  const [idCliente, setIdCliente] = useState<string | null>(null)
  const [idEmpresa, setIdEmpresa] = useState<string | null>(null)

  const cargar = useCallback(async () => {
    const res = await apiFetch(`/api/correos-entrantes/${id}`)
    if (!res.ok) {
      setNoEncontrado(true)
      return
    }
    setCorreo(await res.json())
  }, [id])

  useEffect(() => { cargar() }, [cargar])

  useEffect(() => {
    apiFetch('/api/clientes').then(r => r.json()).then(setClientes).catch(() => {})
    apiFetch('/api/empresas').then(r => r.json()).then(setEmpresas).catch(() => {})
  }, [])

  async function enviarRespuesta() {
    if (!correo || !cuerpoRespuesta.trim()) return
    setEnviando(true)
    setError('')
    try {
      const res = await apiFetch(`/api/correos-entrantes/${correo.idCorreo}/responder`, {
        method: 'PUT',
        body: JSON.stringify({ cuerpo: cuerpoRespuesta }),
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo enviar la respuesta.')
        return
      }
      setMostrarResponder(false)
      setCuerpoRespuesta('')
      await cargar()
    } finally {
      setEnviando(false)
    }
  }

  async function marcar(accion: 'descartar') {
    if (!correo) return
    setEnviando(true)
    setError('')
    try {
      const res = await apiFetch(`/api/correos-entrantes/${correo.idCorreo}/${accion}`, { method: 'PUT' })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo completar la acción.')
        return
      }
      await cargar()
    } finally {
      setEnviando(false)
    }
  }

  async function marcarPresupuesto(idPresupuesto: number, estado: 'aceptado' | 'rechazado') {
    setCambiandoPresupuesto(true)
    setError('')
    try {
      const res = await apiFetch(`/api/presupuestos/${idPresupuesto}/estado`, {
        method: 'PUT',
        body: JSON.stringify({ estado }),
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo actualizar el presupuesto.')
        return
      }
      await cargar()
    } finally {
      setCambiandoPresupuesto(false)
    }
  }

  async function convertir() {
    if (!correo) return
    setEnviando(true)
    setError('')
    try {
      const res = await apiFetch(`/api/correos-entrantes/${correo.idCorreo}/convertir`, {
        method: 'POST',
        body: JSON.stringify({
          idCliente: idCliente ? Number(idCliente) : null,
          idEmpresa: idEmpresa ? Number(idEmpresa) : null,
        }),
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo convertir el correo en consulta.')
        return
      }
      const data = await res.json()
      navigate(`/consultas/${data.idConsulta}`)
    } finally {
      setEnviando(false)
    }
  }

  if (noEncontrado) {
    return (
      <Stack gap="md" p="xl">
        <Button variant="subtle" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/correos')} w="fit-content">
          Volver a la bandeja
        </Button>
        <Text c="dimmed">No se encontró el correo.</Text>
      </Stack>
    )
  }

  if (!correo) {
    return <Text c="dimmed" p="xl">Cargando correo…</Text>
  }

  const pendiente = correo.estado === 'pendiente'

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Button variant="subtle" color="gray" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/correos')} w="fit-content" px={0} style={{ flexShrink: 0 }}>
        Correo entrante
      </Button>

      <Group justify="space-between" align="flex-start" wrap="wrap" gap="md">
        <div>
          <Title order={2} fw={600}>{correo.asunto || '(sin asunto)'}</Title>
          <Text c="dimmed" size="sm" mt={4}>
            {correo.remitenteNombre ? `${correo.remitenteNombre} · ${correo.remitente}` : correo.remitente}
            {' · '}{new Date(correo.fechaRecibido).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' })}
          </Text>
        </div>
        <Badge color={ESTADO_CORREO_COLOR[correo.estado] ?? 'gray'} variant="light" size="lg" radius="sm">
          {ESTADO_CORREO_LABEL[correo.estado] ?? correo.estado}
        </Badge>
      </Group>

      <Paper withBorder radius="md" p="lg">
        <Text size="sm" style={{ whiteSpace: 'pre-wrap' }}>{correo.cuerpo || '(sin contenido)'}</Text>
      </Paper>

      {correo.consultaRelacionada && (
        <Alert color="grape" variant="light" style={{ flexShrink: 0 }}>
          <Stack gap="xs">
            <Text size="sm">
              Esto parece ser una respuesta a la consulta{' '}
              <Text component={Link} to={`/consultas/${correo.consultaRelacionada.idConsulta}`} c="grape.8" fw={600} span style={{ textDecoration: 'none' }}>
                {correo.consultaRelacionada.codigo}
              </Text>
              {correo.consultaRelacionada.presupuesto && ` (presupuesto ${correo.consultaRelacionada.presupuesto.codigoRpo ?? ''})`}.
            </Text>
            {puedeGestionarPresupuestos && correo.consultaRelacionada.presupuesto?.estado === 'enviado' && (
              <Group gap="sm">
                <Button
                  size="xs"
                  color="teal"
                  leftSection={<IconCheck size={14} />}
                  loading={cambiandoPresupuesto}
                  onClick={() => marcarPresupuesto(correo.consultaRelacionada!.presupuesto!.idPresupuesto, 'aceptado')}
                >
                  Marcar presupuesto como aceptado
                </Button>
                <Button
                  size="xs"
                  variant="light"
                  color="red"
                  leftSection={<IconX size={14} />}
                  loading={cambiandoPresupuesto}
                  onClick={() => marcarPresupuesto(correo.consultaRelacionada!.presupuesto!.idPresupuesto, 'rechazado')}
                >
                  Marcar como rechazado
                </Button>
              </Group>
            )}
          </Stack>
        </Alert>
      )}

      {error && <Alert color="red" variant="light" style={{ flexShrink: 0 }}>{error}</Alert>}

      {pendiente ? (
        <Paper withBorder radius="md" p="lg">
          <Text fw={600} mb="md">¿Qué hacemos con este correo?</Text>

          {mostrarResponder ? (
            <Stack gap="sm">
              <Textarea
                placeholder="Escribí la respuesta…"
                value={cuerpoRespuesta}
                onChange={e => setCuerpoRespuesta(e.currentTarget.value)}
                minRows={5}
                autosize
                autoFocus
              />
              <Text size="xs" c="dimmed">
                Se manda de verdad por mail, desde la casilla del laboratorio, a {correo.remitente}.
              </Text>
              <Group gap="sm">
                <Button variant="default" disabled={enviando} onClick={() => { setMostrarResponder(false); setCuerpoRespuesta('') }}>Cancelar</Button>
                <Button leftSection={<IconSend size={16} />} loading={enviando} disabled={!cuerpoRespuesta.trim()} onClick={enviarRespuesta}>
                  Enviar respuesta
                </Button>
              </Group>
            </Stack>
          ) : !mostrarConvertir ? (
            <Group gap="sm">
              <Button leftSection={<IconMail size={16} />} variant="default" disabled={enviando} onClick={() => setMostrarResponder(true)}>
                Responder
              </Button>
              <Button leftSection={<IconFileSpreadsheet size={16} />} disabled={enviando} onClick={() => setMostrarConvertir(true)}>
                Convertir en consulta
              </Button>
              <Button leftSection={<IconBan size={16} />} variant="light" color="red" disabled={enviando} onClick={() => marcar('descartar')}>
                Descartar
              </Button>
            </Group>
          ) : (
            <Stack gap="sm">
              <Text size="sm" c="dimmed">
                Se va a crear una Consulta formal a partir de este correo. Podés vincularla a un
                cliente o empresa ya cargado, o dejarla sin asignar (se completa después).
              </Text>
              <Group gap="sm" grow>
                <Select
                  placeholder="Cliente (opcional)"
                  clearable
                  searchable
                  value={idCliente}
                  onChange={setIdCliente}
                  data={clientes.map(c => ({ value: String(c.idCliente), label: `${c.apellido}, ${c.nombre}` }))}
                />
                <Select
                  placeholder="Empresa (opcional)"
                  clearable
                  searchable
                  value={idEmpresa}
                  onChange={setIdEmpresa}
                  data={empresas.map(e => ({ value: String(e.idEmpresa), label: e.razonSocial }))}
                />
              </Group>
              <Group gap="sm">
                <Button variant="default" disabled={enviando} onClick={() => setMostrarConvertir(false)}>Cancelar</Button>
                <Button loading={enviando} onClick={convertir}>Confirmar conversión</Button>
              </Group>
            </Stack>
          )}
        </Paper>
      ) : (
        <Paper withBorder radius="md" p="lg">
          <Text size="sm" c="dimmed">
            {correo.usuarioAccion && (
              <>Procesado por {correo.usuarioAccion.nombre} {correo.usuarioAccion.apellido}
              {correo.fechaAccion && ` el ${new Date(correo.fechaAccion).toLocaleDateString('es-AR')}`}. </>
            )}
            {correo.estado === 'convertido' && correo.consultaGenerada && (
              <>
                <Divider my="sm" />
                Se convirtió en la consulta{' '}
                <Text component={Link} to={`/consultas/${correo.consultaGenerada.idConsulta}`} c="teal.7" fw={600} span style={{ textDecoration: 'none' }}>
                  {correo.consultaGenerada.codigo}
                </Text>.
              </>
            )}
          </Text>
        </Paper>
      )}
    </Stack>
  )
}
