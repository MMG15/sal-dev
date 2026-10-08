import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Alert, Badge, Button, Group, Pagination, Paper, Select, Stack, Table, Text, TextInput, Title } from '@mantine/core'
import { IconPlus, IconSearch } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'
import { etiquetaArea } from '../../lib/areas'
import { AREAS, ESTADOS_SSE, ModalNuevaSse, type SseResumen } from './sseComun'

const COLOR_ESTADO: Record<string, string> = {
  activa: 'blue',
  analizando: 'orange',
  completada: 'teal',
  cerrada: 'gray',
  rechazada: 'red',
}

const COLOR_AREA: Record<string, string> = { MIC: 'blue', FQ: 'violet', AMBAS: 'grape' }

const POR_PAGINA = 20

const fmtUsd = (n: number) => `USD ${n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`

export default function SsePage() {
  const navigate = useNavigate()
  const [lista, setLista] = useState<SseResumen[]>([])
  const [total, setTotal] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [filtroEstado, setFiltroEstado] = useState<string | null>(null)
  const [filtroArea, setFiltroArea] = useState<string | null>(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [modalNueva, setModalNueva] = useState(false)

  const cargarLista = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const params = new URLSearchParams({ pagina: String(pagina), porPagina: String(POR_PAGINA) })
      if (busqueda) params.set('busqueda', busqueda)
      if (filtroEstado) params.set('estado', filtroEstado)
      if (filtroArea) params.set('area', filtroArea)
      const res = await apiFetch(`/api/sses?${params}`)
      const data = await res.json().catch(() => null)
      if (!res.ok || !data) {
        setLista([])
        setTotal(0)
        setError(`No se pudieron cargar las SSE (error ${res.status}).`)
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
  }, [pagina, busqueda, filtroEstado, filtroArea])

  useEffect(() => { cargarLista() }, [cargarLista])

  const nombreCliente = (s: SseResumen) =>
    s.cliente ? `${s.cliente.nombre} ${s.cliente.apellido}` : s.empresa?.razonSocial ?? 'Sin cliente'

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Group justify="space-between" align="flex-end" wrap="nowrap">
        <div>
          <Title order={2} fw={600}>SSE y rótulos</Title>
          <Text c="dimmed" size="sm">
            {total} solicitud{total === 1 ? '' : 'es'} de servicio · cada SSE puede tener un rótulo interno
          </Text>
        </div>
        <Button leftSection={<IconPlus size={16} />} onClick={() => setModalNueva(true)}>Nueva SSE</Button>
      </Group>

      <Group gap="sm" wrap="nowrap">
        <TextInput
          placeholder="Código SSE, rótulo, cliente…"
          leftSection={<IconSearch size={16} />}
          value={busqueda}
          onChange={e => { setBusqueda(e.currentTarget.value); setPagina(1) }}
          style={{ flex: 1 }}
        />
        <Select
          placeholder="Todos los estados"
          clearable
          w={200}
          value={filtroEstado}
          onChange={v => { setFiltroEstado(v); setPagina(1) }}
          data={ESTADOS_SSE.map(e => ({ value: e, label: e.charAt(0).toUpperCase() + e.slice(1) }))}
        />
        <Select
          placeholder="Todas las áreas"
          clearable
          w={180}
          value={filtroArea}
          onChange={v => { setFiltroArea(v); setPagina(1) }}
          data={AREAS.map(a => ({ value: a, label: etiquetaArea(a) }))}
        />
      </Group>

      {error && <Alert color="red" variant="light">{error}</Alert>}

      <Paper withBorder radius="md" style={{ overflowX: "auto" }}>
        {cargando ? (
          <Text c="dimmed" p="xl">Cargando SSE…</Text>
        ) : lista.length === 0 ? (
          <Text c="dimmed" p="xl" ta="center">
            {busqueda || filtroEstado || filtroArea
              ? 'Sin resultados para esa búsqueda.'
              : 'No hay SSE registradas. Creá la primera con el botón de arriba.'}
          </Text>
        ) : (
          <Table highlightOnHover verticalSpacing="md" horizontalSpacing="sm" styles={{ th: { fontSize: 13, fontWeight: 600 } }}>
            <Table.Thead style={{ background: 'var(--mantine-color-gray-0)' }}>
              <Table.Tr>
                <Table.Th>Código SSE</Table.Th>
                <Table.Th>Cliente</Table.Th>
                <Table.Th>Área</Table.Th>
                <Table.Th>Rótulo</Table.Th>
                <Table.Th ta="right">Importe</Table.Th>
                <Table.Th>Estado</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {lista.map(s => (
                <Table.Tr key={s.idSse} onClick={() => navigate(`/sse/${s.idSse}`)} style={{ cursor: 'pointer' }}>
                  <Table.Td>
                    <Text ff="monospace" fw={600} size="sm" style={{ whiteSpace: 'nowrap' }}>{s.codigo ?? '—'}</Text>
                    {s.codigoRpo && <Text size="xs" c="dimmed" ff="monospace">{s.codigoRpo}</Text>}
                  </Table.Td>
                  <Table.Td>
                    <Text fw={600} size="sm">{nombreCliente(s)}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Badge color={COLOR_AREA[s.area] ?? 'gray'} variant="light" radius="sm">{etiquetaArea(s.area)}</Badge>
                  </Table.Td>
                  <Table.Td>
                    {s.rotulo ? (
                      <Badge variant="outline" color="teal" radius="sm" ff="monospace">{s.rotulo.numeroUnico}</Badge>
                    ) : (
                      <Text size="sm" c="dimmed">Sin rótulo</Text>
                    )}
                  </Table.Td>
                  <Table.Td ta="right">
                    <Text size="sm" ff="monospace" fw={600} style={{ whiteSpace: "nowrap" }}>{fmtUsd(s.importeDolares)}</Text>
                    <Text size="xs" c="dimmed">{new Date(s.fecha).toLocaleDateString("es-AR")}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Badge color={COLOR_ESTADO[s.estado] ?? 'gray'} variant="light" radius="sm">{s.estado}</Badge>
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
        <ModalNuevaSse
          onCreada={id => { setModalNueva(false); navigate(`/sse/${id}`) }}
          onCerrar={() => setModalNueva(false)}
        />
      )}
    </Stack>
  )
}
