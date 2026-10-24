import { useEffect, useState, useCallback } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { ActionIcon, Alert, Badge, Button, Divider, FileButton, Group, Paper, Pill, SimpleGrid, Stack, Table, Tabs, Text, Title } from '@mantine/core'
import { IconArrowLeft, IconDownload, IconFileSpreadsheet, IconFileTypePdf, IconInfoCircle, IconMail, IconPaperclip, IconTrash, IconUser } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'
import {
  TIPO_ASESORAMIENTO_LABEL,
  TRANSICIONES,
  datosEnvioPresupuesto,
  formatBytes,
  iconoMime,
  type Archivo,
  type PresupuestoDetalle,
} from './presupuestosComun'

const COLOR_ESTADO: Record<string, string> = {
  borrador: 'gray',
  enviado: 'blue',
  aceptado: 'teal',
  rechazado: 'red',
}

const fmtUsd = (n: number) => `USD ${n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
const fmtArs = (n: number) => `$ ${n.toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`

export default function PresupuestoDetallePage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [presupuesto, setPresupuesto] = useState<PresupuestoDetalle | null>(null)
  const [noEncontrado, setNoEncontrado] = useState(false)
  const [archivos, setArchivos] = useState<Archivo[]>([])
  const [pestana, setPestana] = useState<string | null>('analisis')
  const [cambiandoEstado, setCambiandoEstado] = useState(false)
  const [subiendo, setSubiendo] = useState(false)
  const [error, setError] = useState('')
  const [avisoEmail, setAvisoEmail] = useState(false)
  const [enviandoEmail, setEnviandoEmail] = useState(false)

  const cargarArchivos = useCallback(async () => {
    const res = await apiFetch(`/api/presupuestos/${id}/archivos`)
    if (res.ok) setArchivos(await res.json())
  }, [id])

  const cargar = useCallback(async () => {
    const res = await apiFetch(`/api/presupuestos/${id}`)
    if (!res.ok) {
      setNoEncontrado(true)
      return
    }
    setPresupuesto(await res.json())
    await cargarArchivos()
  }, [id, cargarArchivos])

  useEffect(() => { cargar() }, [cargar])

  async function cambiarEstado(nuevoEstado: string) {
    setCambiandoEstado(true)
    try {
      await apiFetch(`/api/presupuestos/${id}/estado`, { method: 'PUT', body: JSON.stringify({ estado: nuevoEstado }) })
      await cargar()
    } finally {
      setCambiandoEstado(false)
    }
  }

  async function descargarPdfPresupuesto(p: PresupuestoDetalle) {
    const res = await apiFetch(`/api/presupuestos/${id}/pdf`)
    if (!res.ok) {
      setError('No se pudo generar el PDF del presupuesto.')
      return
    }
    const blob = await res.blob()
    const url = URL.createObjectURL(blob)
    const a = document.createElement('a')
    a.href = url
    a.download = `${p.codigoRpo ?? 'presupuesto'}.pdf`
    a.click()
    URL.revokeObjectURL(url)
  }

  async function enviarPorEmail(p: PresupuestoDetalle) {
    const { destinatario, asunto, cuerpo } = datosEnvioPresupuesto(p, archivos)
    if (!destinatario) {
      setError('El cliente/empresa de esta consulta no tiene un email registrado. Cargalo en su ficha para poder enviar el presupuesto.')
      return
    }
    setEnviandoEmail(true)
    setError('')
    try {
      const res = await apiFetch(`/api/presupuestos/${id}/enviar`, {
        method: 'POST',
        body: JSON.stringify({ destinatario, asunto, cuerpo }),
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo enviar el presupuesto por email.')
        return
      }
      setAvisoEmail(true)
      await cargar()
    } finally {
      setEnviandoEmail(false)
    }
  }

  async function subirArchivo(file: File | null) {
    if (!file) return
    setSubiendo(true)
    setError('')
    try {
      const form = new FormData()
      form.append('archivo', file)
      const res = await apiFetch(`/api/presupuestos/${id}/archivos`, { method: 'POST', body: form, headers: {} })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo adjuntar el archivo.')
        return
      }
      await cargarArchivos()
    } finally {
      setSubiendo(false)
    }
  }

  async function eliminarArchivo(idArchivo: number) {
    await apiFetch(`/api/presupuestos/${id}/archivos/${idArchivo}`, { method: 'DELETE' })
    setArchivos(prev => prev.filter(a => a.id !== idArchivo))
  }

  function descargarArchivo(archivo: Archivo) {
    apiFetch(`/api/presupuestos/${id}/archivos/${archivo.id}`)
      .then(r => r.blob())
      .then(blob => {
        const url = URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = archivo.nombreOriginal
        a.click()
        URL.revokeObjectURL(url)
      })
  }

  if (noEncontrado) {
    return (
      <Stack gap="md" p="xl">
        <Button variant="subtle" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/presupuestos')} w="fit-content">
          Volver a presupuestos
        </Button>
        <Text c="dimmed">No se encontró el presupuesto.</Text>
      </Stack>
    )
  }

  if (!presupuesto) {
    return <Text c="dimmed" p="xl">Cargando presupuesto…</Text>
  }

  const transiciones = TRANSICIONES[presupuesto.estado] ?? []
  const subtotalAnalisis = presupuesto.items.reduce((s, i) => s + i.precioUsdSnapshot, 0)
    + presupuesto.paquetes.reduce((s, p) => s + p.precioUsdSnapshot, 0)
  const cliente = presupuesto.consulta.cliente
  const puedeEnviar = presupuesto.estado === 'borrador' || presupuesto.estado === 'enviado'

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Button variant="subtle" color="gray" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/presupuestos')} w="fit-content" px={0} style={{ flexShrink: 0 }}>
        Presupuestos
      </Button>

      <Group justify="space-between" align="flex-start" wrap="wrap" gap="md">
        <div>
          <Group gap="sm" align="center">
            <Title order={2} fw={600} ff="monospace">{presupuesto.codigoRpo ?? '—'}</Title>
            <Badge color={COLOR_ESTADO[presupuesto.estado] ?? 'gray'} variant="light" size="lg" radius="sm">{presupuesto.estado}</Badge>
          </Group>
          <Text c="dimmed" size="sm" mt={4}>
            {cliente ? `${cliente.nombre} ${cliente.apellido}` : presupuesto.consulta.empresa?.razonSocial ?? 'Sin cliente'}
            {' · '}Consulta {presupuesto.consulta.codigo ?? '—'}
            {' · '}{new Date(presupuesto.fecha).toLocaleDateString('es-AR')}
          </Text>
        </div>

        <Group gap="sm">
          {transiciones.map(t => (
            <Button
              key={t.siguienteEstado}
              variant={t.variante === 'primario' ? 'default' : 'light'}
              color={t.variante === 'verde' ? 'teal' : t.variante === 'rojo' ? 'red' : 'gray'}
              disabled={cambiandoEstado}
              onClick={() => cambiarEstado(t.siguienteEstado)}
            >
              {t.label}
            </Button>
          ))}
          <Button
            variant="default"
            leftSection={<IconFileTypePdf size={16} />}
            onClick={() => descargarPdfPresupuesto(presupuesto)}
            title="Descarga el PDF del presupuesto"
          >
            Descargar PDF
          </Button>
          {puedeEnviar && (
            <Button
              leftSection={<IconMail size={16} />}
              disabled={cambiandoEstado}
              loading={enviandoEmail}
              onClick={() => enviarPorEmail(presupuesto)}
              title="Manda el presupuesto por mail, con el PDF adjunto, desde la casilla del laboratorio"
            >
              Enviar por email
            </Button>
          )}
        </Group>
      </Group>

      {avisoEmail && (
        <Alert
          color="blue"
          variant="light"
          withCloseButton
          onClose={() => setAvisoEmail(false)}
          style={{ flexShrink: 0 }}
        >
          Se envió el presupuesto por email, con el PDF adjunto, desde la casilla del laboratorio.
        </Alert>
      )}

      {error && (
        <Alert color="red" variant="light" withCloseButton onClose={() => setError('')} style={{ flexShrink: 0 }}>
          {error}
        </Alert>
      )}

      <SimpleGrid cols={{ base: 1, sm: 3 }} spacing="md">
        <Paper withBorder radius="md" p="md" bg="gray.0">
          <Text size="sm" c="dimmed">Total en dólares</Text>
          <Text fz={24} fw={700}>{fmtUsd(presupuesto.importeDolares)}</Text>
        </Paper>
        <Paper withBorder radius="md" p="md" bg="gray.0">
          <Text size="sm" c="dimmed">Cotización</Text>
          <Text fz={24} fw={700}>{fmtArs(presupuesto.cotizacion)}</Text>
        </Paper>
        <Paper withBorder radius="md" p="md" bg="teal.0" style={{ borderColor: 'var(--mantine-color-teal-3)' }}>
          <Text size="sm" c="teal.8">Total en pesos</Text>
          <Text fz={24} fw={700} c="teal.9">{fmtArs(presupuesto.importePesos)}</Text>
        </Paper>
      </SimpleGrid>

      <Tabs value={pestana} onChange={setPestana} keepMounted={false}>
        <Tabs.List>
          <Tabs.Tab value="analisis" leftSection={<IconFileSpreadsheet size={16} />}>
            Análisis
            <Pill size="xs" ml={6}>{presupuesto.items.length + presupuesto.paquetes.length}</Pill>
          </Tabs.Tab>
          <Tabs.Tab value="consulta" leftSection={<IconInfoCircle size={16} />}>Consulta y cliente</Tabs.Tab>
          <Tabs.Tab value="documentos" leftSection={<IconPaperclip size={16} />}>
            Documentos
            <Pill size="xs" ml={6}>{archivos.length}</Pill>
          </Tabs.Tab>
        </Tabs.List>

        <Tabs.Panel value="analisis" pt="lg">
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
                {presupuesto.paquetes.map(p => (
                  <Table.Tr key={`paquete-${p.id}`}>
                    <Table.Td>
                      <Badge color={p.area === 'MIC' ? 'blue' : 'violet'} variant="light" radius="sm">{p.area}</Badge>
                    </Table.Td>
                    <Table.Td>
                      <Text size="sm" fw={500}>{p.nombre} <Text component="span" size="xs" c="dimmed">(paquete)</Text></Text>
                      <Text size="xs" c="dimmed">Incluye: {p.analisisIncluidos.join(', ')}</Text>
                    </Table.Td>
                    <Table.Td ta="right">
                      <Text size="sm" ff="monospace">{fmtUsd(p.precioUsdSnapshot)}</Text>
                    </Table.Td>
                  </Table.Tr>
                ))}
                {presupuesto.items.map(i => (
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
                {presupuesto.adicionalAsesoramiento > 0 && (
                  <Table.Tr>
                    <Table.Td />
                    <Table.Td>
                      <Text size="sm" fw={500}>
                        Asesoramiento{presupuesto.tipoAsesoramiento && ` · ${TIPO_ASESORAMIENTO_LABEL[presupuesto.tipoAsesoramiento] ?? presupuesto.tipoAsesoramiento}`}
                      </Text>
                    </Table.Td>
                    <Table.Td ta="right">
                      <Text size="sm" ff="monospace">{fmtUsd(presupuesto.adicionalAsesoramiento)}</Text>
                    </Table.Td>
                  </Table.Tr>
                )}
              </Table.Tbody>
            </Table>
            <Divider />
            <Group justify="space-between" p="md">
              <Text size="sm" c="dimmed">Subtotal de análisis</Text>
              <Text size="sm" ff="monospace" fw={600}>{fmtUsd(subtotalAnalisis)}</Text>
            </Group>
          </Paper>
        </Tabs.Panel>

        <Tabs.Panel value="consulta" pt="lg">
          <SimpleGrid cols={{ base: 1, lg: 2 }} spacing="lg">
            <Paper withBorder radius="md" p="lg">
              <Text fw={600} mb="md">Consulta asociada</Text>
              <Stack gap="sm">
                <Dato label="Código" valor={presupuesto.consulta.codigo ?? '—'} />
                <Dato label="Canal" valor={presupuesto.consulta.canal} />
                <Dato label="Creado por" valor={`${presupuesto.creadoPor.nombre} ${presupuesto.creadoPor.apellido}`} />
              </Stack>
              {presupuesto.consulta.descripcion && (
                <>
                  <Divider my="md" />
                  <Text size="sm" c="dimmed" mb={4}>Descripción</Text>
                  <Text size="sm">{presupuesto.consulta.descripcion}</Text>
                </>
              )}
              <Button variant="subtle" mt="md" px={0} onClick={() => navigate(`/consultas/${presupuesto.consulta.idConsulta}`)}>
                Ver la consulta →
              </Button>
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
                  {cliente.empresa && <Dato label="Empresa" valor={cliente.empresa.razonSocial} />}
                </Stack>
              ) : presupuesto.consulta.empresa ? (
                <Dato label="Empresa" valor={presupuesto.consulta.empresa.razonSocial} />
              ) : (
                <Text c="dimmed" size="sm">Sin cliente asignado a la consulta.</Text>
              )}
            </Paper>
          </SimpleGrid>
        </Tabs.Panel>

        <Tabs.Panel value="documentos" pt="lg">
          <Paper withBorder radius="md" p="lg">
            <Group justify="space-between" mb="md">
              <div>
                <Text fw={600}>Documentos adjuntos</Text>
                <Text size="sm" c="dimmed">PDF, Word, Excel, imagen o ZIP. Se envían junto con el presupuesto.</Text>
              </div>
              <FileButton onChange={subirArchivo} accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg,.zip" disabled={subiendo}>
                {props => <Button {...props} leftSection={<IconPaperclip size={16} />} loading={subiendo}>Adjuntar archivo</Button>}
              </FileButton>
            </Group>

            {archivos.length === 0 ? (
              <Text c="dimmed" size="sm" ta="center" py="lg">Todavía no hay documentos adjuntos.</Text>
            ) : (
              <Stack gap="xs">
                {archivos.map(a => (
                  <Group key={a.id} justify="space-between" wrap="nowrap" p="sm" style={{ border: '1px solid var(--mantine-color-gray-3)', borderRadius: 8 }}>
                    <Group gap="sm" wrap="nowrap" style={{ minWidth: 0 }}>
                      <Text fz={22}>{iconoMime(a.tipoMime)}</Text>
                      <div style={{ minWidth: 0 }}>
                        <Text size="sm" fw={500} truncate>{a.nombreOriginal}</Text>
                        <Text size="xs" c="dimmed">{formatBytes(a.tamañoBytes)} · {new Date(a.subidoEn).toLocaleDateString('es-AR')}</Text>
                      </div>
                    </Group>
                    <Group gap="xs" wrap="nowrap">
                      <ActionIcon variant="default" size="lg" aria-label="Descargar" title="Descargar" onClick={() => descargarArchivo(a)}>
                        <IconDownload size={16} />
                      </ActionIcon>
                      <ActionIcon variant="light" color="red" size="lg" aria-label="Eliminar" title="Eliminar" onClick={() => eliminarArchivo(a.id)}>
                        <IconTrash size={16} />
                      </ActionIcon>
                    </Group>
                  </Group>
                ))}
              </Stack>
            )}
          </Paper>
        </Tabs.Panel>
      </Tabs>
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
