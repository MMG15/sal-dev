import { useEffect, useState, useCallback } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { Alert, Badge, Button, Group, Modal, Paper, PasswordInput, SimpleGrid, Stack, Stepper, Text, Textarea, Timeline, Title } from '@mantine/core'
import { IconArrowLeft, IconCheck, IconDownload, IconMail, IconSignature, IconX } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'
import { etiquetaArea } from '../../lib/areas'
import { useSesion } from '../../context/SesionContext'
import {
  type DetalleDocumento,
  ESTADO_FIRMA_COLOR,
  ESTADO_FIRMA_LABEL,
  TIPO_EVENTO,
  TIPO_LABEL,
  datosEnvioInforme,
  fechaCorta,
  fechaHora,
} from './firmaComun'

const AREA_POR_ROL: Record<string, string> = { 'ROL-08': 'MIC', 'ROL-09': 'FQ' }

export default function FirmaDetallePage() {
  const { tipo, id } = useParams()
  const navigate = useNavigate()
  const { sesion } = useSesion()
  const [detalle, setDetalle] = useState<DetalleDocumento | null>(null)
  const [noEncontrado, setNoEncontrado] = useState(false)
  const [password, setPassword] = useState('')
  const [motivo, setMotivo] = useState('')
  const [modalRechazo, setModalRechazo] = useState(false)
  const [enviando, setEnviando] = useState(false)
  const [enviandoEmail, setEnviandoEmail] = useState(false)
  const [avisoEmail, setAvisoEmail] = useState(false)
  const [error, setError] = useState('')

  const base = `/api/firmas/documentos/${tipo}/${id}`

  const cargar = useCallback(async () => {
    const res = await apiFetch(base)
    if (!res.ok) {
      setNoEncontrado(true)
      return
    }
    setDetalle(await res.json())
  }, [base])

  useEffect(() => { cargar() }, [cargar])

  async function accion(ruta: 'firmar' | 'aprobar' | 'rechazar' | 'reenviar', cuerpo?: object) {
    setEnviando(true)
    setError('')
    try {
      const res = await apiFetch(`${base}/${ruta}`, {
        method: 'POST',
        body: cuerpo ? JSON.stringify(cuerpo) : undefined,
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo completar la acción.')
        return false
      }
      setPassword('')
      setMotivo('')
      await cargar()
      return true
    } finally {
      setEnviando(false)
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

  async function enviarInformePorEmail(doc: DetalleDocumento['documento']) {
    const { destinatario, asunto, cuerpo } = datosEnvioInforme(doc)
    if (!destinatario) {
      setError('El cliente/empresa de esta SSE no tiene un email registrado. Cargalo en su ficha para poder enviar el informe.')
      return
    }
    setEnviandoEmail(true)
    setError('')
    try {
      const res = await apiFetch(`/api/firmas/documentos/informe/${doc.id}/enviar`, {
        method: 'POST',
        body: JSON.stringify({ destinatario, asunto, cuerpo }),
      })
      if (!res.ok) {
        const d = await res.json().catch(() => ({}))
        setError(d.error ?? 'No se pudo enviar el informe por email.')
        return
      }
      setAvisoEmail(true)
    } finally {
      setEnviandoEmail(false)
    }
  }

  if (noEncontrado) {
    return (
      <Stack gap="md" p="xl">
        <Button variant="subtle" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/firma')} w="fit-content">
          Volver a firma digital
        </Button>
        <Text c="dimmed">No se encontró el documento.</Text>
      </Stack>
    )
  }

  if (!detalle) {
    return <Text c="dimmed" p="xl">Cargando documento…</Text>
  }

  const doc = detalle.documento
  const areaFirmante = AREA_POR_ROL[sesion.rolCodigo] ?? null
  const hayAcciones = detalle.permisos.firmar || detalle.permisos.aprobar || detalle.permisos.rechazar || detalle.permisos.reenviar

  // Paso activo: una etapa por área y, al final, la aprobación del administrador
  const pasos = doc.areas.length + 1
  const firmasHechas = doc.areas.filter(a => detalle.areasFirmadas.includes(a)).length
  const activo = detalle.estado === 'aprobado' ? pasos
    : detalle.estado === 'firmado' ? pasos - 1
    : firmasHechas

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Button variant="subtle" color="gray" leftSection={<IconArrowLeft size={16} />} onClick={() => navigate('/firma')} w="fit-content" px={0} style={{ flexShrink: 0 }}>
        Firma digital
      </Button>

      <Group justify="space-between" align="flex-start" wrap="wrap" gap="md">
        <div>
          <Group gap="sm" align="center">
            <Title order={2} fw={600} ff="monospace">{doc.codigo}</Title>
            <Badge color={ESTADO_FIRMA_COLOR[detalle.estado] ?? 'gray'} variant="light" size="lg" radius="sm">
              {ESTADO_FIRMA_LABEL[detalle.estado] ?? detalle.estado}
            </Badge>
          </Group>
          <Text c="dimmed" size="sm" mt={4}>
            {TIPO_LABEL[doc.tipo]} · {doc.destinatario}
            {doc.sse && ` · SSE ${doc.sse}`}
            {doc.rotulo && ` · Rótulo ${doc.rotulo}`}
            {' · '}{fechaCorta(doc.fecha)}
          </Text>
        </div>
        {doc.tipo === 'informe' && (
          <Group gap="sm">
            <Button variant="default" leftSection={<IconDownload size={16} />} onClick={() => descargarPdf(doc.id, doc.codigo)}>
              Descargar PDF{detalle.estado !== 'aprobado' ? ' (borrador)' : ''}
            </Button>
            {detalle.estado === 'aprobado' && (
              <Button
                leftSection={<IconMail size={16} />}
                loading={enviandoEmail}
                onClick={() => enviarInformePorEmail(doc)}
                title="Manda el informe firmado por mail, con el PDF adjunto, al cliente o empresa de la SSE"
              >
                Enviar al cliente por email
              </Button>
            )}
          </Group>
        )}
      </Group>

      {avisoEmail && (
        <Alert color="blue" variant="light" withCloseButton onClose={() => setAvisoEmail(false)} style={{ flexShrink: 0 }}>
          Informe enviado por email.
        </Alert>
      )}

      <Paper withBorder radius="md" p="lg">
        <Text fw={600} mb="md">Recorrido de firmas</Text>
        <Stepper active={activo} allowNextStepsSelect={false} size="sm" color="teal">
          {doc.areas.map(a => (
            <Stepper.Step
              key={a}
              label={`Responsable ${etiquetaArea(a)}`}
              description={detalle.areasFirmadas.includes(a) ? 'Firmado' : 'Pendiente'}
            />
          ))}
          <Stepper.Step
            label="Aprobación final"
            description={detalle.estado === 'aprobado' ? 'Aprobado' : 'Administrador general'}
          />
        </Stepper>
      </Paper>

      {error && <Alert color="red" variant="light" style={{ flexShrink: 0 }}>{error}</Alert>}

      {hayAcciones ? (
        <SimpleGrid cols={{ base: 1, md: 2 }} spacing="lg">
          {detalle.permisos.firmar && (
            <Paper withBorder radius="md" p="lg">
              <Group gap="xs" mb={4}>
                <IconSignature size={18} />
                <Text fw={600}>Firmar como responsable{areaFirmante ? ` de ${areaFirmante}` : ''}</Text>
              </Group>
              <Text size="sm" c="dimmed" mb="md">
                Confirmá con tu contraseña. Queda registrado quién firmó y cuándo.
              </Text>
              <Stack gap="sm">
                <PasswordInput label="Tu contraseña" value={password} onChange={e => setPassword(e.currentTarget.value)} />
                <Button leftSection={<IconSignature size={16} />} disabled={!password} loading={enviando} onClick={() => accion('firmar', { password })}>
                  Firmar documento
                </Button>
              </Stack>
            </Paper>
          )}

          {detalle.permisos.aprobar && (
            <Paper withBorder radius="md" p="lg">
              <Group gap="xs" mb={4}>
                <IconCheck size={18} />
                <Text fw={600}>Aprobación final</Text>
              </Group>
              <Text size="sm" c="dimmed" mb="md">
                Las áreas ya firmaron. Aprobá con tu contraseña para habilitar la entrega.
              </Text>
              <Stack gap="sm">
                <PasswordInput label="Tu contraseña" value={password} onChange={e => setPassword(e.currentTarget.value)} />
                <Button color="teal" leftSection={<IconCheck size={16} />} disabled={!password} loading={enviando} onClick={() => accion('aprobar', { password })}>
                  Aprobar documento
                </Button>
              </Stack>
            </Paper>
          )}

          {detalle.permisos.rechazar && (
            <Paper withBorder radius="md" p="lg">
              <Group gap="xs" mb={4}>
                <IconX size={18} />
                <Text fw={600}>Rechazar</Text>
              </Group>
              <Text size="sm" c="dimmed" mb="md">
                El documento vuelve a su autor con el motivo indicado. Las firmas previas se invalidan.
              </Text>
              <Button variant="light" color="red" leftSection={<IconX size={16} />} onClick={() => setModalRechazo(true)}>
                Rechazar documento
              </Button>
            </Paper>
          )}

          {detalle.permisos.reenviar && (
            <Paper withBorder radius="md" p="lg">
              <Group gap="xs" mb={4}>
                <IconSignature size={18} />
                <Text fw={600}>Reenviar para firma</Text>
              </Group>
              <Text size="sm" c="dimmed" mb="md">
                Cuando lo hayas corregido, reenviá el documento para iniciar un nuevo ciclo de firma.
              </Text>
              <Button loading={enviando} onClick={() => accion('reenviar')}>Reenviar</Button>
            </Paper>
          )}
        </SimpleGrid>
      ) : (
        <Text size="sm" c="dimmed">No hay acciones pendientes para vos en este documento.</Text>
      )}

      <Paper withBorder radius="md" p="lg">
        <Text fw={600} mb="md">Historial</Text>
        {detalle.historial.length === 0 ? (
          <Text size="sm" c="dimmed">Sin movimientos todavía.</Text>
        ) : (
          <Timeline active={detalle.historial.length - 1} bulletSize={22} lineWidth={2} color="teal">
            {detalle.historial.map(e => (
              <Timeline.Item key={e.idFirma} title={TIPO_EVENTO[e.tipoFirma] ?? e.tipoFirma}>
                <Text size="sm">
                  {e.area ? `${etiquetaArea(e.area)} · ` : ''}{e.firmante}
                </Text>
                <Text size="xs" c="dimmed">{fechaHora(e.fechaFirma)}</Text>
                {e.motivoRechazo && (
                  <Text size="sm" c="red.7" mt={4}>Motivo: {e.motivoRechazo}</Text>
                )}
              </Timeline.Item>
            ))}
          </Timeline>
        )}
      </Paper>

      <Modal opened={modalRechazo} onClose={() => setModalRechazo(false)} title="Rechazar documento" centered>
        <Stack gap="md">
          <Textarea
            label="Motivo del rechazo"
            value={motivo}
            onChange={e => setMotivo(e.currentTarget.value)}
            autosize
            minRows={3}
            data-autofocus
          />
          <Group justify="flex-end">
            <Button variant="default" onClick={() => setModalRechazo(false)} disabled={enviando}>Cancelar</Button>
            <Button
              color="red"
              disabled={!motivo.trim()}
              loading={enviando}
              onClick={async () => {
                const ok = await accion('rechazar', { motivo })
                if (ok) setModalRechazo(false)
              }}
            >
              Confirmar rechazo
            </Button>
          </Group>
        </Stack>
      </Modal>
    </Stack>
  )
}
