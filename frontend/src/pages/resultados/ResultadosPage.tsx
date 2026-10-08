import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Alert, Badge, Group, Pagination, Paper, Progress, Select, Stack, Table, Text, TextInput, Title } from '@mantine/core'
import { IconSearch } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'

export interface MuestraResumen {
  idMuestra: number
  numeroRotulo: string
  codigoSse: string | null
  cliente: { nombre: string; apellido: string } | null
  empresa: { razonSocial: string } | null
  areas: string[]
  total: number
  pendientes: number
  porValidar: number
  validados: number
  fechaRecepcion: string
  estadoGeneral: 'pendiente' | 'por_validar' | 'completo'
}

const ESTADO_GENERAL: Record<string, { label: string; color: string }> = {
  pendiente: { label: 'Pendiente de carga', color: 'orange' },
  por_validar: { label: 'Por validar', color: 'blue' },
  completo: { label: 'Completo', color: 'teal' },
}

const POR_PAGINA = 20

export default function ResultadosPage() {
  const navigate = useNavigate()
  const [muestras, setMuestras] = useState<MuestraResumen[]>([])
  const [total, setTotal] = useState(0)
  const [pagina, setPagina] = useState(1)
  const [busqueda, setBusqueda] = useState('')
  const [filtroEstado, setFiltroEstado] = useState<string | null>(null)
  const [filtroArea, setFiltroArea] = useState<string | null>(null)
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')

  const cargarLista = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const params = new URLSearchParams({ pagina: String(pagina), porPagina: String(POR_PAGINA) })
      if (busqueda) params.set('busqueda', busqueda)
      if (filtroEstado) params.set('estado', filtroEstado)
      if (filtroArea) params.set('area', filtroArea)
      const res = await apiFetch(`/api/resultados/muestras?${params}`)
      const data = await res.json().catch(() => null)
      if (!res.ok || !data) {
        setMuestras([])
        setTotal(0)
        setError(`No se pudieron cargar las muestras (error ${res.status}).`)
        return
      }
      setMuestras(data.items)
      setTotal(data.total)
    } catch {
      setMuestras([])
      setTotal(0)
      setError('No se pudo conectar con el servidor.')
    } finally {
      setCargando(false)
    }
  }, [pagina, busqueda, filtroEstado, filtroArea])

  useEffect(() => { cargarLista() }, [cargarLista])

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Group justify="space-between" align="flex-end" wrap="nowrap">
        <div>
          <Title order={2} fw={600}>Resultados</Title>
          <Text c="dimmed" size="sm">{total} muestra{total === 1 ? '' : 's'} con análisis asignados</Text>
        </div>
      </Group>

      <Group gap="sm" wrap="nowrap">
        <TextInput
          placeholder="Buscar por rótulo, SSE, cliente…"
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
          data={[
            { value: 'pendientes', label: 'Pendientes de carga' },
            { value: 'por_validar', label: 'Por validar' },
            { value: 'completos', label: 'Completos' },
          ]}
        />
        <Select
          placeholder="Todas las áreas"
          clearable
          w={180}
          value={filtroArea}
          onChange={v => { setFiltroArea(v); setPagina(1) }}
          data={[{ value: 'MIC', label: 'MIC' }, { value: 'FQ', label: 'FQ' }]}
        />
      </Group>

      {error && <Alert color="red" variant="light">{error}</Alert>}

      <Paper withBorder radius="md" style={{ overflowX: 'auto' }}>
        {cargando ? (
          <Text c="dimmed" p="xl">Cargando muestras…</Text>
        ) : muestras.length === 0 ? (
          <Text c="dimmed" p="xl" ta="center">
            No hay muestras{busqueda || filtroEstado || filtroArea ? ' que coincidan' : ' con resultados todavía'}.
          </Text>
        ) : (
          <Table highlightOnHover verticalSpacing="md" horizontalSpacing="sm" styles={{ th: { fontSize: 13, fontWeight: 600 } }}>
            <Table.Thead style={{ background: 'var(--mantine-color-gray-0)' }}>
              <Table.Tr>
                <Table.Th>Rótulo</Table.Th>
                <Table.Th>Cliente y SSE</Table.Th>
                <Table.Th>Áreas</Table.Th>
                <Table.Th style={{ minWidth: 200 }}>Avance</Table.Th>
                <Table.Th>Estado</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {muestras.map(m => {
                const progreso = m.total > 0 ? (m.validados / m.total) * 100 : 0
                const estado = ESTADO_GENERAL[m.estadoGeneral]
                return (
                  <Table.Tr key={m.idMuestra} onClick={() => navigate(`/resultados/${m.idMuestra}`)} style={{ cursor: 'pointer' }}>
                    <Table.Td>
                      <Text ff="monospace" fw={600} size="sm" style={{ whiteSpace: 'nowrap' }}>{m.numeroRotulo}</Text>
                      <Text size="xs" c="dimmed">Recibida {new Date(m.fechaRecepcion).toLocaleDateString('es-AR')}</Text>
                    </Table.Td>
                    <Table.Td>
                      <Text fw={600} size="sm">
                        {m.cliente ? `${m.cliente.nombre} ${m.cliente.apellido}` : m.empresa?.razonSocial ?? 'Sin cliente'}
                      </Text>
                      <Text size="xs" c="dimmed" ff="monospace">{m.codigoSse ?? '—'}</Text>
                    </Table.Td>
                    <Table.Td>
                      <Group gap={6} wrap="nowrap">
                        {m.areas.map(a => (
                          <Badge key={a} color={a === 'MIC' ? 'blue' : 'violet'} variant="light" radius="sm">{a}</Badge>
                        ))}
                      </Group>
                    </Table.Td>
                    <Table.Td>
                      <Progress value={progreso} color={m.estadoGeneral === 'completo' ? 'teal' : 'orange'} size="md" radius="xl" />
                      <Text size="xs" c="dimmed" mt={4}>
                        {m.validados} de {m.total} validados · {m.porValidar} por validar
                      </Text>
                    </Table.Td>
                    <Table.Td>
                      <Badge color={estado?.color ?? 'gray'} variant="light" radius="sm">{estado?.label ?? m.estadoGeneral}</Badge>
                    </Table.Td>
                  </Table.Tr>
                )
              })}
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
