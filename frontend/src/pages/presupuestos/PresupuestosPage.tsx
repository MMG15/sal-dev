import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Alert, Badge, Button, Group, Pagination, Paper, Select, Stack, Table, Text, TextInput, Title } from '@mantine/core'
import { IconSearch } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'
import { ESTADOS, type PresupuestoResumen } from './presupuestosComun'

const COLOR_ESTADO: Record<string, string> = {
  borrador: 'gray',
  enviado: 'blue',
  aceptado: 'teal',
  rechazado: 'red',
}

const POR_PAGINA = 20

const fmtUsd = (n: number) => `USD ${n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
const fmtArs = (n: number) => `$ ${n.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`

export default function PresupuestosPage() {
  const navigate = useNavigate()
  const [lista, setLista] = useState<PresupuestoResumen[]>([])
  const [total, setTotal] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [filtroEstado, setFiltroEstado] = useState<string | null>(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')

  const cargarLista = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const params = new URLSearchParams({ pagina: String(pagina), porPagina: String(POR_PAGINA) })
      if (busqueda) params.set('busqueda', busqueda)
      if (filtroEstado) params.set('estado', filtroEstado)
      const res = await apiFetch(`/api/presupuestos?${params}`)
      const data = await res.json().catch(() => null)
      if (!res.ok || !data) {
        setLista([])
        setTotal(0)
        setError(`No se pudieron cargar los presupuestos (error ${res.status}).`)
        return
      }
      setLista(data.items)
      setTotal(data.total)
    } catch {
      setLista([])
      setTotal(0)
      setError('No se pudo conectar con el servidor.')
    } finally {
      setCargando(false)
    }
  }, [pagina, busqueda, filtroEstado])

  useEffect(() => { cargarLista() }, [cargarLista])

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Group justify="space-between" align="flex-end" wrap="nowrap">
        <div>
          <Title order={2} fw={600}>Presupuestos</Title>
          <Text c="dimmed" size="sm">
            {total} presupuesto{total === 1 ? '' : 's'} · se crean desde una consulta
          </Text>
        </div>
        <Button variant="default" onClick={() => navigate('/analisis')}>Catálogo de análisis</Button>
      </Group>

      <Group gap="sm" wrap="nowrap">
        <TextInput
          placeholder="Código RPO, CF, cliente, empresa…"
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
          data={ESTADOS.map(e => ({ value: e, label: e.charAt(0).toUpperCase() + e.slice(1) }))}
        />
      </Group>

      {error && <Alert color="red" variant="light">{error}</Alert>}

      <Paper withBorder radius="md" style={{ overflow: 'hidden' }}>
        {cargando ? (
          <Text c="dimmed" p="xl">Cargando presupuestos…</Text>
        ) : lista.length === 0 ? (
          <Text c="dimmed" p="xl" ta="center">
            {busqueda || filtroEstado
              ? 'Sin resultados para esa búsqueda.'
              : 'No hay presupuestos cargados. Creá uno desde una consulta.'}
          </Text>
        ) : (
          <Table highlightOnHover verticalSpacing="md" horizontalSpacing="md" styles={{ th: { fontSize: 13, fontWeight: 600 } }}>
            <Table.Thead style={{ background: 'var(--mantine-color-gray-0)' }}>
              <Table.Tr>
                <Table.Th>Código RPO</Table.Th>
                <Table.Th>Cliente</Table.Th>
                <Table.Th>Consulta</Table.Th>
                <Table.Th ta="right">Total</Table.Th>
                <Table.Th>Fecha</Table.Th>
                <Table.Th>Estado</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {lista.map(p => (
                <Table.Tr
                  key={p.idPresupuesto}
                  onClick={() => navigate(`/presupuestos/${p.idPresupuesto}`)}
                  style={{ cursor: 'pointer' }}
                >
                  <Table.Td>
                    <Text ff="monospace" fw={600} size="sm" style={{ whiteSpace: 'nowrap' }}>{p.codigoRpo ?? '—'}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text fw={600} size="sm">
                      {p.cliente
                        ? `${p.cliente.nombre} ${p.cliente.apellido}`
                        : p.empresa?.razonSocial ?? 'Sin cliente'}
                    </Text>
                    {p.cliente && p.empresa && <Text size="sm" c="dimmed">{p.empresa.razonSocial}</Text>}
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm" ff="monospace" c="dimmed" style={{ whiteSpace: 'nowrap' }}>{p.consultaCodigo ?? '—'}</Text>
                  </Table.Td>
                  <Table.Td ta="right">
                    <Text size="sm" fw={600} ff="monospace" style={{ whiteSpace: 'nowrap' }}>{fmtUsd(p.importeDolares)}</Text>
                    <Text size="xs" c="dimmed" ff="monospace" style={{ whiteSpace: 'nowrap' }}>{fmtArs(p.importePesos)}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{new Date(p.fecha).toLocaleDateString('es-AR')}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Badge color={COLOR_ESTADO[p.estado] ?? 'gray'} variant="light" radius="sm">{p.estado}</Badge>
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
