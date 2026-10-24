import { useEffect, useState, useCallback } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Alert, Badge, Button, Group, Modal, Paper, Progress, SimpleGrid, Stack, Table, Text, TextInput, Textarea, Title } from '@mantine/core'
import { IconAlertTriangle, IconArrowLeft, IconCheck, IconDownload, IconFileText, IconX } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'

interface ResultadoDetalle {
  idResultado: number
  valor: string | null
  estado: 'pendiente' | 'cargado' | 'validado' | 'rechazado'
  fechaCarga: string | null
  motivoRechazo: string | null
  cargadoPor: { nombre: string; apellido: string } | null
  validadoPor: { nombre: string; apellido: string } | null
  fechaValidacion: string | null
  analisis: {
    idAnalisis: number
    codigo: string
    nombre: string
    area: string
    unidad: string | null
    rangoMin: number | null
    rangoMax: number | null
    valorEsperado: string | null
    grupoNombre: string
  }
  fueraDeRango: boolean | null
}

interface MuestraDetalle {
  idMuestra: number
  tipo: string | null
  estado: string
  fechaRecepcion: string
  numeroRotulo: string
  codigoSse: string | null
  cliente: { nombre: string; apellido: string } | null
  empresa: { razonSocial: string } | null
  informe: { idInforme: number; codigo: string; fechaGeneracion: string } | null
  todoValidado: boolean
  resultados: ResultadoDetalle[]
}

const ESTADO_RESULTADO: Record<string, { label: string; color: string }> = {
  pendiente: { label: 'Pendiente', color: 'gray' },
  cargado: { label: 'Por validar', color: 'orange' },
  validado: { label: 'Validado', color: 'teal' },
  rechazado: { label: 'Rechazado', color: 'red' },
}

function referenciaTexto(a: ResultadoDetalle['analisis']) {
  if (a.valorEsperado) return `Esperado: ${a.valorEsperado}`
  if (a.rangoMin != null || a.rangoMax != null) {
    return `Rango: ${a.rangoMin ?? '—'} a ${a.rangoMax ?? '—'} ${a.unidad ?? ''}`.trim()
  }
  return null
}

