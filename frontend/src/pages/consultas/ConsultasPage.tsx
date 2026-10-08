import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Alert, Badge, Button, Group, Pagination, Paper, Select, Stack, Table, Text, TextInput, Title } from '@mantine/core'
import { IconAlertTriangle, IconPlus, IconSearch } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'
import { DIAS_PENDIENTE, ESTADOS_CONSULTA, NuevaConsultaModal, type ConsultaResumen } from './consultasComun'

// Colores de los estados de la consulta y del presupuesto
const COLOR_ESTADO: Record<string, string> = {
  recibida: 'blue',
  respondida: 'orange',
  aceptada: 'teal',
  tercerizada: 'violet',
  eliminada: 'red',
  borrador: 'gray',
  enviado: 'blue',
  aceptado: 'teal',
  rechazado: 'red',
}

const POR_PAGINA = 20

export default function ConsultasPage() {
  const navigate = useNavigate()
  const [consultas, setConsultas] = useState<ConsultaResumen[]>([])
  const [total, setTotal] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [filtroEstado, setFiltroEstado] = useState<string | null>(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [totalPendientes, setTotalPendientes] = useState(0)
  const [modalNueva, setModalNueva] = useState(false)

  const cargarLista = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const params = new URLSearchParams({ pagina: String(pagina), porPagina: String(POR_PAGINA) })
      if (busqueda) params.set('busqueda', busqueda)
      if (filtroEstado) params.set('estado', filtroEstado)
      const res = await apiFetch(`/api/consultas?${params}`)
      const data = await res.json().catch(() => null)
      if (!res.ok || !data) {
        setConsultas([])
        setTotal(0)
        setError(`No se pudieron cargar las consultas (error ${res.status}).`)
        return
      }
      setConsultas(data.items)
      setTotal(data.total)
    } catch {
      setConsultas([])
      setTotal(0)
      setError('No se pudo conectar con el servidor.')
    } finally {
      setCargando(false)
    }
  }, [pagina, busqueda, filtroEstado])

  useEffect(() => { cargarLista() }, [cargarLista])

  useEffect(() => {
    apiFetch('/api/consultas?estado=pendiente&porPagina=1')
      .then(r => r.json())
      .then(d => setTotalPendientes(d.total))
      .catch(() => {})
  }, [cargarLista])

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Group justify="space-between" align="flex-end" wrap="nowrap">
        <div>
          <Title order={2} fw={600}>Consultas</Title>
          <Text c="dimmed" size="sm">{total} consulta{total === 1 ? '' : 's'} registrada{total === 1 ? '' : 's'}</Text>
        </div>
        <Button leftSection={<IconPlus size={16} />} onClick={() => setModalNueva(true)}>
          Nueva consulta
        </Button>
      </Group>

      {error && <Alert color="red" variant="light">{error}</Alert>}

      {totalPendientes > 0 && (
        <Alert color="orange" variant="light" icon={<IconAlertTriangle size={18} />}>
          <Group justify="space-between" wrap="nowrap">
            <Text size="sm">
              {totalPendientes} consulta{totalPendientes === 1 ? '' : 's'} sin responder hace {DIAS_PENDIENTE} o más días.
            </Text>
            <Button
              size="xs"
              variant={filtroEstado === 'pendiente' ? 'filled' : 'light'}
              color="orange"
              onClick={() => { setFiltroEstado(f => (f === 'pendiente' ? null : 'pendiente')); setPagina(1) }}
            >
              {filtroEstado === 'pendiente' ? 'Ver todas' : 'Ver pendientes'}
            </Button>
          </Group>
        </Alert>
      )}

      <Group gap="sm" wrap="nowrap">
        <TextInput
          placeholder="Buscar por código, cliente, empresa…"
          leftSection={<IconSearch size={16} />}
          value={busqueda}
          onChange={e => { setBusqueda(e.currentTarget.value); setPagina(1) }}
          style={{ flex: 1 }}
        />
        <Select
          placeholder="Todos los estados"
          clearable
          w={240}
          value={filtroEstado}
          onChange={v => { setFiltroEstado(v); setPagina(1) }}
          data={[
            { value: 'pendiente', label: 'Pendientes (sin responder)' },
            ...ESTADOS_CONSULTA.map(e => ({ value: e, label: e.charAt(0).toUpperCase() + e.slice(1) })),
          ]}
        />
      </Group>

      <Paper withBorder radius="md" style={{ overflow: 'hidden' }}>
        {cargando ? (
          <Text c="dimmed" p="xl">Cargando consultas…</Text>
        ) : consultas.length === 0 ? (
          <Text c="dimmed" p="xl" ta="center">
            No hay consultas{busqueda || filtroEstado ? ' que coincidan con la búsqueda' : ' registradas todavía'}.
          </Text>
        ) : (
          <Table highlightOnHover verticalSpacing="md" horizontalSpacing="md" styles={{ th: { fontSize: 13, fontWeight: 600 } }}>
            <Table.Thead style={{ background: 'var(--mantine-color-gray-0)' }}>
              <Table.Tr>
                <Table.Th>Código</Table.Th>
                <Table.Th>Cliente y consulta</Table.Th>
                <Table.Th>Canal</Table.Th>
                <Table.Th>Fecha</Table.Th>
                <Table.Th>Estado</Table.Th>
                <Table.Th>Presupuesto</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {consultas.map(c => (
                <Table.Tr
                  key={c.idConsulta}
                  onClick={() => navigate(`/consultas/${c.idConsulta}`)}
                  style={{ cursor: 'pointer' }}
                >
                  <Table.Td>
                    <Text ff="monospace" fw={600} size="sm" style={{ whiteSpace: "nowrap" }}>{c.codigo ?? "—"}</Text>
                    {c.pendiente && (
                      <Text size="xs" c="orange.8" fw={600} mt={2}>
                        Sin responder · {c.diasSinResponder} días
                      </Text>
                    )}
                  </Table.Td>
                  <Table.Td maw={380}>
                    <Text fw={600} size="sm">
                      {c.cliente
                        ? `${c.cliente.nombre} ${c.cliente.apellido}`
                        : c.empresa?.razonSocial ?? 'Sin cliente'}
                    </Text>
                    {c.descripcion ? (
                      <Text size="sm" c="dimmed" lineClamp={1}>{c.descripcion}</Text>
                    ) : (
                      <Text size="sm" c="dimmed" fs="italic">Sin descripción</Text>
                    )}
                  </Table.Td>
                  <Table.Td>
                    <Badge variant="outline" color="gray" radius="sm">{c.canal}</Badge>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{new Date(c.createdAt).toLocaleDateString('es-AR')}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Badge color={COLOR_ESTADO[c.estado] ?? 'gray'} variant="light" radius="sm">{c.estado}</Badge>
                  </Table.Td>
                  <Table.Td>
                    {c.tienePresupuesto && c.estadoPresupuesto ? (
                      <Badge color={COLOR_ESTADO[c.estadoPresupuesto] ?? 'gray'} variant="dot" radius="sm">
                        {c.estadoPresupuesto}
                      </Badge>
                    ) : (
                      <Text size="sm" c="dimmed">Sin presupuesto</Text>
                    )}
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

      {modalNueva && (
        <NuevaConsultaModal
          onCreada={id => {
            setModalNueva(false)
            navigate(`/consultas/${id}`)
          }}
          onCerrar={() => setModalNueva(false)}
        />
      )}
    </Stack>
  )
}
