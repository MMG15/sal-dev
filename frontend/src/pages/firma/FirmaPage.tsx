import PlaceholderModulo from '../../components/PlaceholderModulo'

export default function FirmaPage() {
  return (
    <PlaceholderModulo
      titulo="Firma digital"
      descripcion="Flujo de doble validación: firma del Responsable de área y aprobación del Administrador general."
      requisitos={[
        'RF-32 — Firma digital por Responsable de área (ROL-08 / ROL-09)',
        'RF-33 — Aprobación final por Administrador general (ROL-01)',
        'RF-34 — Documentos firmables: informe, presupuesto, SSE, factura',
        'RF-35 — Registro de quién firmó, aprobó, fecha y hora',
        'RF-36 — Documentos no válidos sin ambas firmas',
        'RF-37 — Notificación al Admin cuando un documento está pendiente',
        'RF-38 — Rechazo con motivo para corrección',
      ]}
    />
  )
}