export default function ResultadoDetallePage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const [muestra, setMuestra] = useState<MuestraDetalle | null>(null)
  const [noEncontrada, setNoEncontrada] = useState(false)
  const [guardandoInforme, setGuardandoInforme] = useState(false)
  const [error, setError] = useState('')
  const [cargarPara, setCargarPara] = useState<ResultadoDetalle | null>(null)
  const [rechazarPara, setRechazarPara] = useState<ResultadoDetalle | null>(null)

  const cargar = useCallback(async () => {
    const res = await apiFetch(`/api/resultados/muestras/${id}`)
    if (!res.ok) {
      setNoEncontrada(true)
      return
    }
    setMuestra(await res.json())
  }, [id])

  useEffect(() => { cargar() }, [cargar])

  async function guardarInforme() {
    if (!muestra) return
    setGuardandoInforme(true)
    setError('')
    try {
      const res = await apiFetch(`/api/resultados/muestras/${muestra.idMuestra}/informe`, { method: 'POST' })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo guardar el informe.')
        return
      }
      await cargar()
    } finally {
      setGuardandoInforme(false)
    }
  }

  function descargarPdf(idInforme: number, codigo: string) {
    apiFetch(`/api/resultados/informes/${idInforme}/pdf`)
      .then(r => r.blob())
      .then(blob => {
        const url = URL.createObjectURL(blob)
        const a = document.createElement('a')
        a.href = url
        a.download = `${codigo}.pdf`
        a.click()
        URL.revokeObjectURL(url)
      })
  }

  if (noEncontrada) {
    return (
      <Stack gap="md" p="xl">
        <Button variant="subtle" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/resultados')} w="fit-content">
          Volver a resultados
        </Button>
        <Text c="dimmed">No se encontró la muestra.</Text>
      </Stack>
    )
  }

  if (!muestra) {
    return <Text c="dimmed" p="xl">Cargando muestra…</Text>
  }

  const validados = muestra.resultados.filter(r => r.estado === 'validado').length
  const total = muestra.resultados.length
  const pendientesDeValidar = muestra.resultados.filter(r => r.estado !== 'validado').length
  const progreso = total > 0 ? (validados / total) * 100 : 0

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Button variant="subtle" color="gray" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/resultados')} w="fit-content" px={0} style={{ flexShrink: 0 }}>
        Resultados
      </Button>

      <Group justify="space-between" align="flex-start" wrap="wrap" gap="md">
        <div>
          <Title order={2} fw={600} ff="monospace">{muestra.numeroRotulo}</Title>
          <Text c="dimmed" size="sm" mt={4}>
            {muestra.cliente ? `${muestra.cliente.nombre} ${muestra.cliente.apellido}` : muestra.empresa?.razonSocial ?? 'Sin cliente'}
            {' · '}SSE {muestra.codigoSse ?? '—'}
            {muestra.tipo && ` · ${muestra.tipo}`}
            {' · '}Recibida el {new Date(muestra.fechaRecepcion).toLocaleDateString('es-AR')}
          </Text>
        </div>
      </Group>

      <SimpleGrid cols={{ base: 1, md: 2 }} spacing="lg">
        <Paper withBorder radius="md" p="lg">
          <Group justify="space-between" mb="sm">
            <Text fw={600}>Avance de validación</Text>
            <Text size="sm" fw={600}>{validados} de {total}</Text>
          </Group>
          <Progress value={progreso} color={progreso === 100 ? 'teal' : 'orange'} size="lg" radius="xl" />
          <Text size="sm" c="dimmed" mt="sm">
            {pendientesDeValidar > 0
              ? `Faltan ${pendientesDeValidar} análisis por cargar o validar.`
              : 'Todos los análisis están validados.'}
          </Text>
        </Paper>

        <Paper withBorder radius="md" p="lg">
          <Group gap="xs" mb="sm">
            <IconFileText size={18} />
            <Text fw={600}>Informe de resultados</Text>
          </Group>
          {muestra.informe ? (
            <Group justify="space-between" align="center">
              <div>
                <Text ff="monospace" fw={600}>{muestra.informe.codigo}</Text>
                <Text size="sm" c="dimmed">Guardado el {new Date(muestra.informe.fechaGeneracion).toLocaleDateString('es-AR')}</Text>
              </div>
              <Button leftSection={<IconDownload size={16} />} onClick={() => descargarPdf(muestra.informe!.idInforme, muestra.informe!.codigo)}>
                Descargar PDF
              </Button>
            </Group>
          ) : muestra.todoValidado ? (
            <Group justify="space-between" align="center">
              <Text size="sm" c="dimmed">Todos los análisis están validados. Ya se puede guardar el informe.</Text>
              <Button onClick={guardarInforme} loading={guardandoInforme}>Guardar informe</Button>
            </Group>
          ) : (
            <Text size="sm" c="dimmed">El informe se guarda cuando todos los análisis estén validados.</Text>
          )}
          {error && <Alert color="red" variant="light" mt="sm" style={{ flexShrink: 0 }}>{error}</Alert>}
        </Paper>
      </SimpleGrid>

      <Paper withBorder radius="md" style={{ overflowX: 'auto', flexShrink: 0 }}>
        <Table verticalSpacing="md" horizontalSpacing="md" styles={{ th: { fontSize: 13, fontWeight: 600 } }}>
          <Table.Thead style={{ background: 'var(--mantine-color-gray-0)' }}>
            <Table.Tr>
              <Table.Th>Análisis</Table.Th>
              <Table.Th>Valor</Table.Th>
              <Table.Th>Estado</Table.Th>
              <Table.Th ta="right">Acciones</Table.Th>
            </Table.Tr>
          </Table.Thead>
          <Table.Tbody>
            {muestra.resultados.map(r => (
              <FilaResultado
                key={r.idResultado}
                resultado={r}
                onCargar={() => setCargarPara(r)}
                onValidar={async () => {
                  setError('')
                  const res = await apiFetch(`/api/resultados/${r.idResultado}/validar`, {
                    method: 'PUT',
                    body: JSON.stringify({ aprobado: true, motivoRechazo: null }),
                  })
                  if (!res.ok) {
                    const d = await res.json().catch(() => ({}))
                    setError(d.error ?? 'No se pudo validar.')
                    return
                  }
                  await cargar()
                }}
                onRechazar={() => setRechazarPara(r)}
              />
            ))}
          </Table.Tbody>
        </Table>
      </Paper>

      {cargarPara && (
        <ModalCargarValor
          resultado={cargarPara}
          onCerrar={() => setCargarPara(null)}
          onGuardado={async () => { setCargarPara(null); await cargar() }}
        />
      )}

      {rechazarPara && (
        <ModalRechazar
          resultado={rechazarPara}
          onCerrar={() => setRechazarPara(null)}
          onRechazado={async () => { setRechazarPara(null); await cargar() }}
        />
      )}
    </Stack>
  )
}

