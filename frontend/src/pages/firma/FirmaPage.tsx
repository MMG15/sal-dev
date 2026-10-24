import { useEffect, useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Alert, Badge, Group, Pagination, Paper, Select, Stack, Table, Text, Title } from '@mantine/core'
import { IconAlertCircle } from '@tabler/icons-react'
import { apiFetch } from '../../lib/api'
import { etiquetaArea } from '../../lib/areas'
import { useSesion } from '../../context/SesionContext'
import { ESTADO_FIRMA_COLOR, ESTADO_FIRMA_LABEL, TIPO_LABEL, fechaCorta, type DocumentoLista } from './firmaComun'

const POR_PAGINA = 20

export default function FirmaPage() {
  const navigate = useNavigate()
  const { sesion } = useSesion()
  const [documentos, setDocumentos] = useState<DocumentoLista[]>([])
  const [cargando, setCargando] = useState(true)
  const [error, setError] = useState('')
  const [filtroTipo, setFiltroTipo] = useState<string | null>(null)
  const [filtroEstado, setFiltroEstado] = useState<string | null>(null)
  const [filtroArea, setFiltroArea] = useState<string | null>(null)
  const [pagina, setPagina] = useState(1)

  const cargar = useCallback(async () => {
    setCargando(true)
    setError('')
    try {
      const params = new URLSearchParams()
      if (filtroTipo) params.set('tipo', filtroTipo)
      if (filtroEstado) params.set('estado', filtroEstado)
      if (filtroArea) params.set('area', filtroArea)
      const res = await apiFetch(`/api/firmas/documentos?${params}`)
      const data = await res.json().catch(() => null)
      if (!res.ok || !Array.isArray(data)) {
        setDocumentos([])
        setError(`No se pudieron cargar los documentos (error ${res.status}).`)
        return
      }
      setDocumentos(data)
    } finally {
      setCargando(false)
    }
  }, [filtroTipo, filtroEstado, filtroArea])

  useEffect(() => { cargar() }, [cargar])

  // Documentos que esperan una acción de este usuario
  const areaFirmante = sesion.rolCodigo === 'ROL-08' ? 'MIC' : sesion.rolCodigo === 'ROL-09' ? 'FQ' : null
  const paraMi = documentos.filter(d =>
    (sesion.rolCodigo === 'ROL-01' && d.estado === 'firmado')
    || (areaFirmante && d.estado === 'pendiente_firma' && d.areas.includes(areaFirmante) && !d.areasFirmadas.includes(areaFirmante))
  ).length

  const visibles = documentos.slice((pagina - 1) * POR_PAGINA, pagina * POR_PAGINA)

  return (
    <Stack gap="lg" h="100%" style={{ overflowY: 'auto' }}>
      <Group justify="space-between" align="flex-end" wrap="nowrap">
        <div>
          <Title order={2} fw={600}>Firma digital</Title>
          <Text c="dimmed" size="sm">
            {sesion.rolCodigo === 'ROL-01'
              ? 'Documentos firmados por las áreas que esperan tu aprobación final.'
              : 'Documentos de tu área que esperan tu firma.'}
          </Text>
        </div>
        {paraMi > 0 && (
          <Badge size="lg" color="orange" variant="light">{paraMi} para vos</Badge>
        )}
      </Group>

      <Group gap="sm" wrap="nowrap">
        <Select
          placeholder="Todos los tipos"
          clearable
          w={200}
          value={filtroTipo}
          onChange={v => { setFiltroTipo(v); setPagina(1) }}
          data={[{ value: 'informe', label: 'Informes' }, { value: 'presupuesto', label: 'Presupuestos' }]}
        />
        <Select
          placeholder="Todos los estados"
          clearable
          w={260}
          value={filtroEstado}
          onChange={v => { setFiltroEstado(v); setPagina(1) }}
          data={[
            { value: 'pendiente_firma', label: 'Pendientes de firma' },
            { value: 'firmado', label: 'Pendientes de aprobación' },
            { value: 'rechazado', label: 'Rechazados' },
            { value: 'aprobado', label: 'Aprobados' },
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

      {error && <Alert color="red" variant="light" icon={<IconAlertCircle size={18} />} style={{ flexShrink: 0 }}>{error}</Alert>}

      <Paper withBorder radius="md" style={{ overflowX: 'auto', flexShrink: 0 }}>
        {cargando ? (
          <Text c="dimmed" p="xl">Cargando documentos…</Text>
        ) : visibles.length === 0 ? (
          <Text c="dimmed" p="xl" ta="center">No hay documentos con esos filtros.</Text>
        ) : (
          <Table highlightOnHover verticalSpacing="md" horizontalSpacing="sm" styles={{ th: { fontSize: 13, fontWeight: 600 } }}>
            <Table.Thead style={{ background: 'var(--mantine-color-gray-0)' }}>
              <Table.Tr>
                <Table.Th>Documento</Table.Th>
                <Table.Th>Destinatario</Table.Th>
                <Table.Th>Firmas por área</Table.Th>
                <Table.Th>Fecha</Table.Th>
                <Table.Th>Estado</Table.Th>
              </Table.Tr>
            </Table.Thead>
            <Table.Tbody>
              {visibles.map(d => (
                <Table.Tr
                  key={`${d.tipo}-${d.id}`}
                  onClick={() => navigate(`/firma/${d.tipo}/${d.id}`)}
                  style={{ cursor: 'pointer' }}
                >
                  <Table.Td>
                    <Text ff="monospace" fw={600} size="sm" style={{ whiteSpace: 'nowrap' }}>{d.codigo}</Text>
                    <Text size="xs" c="dimmed">{TIPO_LABEL[d.tipo]}{d.rotulo ? ` · ${d.rotulo}` : ''}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Text fw={600} size="sm">{d.destinatario}</Text>
                    {d.sse && <Text size="xs" c="dimmed" ff="monospace">{d.sse}</Text>}
                  </Table.Td>
                  <Table.Td>
                    <Group gap={6} wrap="nowrap">
                      {d.areas.map(a => {
                        const firmada = d.areasFirmadas.includes(a)
                        return (
                          <Badge
                            key={a}
                            color={firmada ? 'teal' : 'gray'}
                            variant={firmada ? 'filled' : 'outline'}
                            radius="sm"
                          >
                            {firmada ? '✓ ' : ''}{etiquetaArea(a)}
                          </Badge>
                        )
                      })}
                    </Group>
                  </Table.Td>
                  <Table.Td>
                    <Text size="sm">{fechaCorta(d.fecha)}</Text>
                  </Table.Td>
                  <Table.Td>
                    <Badge color={ESTADO_FIRMA_COLOR[d.estado] ?? 'gray'} variant="light" radius="sm">
                      {ESTADO_FIRMA_LABEL[d.estado] ?? d.estado}
                    </Badge>
                  </Table.Td>
                </Table.Tr>
              ))}
            </Table.Tbody>
          </Table>
        )}
      </Paper>

      {documentos.length > POR_PAGINA && (
        <Group justify="center">
          <Pagination total={Math.ceil(documentos.length / POR_PAGINA)} value={pagina} onChange={setPagina} />
        </Group>
      )}
    </Stack>
  )
}
