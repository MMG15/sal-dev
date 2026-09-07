import PlaceholderModulo from '../../components/PlaceholderModulo'

export default function FacturacionPage() {
  return (
    <PlaceholderModulo
      titulo="Facturación"
      descripcion="Generación, anulación y control de facturas vinculadas a las SSE."
      requisitos={[
        'RF-24 — Generar facturas a partir de la SSE registrada',
        'RF-25 — Anular facturas con registro del motivo',
        'RF-26 — Listado de facturas adeudadas por cliente',
        'RF-27 — Detección y notificación de errores en facturación',
        'RF-28 — Facturación centralizada para clientes con múltiples pedidos',
        'CU-08 — Integración con sistema TANGO',
      ]}
    />
  )
}