function FilaResultado({ resultado, onCargar, onValidar, onRechazar }: {
  resultado: ResultadoDetalle
  onCargar: () => void
  onValidar: () => void
  onRechazar: () => void
}) {
  const estado = ESTADO_RESULTADO[resultado.estado]
  const referencia = referenciaTexto(resultado.analisis)
  const puedeCargar = resultado.estado === 'pendiente' || resultado.estado === 'rechazado'

  return (
    <Table.Tr>
      <Table.Td>
        <Group gap="xs" wrap="nowrap" mb={2}>
          <Badge color={resultado.analisis.area === 'MIC' ? 'blue' : 'violet'} variant="light" radius="sm">{resultado.analisis.area}</Badge>
          <Text size="sm" fw={600}>{resultado.analisis.nombre}</Text>
        </Group>
        {referencia && <Text size="xs" c="dimmed">{referencia}</Text>}
      </Table.Td>

      <Table.Td>
        {resultado.valor ? (
          <>
            <Text size="sm" fw={600} ff="monospace">
              {resultado.valor} {resultado.analisis.unidad ?? ''}
            </Text>
            {resultado.fueraDeRango && (
              <Group gap={4} mt={2}>
                <IconAlertTriangle size={14} color="var(--mantine-color-orange-7)" />
                <Text size="xs" c="orange.8" fw={600}>Fuera del rango de referencia</Text>
              </Group>
            )}
          </>
        ) : (
          <Text size="sm" c="dimmed">Sin cargar</Text>
        )}
        {resultado.estado === 'rechazado' && resultado.motivoRechazo && (
          <Text size="xs" c="red.7" mt={4}>Motivo: {resultado.motivoRechazo}</Text>
        )}
        {resultado.valor && (
          <Text size="xs" c="dimmed" mt={2}>
            {resultado.estado === 'validado' && resultado.validadoPor
              ? `Validado por ${resultado.validadoPor.nombre} ${resultado.validadoPor.apellido}`
              : resultado.cargadoPor
                ? `Cargado por ${resultado.cargadoPor.nombre} ${resultado.cargadoPor.apellido}`
                : null}
          </Text>
        )}
      </Table.Td>

      <Table.Td>
        <Badge color={estado.color} variant="light" radius="sm">{estado.label}</Badge>
      </Table.Td>

      <Table.Td ta="right">
        <Group gap="xs" justify="flex-end" wrap="nowrap">
          {puedeCargar && (
            <Button size="xs" variant="default" onClick={onCargar}>
              {resultado.estado === 'rechazado' ? 'Corregir' : 'Cargar valor'}
            </Button>
          )}
          {resultado.estado === 'cargado' && (
            <>
              <Button size="xs" variant="default" onClick={onCargar}>Editar</Button>
              <Button size="xs" variant="light" color="red" leftSection={<IconX size={14} />} onClick={onRechazar}>Rechazar</Button>
              <Button size="xs" color="teal" leftSection={<IconCheck size={14} />} onClick={onValidar}>Validar</Button>
            </>
          )}
        </Group>
      </Table.Td>
    </Table.Tr>
  )
}

function ModalCargarValor({ resultado, onCerrar, onGuardado }: {
  resultado: ResultadoDetalle
  onCerrar: () => void
  onGuardado: () => void
}) {
  const [valor, setValor] = useState(resultado.valor ?? '')
  const [error, setError] = useState('')
  const [guardando, setGuardando] = useState(false)

  async function guardar() {
    if (!valor.trim()) return setError('Ingresá un valor.')
    setGuardando(true)
    setError('')
    try {
      const res = await apiFetch(`/api/resultados/${resultado.idResultado}`, {
        method: 'PUT',
        body: JSON.stringify({ valor: valor.trim() }),
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo guardar el valor.')
        return
      }
      onGuardado()
    } finally {
      setGuardando(false)
    }
  }

  return (
    <Modal opened onClose={onCerrar} title={resultado.analisis.nombre} centered>
      <Stack gap="md">
        {referenciaTexto(resultado.analisis) && (
          <Text size="sm" c="dimmed">{referenciaTexto(resultado.analisis)}</Text>
        )}
        <TextInput
          label="Valor obtenido"
          placeholder={resultado.analisis.unidad ? `Valor en ${resultado.analisis.unidad}` : 'Valor'}
          value={valor}
          onChange={e => setValor(e.currentTarget.value)}
          onKeyDown={e => { if (e.key === 'Enter') guardar() }}
          data-autofocus
          error={error || undefined}
        />
        <Group justify="flex-end">
          <Button variant="default" onClick={onCerrar} disabled={guardando}>Cancelar</Button>
          <Button onClick={guardar} loading={guardando}>Guardar valor</Button>
        </Group>
      </Stack>
    </Modal>
  )
}

function ModalRechazar({ resultado, onCerrar, onRechazado }: {
  resultado: ResultadoDetalle
  onCerrar: () => void
  onRechazado: () => void
}) {
  const [motivo, setMotivo] = useState('')
  const [error, setError] = useState('')
  const [enviando, setEnviando] = useState(false)

  async function confirmar() {
    if (!motivo.trim()) return setError('Indicá el motivo del rechazo.')
    setEnviando(true)
    setError('')
    try {
      const res = await apiFetch(`/api/resultados/${resultado.idResultado}/validar`, {
        method: 'PUT',
        body: JSON.stringify({ aprobado: false, motivoRechazo: motivo.trim() }),
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo rechazar el resultado.')
        return
      }
      onRechazado()
    } finally {
      setEnviando(false)
    }
  }

  return (
    <Modal opened onClose={onCerrar} title={`Rechazar: ${resultado.analisis.nombre}`} centered>
      <Stack gap="md">
        <Text size="sm" c="dimmed">El analista verá el motivo y deberá corregir el valor.</Text>
        <Textarea
          label="Motivo del rechazo"
          value={motivo}
          onChange={e => setMotivo(e.currentTarget.value)}
          autosize
          minRows={3}
          data-autofocus
          error={error || undefined}
        />
        <Group justify="flex-end">
          <Button variant="default" onClick={onCerrar} disabled={enviando}>Cancelar</Button>
          <Button color="red" onClick={confirmar} loading={enviando}>Confirmar rechazo</Button>
        </Group>
      </Stack>
    </Modal>
  )
}
