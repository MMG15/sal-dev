import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { ActionIcon, Alert, Badge, Button, Group, Pagination, Paper, Select, Stack, Table, Text, TextInput, Title } from '@mantine/core'
import { IconInbox, IconMailOpened, IconRefresh, IconSearch } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'
import { ESTADOS_CORREO, ESTADO_CORREO_COLOR, ESTADO_CORREO_LABEL, type CorreoResumen } from './correosComun'

const POR_PAGINA = 20

export default function CorreosPage() {
  const navigate = useNavigate()
  const [correos, setCorreos] = useState<CorreoResumen[]>([])
  const [total, setTotal] = useState(0)
  const [pendientes, setPendientes] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [filtroEstado, setFiltroEstado] = useState<string | null>('pendiente')
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [revisando, setRevisando] = useState(false)
  const [mensajeRevision, setMensajeRevision] = useState('')

  const cargarLista = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const params = new URLSearchParams({ pagina: String(pagina), porPagina: String(POR_PAGINA) })
      if (busqueda) params.set('busqueda', busqueda)
      if (filtroEstado) params.set('estado', filtroEstado)
      const res = await apiFetch(`/api/correos-entrantes?${params}`)
      const data = await res.json().catch(() => null)
      if (!res.ok || !data) {
        setCorreos([])
        setTotal(0)
        setError(`No se pudo cargar la bandeja (error ${res.status}).`)
        return
      }
      setCorreos(data.items)
      setTotal(data.total)
      setPendientes(data.pendientes)
    } catch {
      setCorreos([])
      setTotal(0)
      setError('No se pudo conectar con el servidor.')
    } finally {
      setCargando(false)
    }
  }, [pagina, busqueda, filtroEstado])

  useEffect(() => { cargarLista() }, [cargarLista])

  async function revisarAhora() {
    setRevisando(true)
    setError('')
    setMensajeRevision('')
    try {
      const res = await apiFetch('/api/cuentas-correo')
      const cuentas = await res.json().catch(() => null)
      if (!res.ok || !Array.isArray(cuentas)) {
        setError('No se pudieron obtener las casillas configuradas.')
        return
      }
      const habilitadas = cuentas.filter((c: { habilitada: boolean }) => c.habilitada)
      if (habilitadas.length === 0) {
        setError('No hay ninguna casilla de correo habilitada para revisar.')
        return
      }
      let importados = 0
      for (const cuenta of habilitadas) {
        const r = await apiFetch(`/api/cuentas-correo/${cuenta.idCuenta}/revisar-ahora`, { method: 'POST' })
        const d = await r.json().catch(() => ({}))
        if (r.ok) importados += d.importados ?? 0
      }
      setMensajeRevision(importados > 0 ? `Se importaron ${importados} correo(s) nuevo(s).` : 'No había correos nuevos.')
      await cargarLista()
    } finally {
      setRevisando(false)
    }
  }

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Group justify="space-between" align="flex-end" wrap="nowrap">
        <div>
          <Title order={2} fw={600}>Correo entrante</Title>
          <Text c="dimmed" size="sm">
            Todo lo que llega al mail de consultas del laboratorio. No todo es un pedido real:
            revisalo y decidí si se convierte en consulta, se responde o se descarta.
          </Text>
        </div>
        <Group gap="xs" wrap="nowrap">
          <Button
            variant="default"
            leftSection={<IconMailOpened size={16} />}
            onClick={revisarAhora}
            loading={revisando}
            title="Conecta ahora mismo con la casilla de correo para buscar mensajes nuevos"
          >
            Revisar correo ahora
          </Button>
          <ActionIcon variant="default" size="lg" onClick={cargarLista} loading={cargando} title="Refrescar bandeja">
            <IconRefresh size={18} />
          </ActionIcon>
        </Group>
      </Group>

      {pendientes > 0 && (
        <Alert color="orange" variant="light" icon={<IconInbox size={18} />} style={{ flexShrink: 0 }}>
          <Group justify="space-between" wrap="nowrap">
            <Text size="sm">{pendientes} correo{pendientes === 1 ? '' : 's'} sin revisar.</Text>
            <Text
              size="sm"
              fw={600}
              c="orange.8"
              style={{ cursor: 'pointer' }}
              onClick={() => { setFiltroEstado('pendiente'); setPagina(1) }}
            >
              Ver pendientes
            </Text>
          </Group>
        </Alert>
      )}

      {error && <Alert color="red" variant="light" style={{ flexShrink: 0 }}>{error}</Alert>}
      {mensajeRevision && <Alert color="teal" variant="light" style={{ flexShrink: 0 }}>{mensajeRevision}</Alert>}

      <Group gap="sm" wrap="nowrap">
        <TextInput
          placeholder="Buscar por remitente o asunto…"
          leftSection={<IconSearch size={16} />}
          value={busqueda}
          onChange={e => { setBusqueda(e.currentTarget.value); setPagina(1) }}
          style={{ flex: 1 }}
        />
        <Select
          placeholder="Todos los estados"
          clearable
          w={220}
          value={filtroEstado}
          onChange={v => { setFiltroEstado(v); setPagina(1) }}
          data={ESTADOS_CORREO.map(e => ({ value: e, label: ESTADO_CORREO_LABEL[e] }))}
        />
      </Group>

      <Paper withBorder radius="md" style={{ overflowX: 'auto', flexShrink: 0 }}>
        {cargando ? (
          <Text c="dimmed" p="xl">Cargando bandeja…</Text>
        ) : correos.length === 0 ? (
          <Text c="dimmed" p="xl" ta="center">
            {busqueda || filtroEstado ? 'No hay correos que coincidan.' : 'La bandeja está vacía.'}
          </Text>
        ) : (
          <Table highlightOnHover verticalSpacing="md" horizontalSpacing="sm" styles={{ th: { fontSize: 13, fontWeight: 600 } }}>
            <Table.Thead style={{ background: 'var(--mantine-color-gray-0)' }}>
              <Table.Tr>
                <Table.Th>Remitente</Table.Th>
                <Table.Th>Asunto</Table.Th>
                <Table.Th>Recibido</Table.Th>
                <Table.Th>Estado</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {correos.map(c => (
                <Table.Tr key={c.idCorreo} onClick={() => navigate(`/correos/${c.idCorreo}`)} style={{ cursor: 'pointer' }}>
                  <Table.Td>
                    <Text fw={600} size="sm">{c.remitenteNombre || c.remitente}</Text>
                    {c.remitenteNombre && <Text size="xs" c="dimmed">{c.remitente}</Text>}
                  </Table.Td>
                  <Table.Td maw={420}>
                    <Text size="sm" fw={500}>{c.asunto || '(sin asunto)'}</Text>
                    {c.resumen && <Text size="sm" c="dimmed" lineClamp={1}>{c.resumen}</Text>}
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm" style={{ whiteSpace: 'nowrap' }}>{new Date(c.fechaRecibido).toLocaleString('es-AR', { dateStyle: 'short', timeStyle: 'short' })}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Group gap={6} wrap="nowrap">
                      <Badge color={ESTADO_CORREO_COLOR[c.estado] ?? 'gray'} variant="light" radius="sm">
                        {ESTADO_CORREO_LABEL[c.estado] ?? c.estado}
                      </Badge>
                      {c.idConsultaRelacionada && (
                        <Badge color="grape" variant="light" radius="sm" title="Parece ser una respuesta a una consulta ya existente">
                          Respuesta a consulta
                        </Badge>
                      )}
                    </Group>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        )}
      </Paper>

      {total > POR_PAGINA && (
        <Group justify="center">
          <Pagination total={Math.ceil(total / POR_PAGINA)} value={pagina} onChange={setPagina} />
        </Group>
      )}
    </Stack>
  )
}
