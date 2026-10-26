import { useEffect, useState, useCallback } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Badge, Button, Divider, Group, Paper, Pill, SimpleGrid, Stack, Table, Tabs, Text, ThemeIcon, Title } from '@mantine/core'
import { IconArrowLeft, IconClipboardText, IconFileTypePdf, IconHistory, IconTag, IconTestPipe, IconUser } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'
import { etiquetaArea } from '../../lib/areas'
import {
  CAMPO_AUDITORIA_LABEL,
  ModalAsignarCliente,
  ModalCorregirRotulo,
  ModalMuestra,
  ModalRotulo,
  type SseDetalle,
} from './sseComun'

const COLOR_ESTADO: Record<string, string> = {
  activa: 'blue',
  analizando: 'orange',
  completada: 'teal',
  cerrada: 'gray',
  rechazada: 'red',
  activo: 'teal',
  anulado: 'red',
  cancelado: 'red',
  en_proceso: 'orange',
}

const COLOR_AREA: Record<string, string> = { MIC: 'blue', FQ: 'violet', AMBAS: 'grape' }

const ESTADO_MUESTRA_LABEL: Record<string, string> = {
  en_proceso: 'En proceso',
  completada: 'Completada',
  rechazada: 'Rechazada',
}

const fmtUsd = (n: number) => `USD ${n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
const fmtArs = (n: number) => `$ ${n.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`

export default function SseDetallePage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [sse, setSse] = useState<SseDetalle | null>(null)
  const [noEncontrada, setNoEncontrada] = useState(false)
  const [pestana, setPestana] = useState<string | null>('servicio')
  const [modal, setModal] = useState<'rotulo' | 'muestra' | 'corregir' | 'cliente' | null>(null)
  const [idRotuloActivo, setIdRotuloActivo] = useState<number | null>(null)

  async function descargarPdf() {
    const res = await apiFetch(`/api/sses/${id}/pdf`)
    if (!res.ok) return
    const blob = await res.blob()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${sse?.codigo ?? 'sse'}.pdf`
    a.click()
    URL.revokeObjectURL(url)
  }

  const cargar = useCallback(async () => {
    const res = await apiFetch(`/api/sses/${id}`)
    if (!res.ok) {
      setNoEncontrada(true)
      return
    }
    setSse(await res.json())
  }, [id])

  useEffect(() => { cargar() }, [cargar])

  if (noEncontrada) {
    return (
      <Stack gap="md" p="xl">
        <Button variant="subtle" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/sse')} w="fit-content">
          Volver a SSE
        </Button>
        <Text c="dimmed">No se encontró la SSE.</Text>
      </Stack>
    )
  }

  if (!sse) {
    return <Text c="dimmed" p="xl">Cargando SSE…</Text>
  }

  const cliente = sse.cliente
  const empresa = sse.empresa
  const rotulos = sse.rotulos
  const analisis = sse.presupuesto?.analisis ?? []
  const rotuloActivo = rotulos.find(r => r.idRotulo === idRotuloActivo) ?? null

  function abrirModal(tipo: 'muestra' | 'corregir', idRotulo: number) {
    setIdRotuloActivo(idRotulo)
    setModal(tipo)
  }

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Button variant="subtle" color="gray" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/sse')} w="fit-content" px={0} style={{ flexShrink: 0 }}>
        SSE y rótulos
      </Button>

      <Group justify="space-between" align="flex-start" wrap="wrap" gap="md">
        <div>
          <Group gap="sm" align="center">
            <Title order={2} fw={600} ff="monospace">{sse.codigo ?? '—'}</Title>
            <Badge color={COLOR_AREA[sse.area] ?? 'gray'} variant="light" size="lg" radius="sm">{etiquetaArea(sse.area)}</Badge>
            <Badge color={COLOR_ESTADO[sse.estado] ?? 'gray'} variant="light" size="lg" radius="sm">{sse.estado}</Badge>
          </Group>
          <Text c="dimmed" size="sm" mt={4}>
            {cliente ? `${cliente.nombre} ${cliente.apellido}` : empresa ? empresa.razonSocial : 'Sin cliente ni empresa asignados'}
            {' · '}Registrada el {new Date(sse.fecha).toLocaleDateString('es-AR')}
            {' · '}por {sse.creadoPor.nombre} {sse.creadoPor.apellido}
          </Text>
        </div>
        <Group gap="sm" align="flex-start">
          {rotulos.length > 0 && (
            <Paper withBorder radius="md" px="md" py="sm">
              <Text size="xs" c="dimmed" tt="uppercase" fw={600}>
                {rotulos.length === 1 ? 'Rótulo interno' : `Rótulos internos (${rotulos.length})`}
              </Text>
              <Group gap="xs" mt={2}>
                <IconTag size={16} />
                <Group gap={4}>
                  {rotulos.map(r => (
                    <Text key={r.idRotulo} ff="monospace" fw={700} fz={rotulos.length === 1 ? 18 : 14}>{r.numeroUnico}</Text>
                  ))}
                </Group>
              </Group>
            </Paper>
          )}
          <Button
            variant="default"
            leftSection={<IconFileTypePdf size={16} />}
            onClick={descargarPdf}
            title="Descarga el PDF de esta SSE, para el laboratorio o para el cliente"
          >
            Descargar PDF
          </Button>
        </Group>
      </Group>

      <Tabs value={pestana} onChange={setPestana} keepMounted={false}>
        <Tabs.List>
          <Tabs.Tab value="servicio" leftSection={<IconClipboardText size={16} />}>Servicio</Tabs.Tab>
          <Tabs.Tab value="rotulo" leftSection={<IconTestPipe size={16} />}>
            Rótulos y muestras
            {rotulos.length > 0 && <Pill size="xs" ml={6}>{rotulos.filter(r => r.muestra).length}/{rotulos.length}</Pill>}
          </Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="servicio" pt="lg">
          <SimpleGrid cols={{ base: 1, lg: 2 }} spacing="lg">
            <Paper withBorder radius="md" p="lg">
              <Text fw={600} mb="md">Datos del servicio</Text>
              <Stack gap="sm">
                <Dato label="Presupuesto" valor={sse.codigoRpo ?? 'Sin presupuesto vinculado'} />
                <Dato label="Área" valor={etiquetaArea(sse.area)} />
                <Dato label="Forma de pago" valor={sse.formaPago ?? '—'} />
              </Stack>
              <Divider my="md" />
              <SimpleGrid cols={2} spacing="md">
                <Paper radius="md" p="md" bg="gray.0">
                  <Text size="sm" c="dimmed">Total USD</Text>
                  <Text fz={22} fw={700}>{fmtUsd(sse.importeDolares)}</Text>
                </Paper>
                <Paper radius="md" p="md" bg="gray.0">
                  <Text size="sm" c="dimmed">Total ARS</Text>
                  <Text fz={22} fw={700}>{fmtArs(sse.importePesos)}</Text>
                </Paper>
              </SimpleGrid>
            </Paper>

            <Stack gap="lg">
              <Paper withBorder radius="md" p="lg">
                <Group justify="space-between" mb="md">
                  <Group gap="xs">
                    <IconUser size={18} />
                    <Text fw={600}>Cliente / empresa</Text>
                  </Group>
                  <Button size="xs" variant="default" onClick={() => setModal('cliente')}>
                    {cliente || empresa ? 'Cambiar' : 'Asignar'}
                  </Button>
                </Group>
                {cliente || empresa ? (
                  <Stack gap="sm">
                    {cliente && <Dato label="Cliente" valor={`${cliente.nombre} ${cliente.apellido}`} />}
                    {cliente?.email && <Dato label="Email" valor={cliente.email} />}
                    {cliente?.telefono && <Dato label="Teléfono" valor={cliente.telefono} />}
                    {empresa && <Dato label="Empresa" valor={empresa.razonSocial} />}
                  </Stack>
                ) : (
                  <Text c="dimmed" size="sm">Sin cliente ni empresa asignados a esta SSE.</Text>
                )}
              </Paper>

              <Paper withBorder radius="md" style={{ overflow: 'hidden', flexShrink: 0 }}>
                <Group p="md" pb="sm">
                  <Text fw={600}>Análisis solicitados</Text>
                  <Pill size="xs">{analisis.length}</Pill>
                </Group>
                {analisis.length === 0 ? (
                  <Text c="dimmed" size="sm" p="md" pt={0}>Sin análisis vinculados.</Text>
                ) : (
                  <Table verticalSpacing="xs" horizontalSpacing="md">
                    <Table.Tbody>
                      {analisis.map((a, i) => (
                        <Table.Tr key={i}>
                          <Table.Td w={90}>
                            <Badge color={COLOR_AREA[a.area] ?? 'gray'} variant="light" radius="sm">{etiquetaArea(a.area)}</Badge>
                          </Table.Td>
                          <Table.Td>
                            <Text size="sm">{a.nombre}</Text>
                          </Table.Td>
                        </Table.Tr>
                      ))}
                    </Table.Tbody>
                  </Table>
                )}
              </Paper>
            </Stack>
          </SimpleGrid>
        </Tabs.Panel>

        <Tabs.Panel value="rotulo" pt="lg">
          {rotulos.length > 0 ? (
            <Stack gap="xl">
              {rotulos.map(rotulo => (
                <Stack key={rotulo.idRotulo} gap="lg">
                  <Paper withBorder radius="md" p="lg">
                    <Group justify="space-between" align="flex-start" mb="sm">
                      <Group gap="sm">
                        <ThemeIcon variant="light" color="teal" size="lg" radius="md"><IconTag size={20} /></ThemeIcon>
                        <div>
                          <Text ff="monospace" fw={700} fz={20}>{rotulo.numeroUnico}</Text>
                          <Text size="sm" c="dimmed">
                            Asignado por {rotulo.asignadoPor.nombre} {rotulo.asignadoPor.apellido} · {new Date(rotulo.fechaAsignacion).toLocaleDateString('es-AR')}
                          </Text>
                        </div>
                      </Group>
                      <Group gap="sm">
                        <Badge color={COLOR_ESTADO[rotulo.estado] ?? 'gray'} variant="light" radius="sm">{rotulo.estado}</Badge>
                        <Button variant="default" onClick={() => abrirModal('corregir', rotulo.idRotulo)}>Corregir rótulo</Button>
                      </Group>
                    </Group>
                    {rotulo.descripcion && <Text size="sm">{rotulo.descripcion}</Text>}
                  </Paper>

                  <Paper withBorder radius="md" p="lg">
                    <Group justify="space-between" mb="md">
                      <Group gap="xs">
                        <IconTestPipe size={18} />
                        <Text fw={600}>Muestra</Text>
                      </Group>
                      {!rotulo.muestra && <Button onClick={() => abrirModal('muestra', rotulo.idRotulo)}>Registrar muestra</Button>}
                    </Group>
                    {rotulo.muestra ? (
                      <SimpleGrid cols={{ base: 1, sm: 2 }} spacing="md">
                        <Dato label="Tipo" valor={rotulo.muestra.tipo ?? '—'} />
                        <Group justify="space-between" align="flex-start" wrap="nowrap" gap="md">
                          <Text size="sm" c="dimmed">Estado</Text>
                          <Badge color={COLOR_ESTADO[rotulo.muestra.estado] ?? 'gray'} variant="light" radius="sm">
                            {ESTADO_MUESTRA_LABEL[rotulo.muestra.estado] ?? rotulo.muestra.estado}
                          </Badge>
                        </Group>
                        <Dato label="Recibida" valor={new Date(rotulo.muestra.fechaRecepcion).toLocaleDateString('es-AR')} />
                        <Dato label="Recibida por" valor={`${rotulo.muestra.recibidoPor.nombre} ${rotulo.muestra.recibidoPor.apellido}`} />
                        {rotulo.muestra.observacion && (
                          <div style={{ gridColumn: '1 / -1' }}>
                            <Dato label="Observación" valor={rotulo.muestra.observacion} />
                          </div>
                        )}
                      </SimpleGrid>
                    ) : (
                      <Text c="dimmed" size="sm">Todavía no se registró la muestra de este rótulo.</Text>
                    )}
                  </Paper>

                  <Paper withBorder radius="md" p="lg">
                    <Group gap="xs" mb="md">
                      <IconHistory size={18} />
                      <Text fw={600}>Historial de cambios</Text>
                    </Group>
                    {rotulo.auditorias.length === 0 ? (
                      <Text c="dimmed" size="sm">El rótulo no tiene cambios registrados.</Text>
                    ) : (
                      <Stack gap="md">
                        {rotulo.auditorias.map(a => (
                          <Group key={a.idAuditoria} align="flex-start" wrap="nowrap" gap="md">
                            <Text size="xs" c="dimmed" w={90} style={{ flexShrink: 0 }}>
                              {new Date(a.fechaCambio).toLocaleDateString('es-AR')}
                            </Text>
                            <div>
                              <Text size="sm">
                                <strong>{a.usuario.nombre} {a.usuario.apellido}</strong> cambió {CAMPO_AUDITORIA_LABEL[a.campo] ?? a.campo}:{' '}
                                <s>{a.valorAnterior || '—'}</s> → <strong>{a.valorNuevo || '—'}</strong>
                              </Text>
                              {a.motivo && <Text size="sm" c="dimmed" fs="italic">“{a.motivo}”</Text>}
                            </div>
                          </Group>
                        ))}
                      </Stack>
                    )}
                  </Paper>
                  {rotulos.length > 1 && rotulo !== rotulos[rotulos.length - 1] && <Divider />}
                </Stack>
              ))}
            </Stack>
          ) : (
            <Paper withBorder radius="md" p="xl" ta="center">
              <Text c="dimmed" mb="md">Esta SSE todavía no tiene rótulo interno.</Text>
              <Button onClick={() => setModal('rotulo')}>Asignar rótulo</Button>
            </Paper>
          )}
        </Tabs.Panel>
      </Tabs>

      {modal === 'cliente' && (
        <ModalAsignarCliente
          idSse={sse.idSse}
          idClienteActual={cliente?.idCliente ?? null}
          idEmpresaActual={empresa?.idEmpresa ?? null}
          onGuardado={async () => { setModal(null); await cargar() }}
          onCerrar={() => setModal(null)}
        />
      )}
      {modal === 'rotulo' && (
        <ModalRotulo
          idSse={sse.idSse}
          onGuardado={async () => { setModal(null); await cargar() }}
          onCerrar={() => setModal(null)}
        />
      )}
      {modal === 'muestra' && idRotuloActivo !== null && (
        <ModalMuestra
          idSse={sse.idSse}
          idRotulo={idRotuloActivo}
          onGuardado={async () => { setModal(null); await cargar() }}
          onCerrar={() => setModal(null)}
        />
      )}
      {modal === 'corregir' && rotuloActivo && (
        <ModalCorregirRotulo
          idSse={sse.idSse}
          idRotulo={rotuloActivo.idRotulo}
          rotulo={rotuloActivo}
          onGuardado={async () => { setModal(null); await cargar() }}
          onCerrar={() => setModal(null)}
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
