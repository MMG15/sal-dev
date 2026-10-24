import { useEffect, useState, useCallback } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Badge, Button, Divider, Group, Paper, Pill, SimpleGrid, Stack, Table, Tabs, Text, Title } from '@mantine/core'
import { IconArrowLeft, IconFileSpreadsheet, IconInfoCircle, IconUser } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'
import { useSesion } from '../../context/SesionContext'
import {
  DIAS_PENDIENTE,
  ROLES_PRESUPUESTOS,
  TIPO_ASESORAMIENTO_LABEL,
  TRANSICIONES_CONSULTA,
  PresupuestoModal,
  type ConsultaDetalle,
} from './consultasComun'

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

const fmtUsd = (n: number) => `USD ${n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
const fmtArs = (n: number) => `$ ${n.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`

export default function ConsultaDetallePage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { sesion } = useSesion()
  const puedeGestionarPresupuestos = ROLES_PRESUPUESTOS.includes(sesion.rolCodigo)

  const [consulta, setConsulta] = useState<ConsultaDetalle | null>(null)
  const [noEncontrada, setNoEncontrada] = useState(false)
  const [pestana, setPestana] = useState<string | null>('resumen')
  const [mostrarPresupuesto, setMostrarPresupuesto] = useState(false)
  const [cambiandoEstado, setCambiandoEstado] = useState(false)

  const cargar = useCallback(async () => {
    const res = await apiFetch(`/api/consultas/${id}`)
    if (!res.ok) {
      setNoEncontrada(true)
      return
    }
    setConsulta(await res.json())
  }, [id])

  useEffect(() => { cargar() }, [cargar])

  async function cambiarEstado(nuevoEstado: string) {
    if (!consulta) return
    setCambiandoEstado(true)
    try {
      await apiFetch(`/api/consultas/${consulta.idConsulta}`, { method: 'PUT', body: JSON.stringify({ estado: nuevoEstado }) })
      await cargar()
    } finally {
      setCambiandoEstado(false)
    }
  }

  if (noEncontrada) {
    return (
      <Stack gap="md" p="xl">
        <Button variant="subtle" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/consultas')} w="fit-content">
          Volver a consultas
        </Button>
        <Text c="dimmed">No se encontró la consulta.</Text>
      </Stack>
    )
  }

  if (!consulta) {
    return <Text c="dimmed" p="xl">Cargando consulta…</Text>
  }

  const transiciones = TRANSICIONES_CONSULTA[consulta.estado] ?? []
  const cliente = consulta.cliente
  const pendiente = consulta.estado === 'recibida'
    && (Date.now() - new Date(consulta.createdAt).getTime()) / 86_400_000 >= DIAS_PENDIENTE

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Button variant="subtle" color="gray" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/consultas')} w="fit-content" px={0} style={{ flexShrink: 0 }}>
        Consultas
      </Button>

      <Group justify="space-between" align="flex-start" wrap="wrap" gap="md">
        <div>
          <Group gap="sm" align="center">
            <Title order={2} fw={600} ff="monospace">{consulta.codigo ?? '—'}</Title>
            <Badge color={COLOR_ESTADO[consulta.estado] ?? 'gray'} variant="light" size="lg" radius="sm">{consulta.estado}</Badge>
            {pendiente && <Badge color="orange" variant="filled" size="lg" radius="sm">Sin responder</Badge>}
          </Group>
          <Text c="dimmed" size="sm" mt={4}>
            {cliente
              ? `${cliente.nombre} ${cliente.apellido}`
              : consulta.empresa?.razonSocial ?? 'Sin cliente asignado'}
            {' · '}
            Recibida el {new Date(consulta.fecha).toLocaleDateString('es-AR')} por {consulta.canal}
          </Text>
        </div>

        {transiciones.length > 0 && (
          <Group gap="sm">
            {transiciones.map(t => (
              <Button
                key={t.siguienteEstado}
                variant={t.siguienteEstado === 'eliminada' ? 'light' : 'default'}
                color={t.siguienteEstado === 'eliminada' ? 'red' : 'gray'}
                disabled={cambiandoEstado}
                onClick={() => cambiarEstado(t.siguienteEstado)}
              >
                {t.label}
              </Button>
            ))}
          </Group>
        )}
      </Group>

      <Tabs value={pestana} onChange={setPestana} keepMounted={false}>
        <Tabs.List>
          <Tabs.Tab value="resumen" leftSection={<IconInfoCircle size={16} />}>Resumen</Tabs.Tab>
          <Tabs.Tab value="presupuesto" leftSection={<IconFileSpreadsheet size={16} />}>
            Presupuesto
            {consulta.presupuesto && <Pill size="xs" ml={6}>1</Pill>}
          </Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="resumen" pt="lg">
          <SimpleGrid cols={{ base: 1, lg: 2 }} spacing="lg">
            <Paper withBorder radius="md" p="lg">
              <Text fw={600} mb="md">Consulta</Text>
              <Stack gap="sm">
                <Dato label="Canal" valor={consulta.canal} />
                <Dato label="Fecha de recepción" valor={new Date(consulta.fecha).toLocaleDateString('es-AR')} />
                <Dato label="Responsable" valor={consulta.responsable ? `${consulta.responsable.nombre} ${consulta.responsable.apellido}` : 'Sin asignar'} />
              </Stack>
              <Divider my="md" />
              <Text size="sm" c="dimmed" mb={4}>Descripción</Text>
              <Text size="sm" style={{ whiteSpace: 'pre-wrap' }}>
                {consulta.descripcion || 'Sin descripción.'}
              </Text>
            </Paper>

            <Paper withBorder radius="md" p="lg">
              <Group gap="xs" mb="md">
                <IconUser size={18} />
                <Text fw={600}>Cliente y empresa</Text>
              </Group>
              {cliente ? (
                <Stack gap="sm">
                  <Dato label="Nombre" valor={`${cliente.nombre} ${cliente.apellido}`} />
                  {cliente.email && <Dato label="Email" valor={cliente.email} />}
                  {cliente.telefono && <Dato label="Teléfono" valor={cliente.telefono} />}
                  <Dato label="Empresa" valor={cliente.empresa?.razonSocial ?? '—'} />
                </Stack>
              ) : consulta.empresa ? (
                <Stack gap="sm">
                  <Dato label="Empresa" valor={consulta.empresa.razonSocial} />
                </Stack>
              ) : (
                <Text c="dimmed" size="sm">Sin cliente asignado.</Text>
              )}
            </Paper>
          </SimpleGrid>
        </Tabs.Panel>

        <Tabs.Panel value="presupuesto" pt="lg">
          {consulta.presupuesto ? (
            <Stack gap="lg">
              <Group justify="space-between" align="center">
                <Group gap="sm">
                  <Text ff="monospace" fw={600}>{consulta.presupuesto.codigoRpo}</Text>
                  <Badge color={COLOR_ESTADO[consulta.presupuesto.estado] ?? 'gray'} variant="light" radius="sm">
                    {consulta.presupuesto.estado}
                  </Badge>
                </Group>
                {puedeGestionarPresupuestos && (
                  <Button variant="default" onClick={() => setMostrarPresupuesto(true)}>Editar presupuesto</Button>
                )}
              </Group>

              <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="md">
                <Paper withBorder radius="md" p="md" bg="gray.0">
                  <Text size="sm" c="dimmed">Total en dólares</Text>
                  <Text fz={24} fw={700}>{fmtUsd(consulta.presupuesto.importeDolares)}</Text>
                </Paper>
                <Paper withBorder radius="md" p="md" bg="gray.0">
                  <Text size="sm" c="dimmed">Cotización</Text>
                  <Text fz={24} fw={700}>{fmtArs(consulta.presupuesto.cotizacion)}</Text>
                </Paper>
                <Paper withBorder radius="md" p="md" bg="gray.0">
                  <Text size="sm" c="dimmed">Total en pesos</Text>
                  <Text fz={24} fw={700}>{fmtArs(consulta.presupuesto.importePesos)}</Text>
                </Paper>
              </SimpleGrid>

              <Paper withBorder radius="md" style={{ overflow: 'hidden', flexShrink: 0 }}>
                <Table verticalSpacing="sm" horizontalSpacing="md">
                  <Table.Thead style={{ background: 'var(--mantine-color-gray-0)' }}>
                    <Table.Tr>
                      <Table.Th>Área</Table.Th>
                      <Table.Th>Análisis</Table.Th>
                      <Table.Th ta="right">Precio</Table.Th>
                    </Table.Tr>
                  </Table.Thead>
                  <Table.Tbody>
                    {consulta.presupuesto.items.map(i => (
                      <Table.Tr key={i.id}>
                        <Table.Td>
                          <Badge color={i.area === 'MIC' ? 'blue' : 'violet'} variant="light" radius="sm">{i.area}</Badge>
                        </Table.Td>
                        <Table.Td>
                          <Text size="sm" fw={500}>{i.nombre}</Text>
                          <Text size="xs" c="dimmed">{i.codigo} · {i.grupoNombre}</Text>
                        </Table.Td>
                        <Table.Td ta="right">
                          <Text size="sm" ff="monospace">{fmtUsd(i.precioUsdSnapshot)}</Text>
                        </Table.Td>
                      </Table.Tr>
                    ))}
                    {consulta.presupuesto.adicionalAsesoramiento > 0 && (
                      <Table.Tr>
                        <Table.Td />
                        <Table.Td>
                          <Text size="sm" fw={500}>
                            Asesoramiento · {TIPO_ASESORAMIENTO_LABEL[consulta.presupuesto.tipoAsesoramiento ?? ''] ?? consulta.presupuesto.tipoAsesoramiento}
                          </Text>
                        </Table.Td>
                        <Table.Td ta="right">
                          <Text size="sm" ff="monospace">{fmtUsd(consulta.presupuesto.adicionalAsesoramiento)}</Text>
                        </Table.Td>
                      </Table.Tr>
                    )}
                  </Table.Tbody>
                </Table>
              </Paper>
            </Stack>
          ) : (
            <Paper withBorder radius="md" p="xl" ta="center">
              <Text c="dimmed" mb="md">Todavía no hay presupuesto para esta consulta.</Text>
              {puedeGestionarPresupuestos && (
                <Button onClick={() => setMostrarPresupuesto(true)}>Crear presupuesto</Button>
              )}
            </Paper>
          )}
        </Tabs.Panel>
      </Tabs>

      {mostrarPresupuesto && (
        <PresupuestoModal
          consulta={consulta}
          onGuardado={async () => {
            setMostrarPresupuesto(false)
            await cargar()
          }}
          onCerrar={() => setMostrarPresupuesto(false)}
        />
      )}
    </Stack>
  )
}

function Dato({ label, valor }: { label: string; valor: string }) {
  return (
    <Group justify="space-between" align="flex-start" wrap="nowrap" gap="md">
      <Text size="sm" c="dimmed">{label}</Text>
      <Text size="sm" fw={500} ta="right">{valor}</Text>
    </Group>
  )
}
