import { useEffect, useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { Badge, Box, Button, Group, SimpleGrid, Stack, Text, ThemeIcon, Title, Card } from '@mantine/core'
import { IconAlertCircle, IconArrowRight, IconFileCheck, IconReceipt, IconMicroscope, IconMessages } from '@tabler/icons-react'
import { apiFetch } from '../lib/api'
import { useSesion } from '../context/SesionContext'
import { modulosParaRol } from '../config/modulos'
import { ICONO_MODULO } from '../config/navegacion'

interface Pendiente {
  id: string
  moduloId: string
  titulo: string
  descripcion: string
  cantidad: number | null
  icono: ReactNode
}

// Áreas a las que pertenece cada responsable para firmar (ROL-08 MIC, ROL-09 FQ)
const AREA_POR_ROL: Record<string, string> = { 'ROL-08': 'MIC', 'ROL-09': 'FQ' }

function formatearFecha(fecha: Date) {
  const texto = fecha.toLocaleDateString('es-AR', { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })
  return texto.charAt(0).toUpperCase() + texto.slice(1)
}

async function contar(url: string): Promise<number | null> {
  try {
    const res = await apiFetch(url)
    if (!res.ok) return null
    const data = await res.json()
    return typeof data.total === 'number' ? data.total : null
  } catch {
    return null
  }
}

export default function InicioPage() {
  const { sesion } = useSesion()
  const modulos = modulosParaRol(sesion.rolCodigo)
  const idsModulos = modulos.map(m => m.id)
  const [pendientes, setPendientes] = useState<Pendiente[] | null>(null)

  useEffect(() => {
    let cancelado = false

    async function cargar() {
      const tareas: Promise<Pendiente | null>[] = []

      if (idsModulos.includes('consultas')) {
        tareas.push(contar('/api/consultas?estado=pendiente&porPagina=1').then(n => ({
          id: 'consultas', moduloId: 'consultas', titulo: 'Consultas sin responder',
          descripcion: 'Consultas recibidas hace más de dos días', cantidad: n,
          icono: <IconMessages size={20} />,
        })))
      }

      if (idsModulos.includes('resultados')) {
        tareas.push(contar('/api/resultados/muestras?estado=por_validar&porPagina=1').then(n => ({
          id: 'resultados', moduloId: 'resultados', titulo: 'Muestras por validar',
          descripcion: 'Con resultados cargados que esperan validación', cantidad: n,
          icono: <IconMicroscope size={20} />,
        })))
      }

      if (idsModulos.includes('facturacion')) {
        tareas.push(contar('/api/facturas?estado=emitida&porPagina=1').then(n => ({
          id: 'facturacion', moduloId: 'facturacion', titulo: 'Facturas adeudadas',
          descripcion: 'Facturas emitidas que todavía no tienen cobro registrado', cantidad: n,
          icono: <IconReceipt size={20} />,
        })))
      }

      if (idsModulos.includes('firma')) {
        tareas.push((async (): Promise<Pendiente | null> => {
          try {
            const res = await apiFetch('/api/firmas/documentos')
            if (!res.ok) return null
            const docs: { estado: string; areas: string[]; areasFirmadas: string[] }[] = await res.json()
            const area = AREA_POR_ROL[sesion.rolCodigo]
            const cantidad = sesion.rolCodigo === 'ROL-01'
              ? docs.filter(d => d.estado === 'firmado').length
              : docs.filter(d => area && d.estado === 'pendiente_firma' && d.areas.includes(area) && !d.areasFirmadas.includes(area)).length
            return {
              id: 'firma', moduloId: 'firma',
              titulo: sesion.rolCodigo === 'ROL-01' ? 'Pendientes de tu aprobación' : 'Pendientes de tu firma',
              descripcion: sesion.rolCodigo === 'ROL-01'
                ? 'Documentos firmados por las áreas, listos para aprobar'
                : `Documentos del área ${area ?? ''} que esperan tu firma`,
              cantidad,
              icono: <IconFileCheck size={20} />,
            }
          } catch {
            return null
          }
        })())
      }

      const resultados = (await Promise.all(tareas)).filter((p): p is Pendiente => p !== null)
      if (!cancelado) setPendientes(resultados)
    }

    cargar()
    return () => { cancelado = true }
    // Se recalcula solo al cambiar de sesión
  }, [sesion.rolCodigo])

  const pendientesTotales = pendientes?.reduce((s, p) => s + (p.cantidad ?? 0), 0) ?? 0

  return (
    <Box p="xl" h="100%" style={{ overflowY: 'auto' }}>
      <Stack gap="xl" maw={1100}>
        <Group justify="space-between" align="flex-end">
          <div>
            <Title order={2} fw={600}>Hola, {sesion.nombre}</Title>
            <Text c="dimmed" size="sm">{sesion.rolNombre} · {formatearFecha(new Date())}</Text>
          </div>
          {pendientes && (
            <Badge size="lg" variant="light" color={pendientesTotales > 0 ? 'orange' : 'teal'}>
              {pendientesTotales > 0 ? `${pendientesTotales} pendientes` : 'Todo al día'}
            </Badge>
          )}
        </Group>

        <Stack gap="sm">
          <Text size="xs" fw={600} tt="uppercase" c="dimmed" style={{ letterSpacing: '0.04em' }}>Para vos hoy</Text>
          {pendientes === null ? (
            <Text c="dimmed" size="sm">Cargando pendientes…</Text>
          ) : pendientes.length === 0 ? (
            <Card withBorder radius="md" p="lg">
              <Group gap="sm">
                <ThemeIcon variant="light" color="teal" size="lg" radius="md"><IconFileCheck size={20} /></ThemeIcon>
                <Text size="sm" c="dimmed">No tenés tareas pendientes en tus módulos.</Text>
              </Group>
            </Card>
          ) : (
            <SimpleGrid cols={{ base: 1, md: 2, xl: 3 }} spacing="md">
              {pendientes.map(p => {
                const modulo = modulos.find(m => m.id === p.moduloId)
                const conPendientes = (p.cantidad ?? 0) > 0
                return (
                  <Card key={p.id} withBorder radius="md" p="lg" component={Link} to={modulo?.path ?? '#'} style={{ textDecoration: 'none', color: 'inherit' }}>
                    <Group justify="space-between" align="flex-start" wrap="nowrap">
                      <ThemeIcon variant="light" color={conPendientes ? 'orange' : 'teal'} size="lg" radius="md">
                        {conPendientes ? <IconAlertCircle size={20} /> : p.icono}
                      </ThemeIcon>
                      <Text fz={32} fw={700} lh={1} c={conPendientes ? 'orange.8' : 'teal.8'}>
                        {p.cantidad ?? '—'}
                      </Text>
                    </Group>
                    <Text fw={600} mt="md">{p.titulo}</Text>
                    <Text size="sm" c="dimmed" mt={4}>{p.descripcion}</Text>
                    <Group justify="flex-end" mt="md">
                      <Text size="sm" c="teal.7" fw={500}>Ir al módulo</Text>
                      <IconArrowRight size={14} color="var(--mantine-color-teal-7)" />
                    </Group>
                  </Card>
                )
              })}
            </SimpleGrid>
          )}
        </Stack>

        <Stack gap="sm">
          <Text size="xs" fw={600} tt="uppercase" c="dimmed" style={{ letterSpacing: '0.04em' }}>Accesos rápidos</Text>
          <Group gap="sm">
            {modulos.filter(m => m.id !== 'inicio').map(m => (
              <Button
                key={m.id}
                component={Link}
                to={m.path}
                variant="default"
                radius="md"
                leftSection={ICONO_MODULO[m.id]}
              >
                {m.label}
              </Button>
            ))}
          </Group>
        </Stack>
      </Stack>
    </Box>
  )
}
