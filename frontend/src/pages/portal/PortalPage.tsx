import PlaceholderModulo from '../../components/PlaceholderModulo'

export default function PortalPage() {
  return (
    <PlaceholderModulo
      titulo="Mis resultados"
      descripcion="Portal web para que el cliente externo visualice y descargue sus informes."
      requisitos={[
        'RF-04 — Credenciales asignadas al registrar la SSE',
        'RF-22 — Visualización de resultados propios',
        'RF-23 — Estado de análisis en curso (resultados parciales)',
        'CU-07 — Descarga del informe PDF',
      ]}
    />
  )
}
