import PlaceholderModulo from '../../components/PlaceholderModulo'

export default function ResultadosPage() {
  return (
    <PlaceholderModulo
      titulo="Resultados e informes"
      descripcion="Carga de resultados de análisis y generación de informes PDF con doble validación."
      requisitos={[
        'RF-20 — Cargar resultados por rótulo (acceso restringido al personal autorizado)',
        'RF-21 — Generar informe PDF (RPO 01-04)',
        'RF-22 — Portal web para que el cliente visualice sus resultados',
        'RF-23 — Visualización de resultados parciales durante el análisis',
        'RF-32 a RF-38 — Flujo de firma digital: Resp. Área → Admin general',
        'CU-06 — Cargar y validar resultados, generar PDF y notificar',
      ]}
    />
  )
}
