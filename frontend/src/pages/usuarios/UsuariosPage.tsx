import PlaceholderModulo from '../../components/PlaceholderModulo'

export default function UsuariosPage() {
  return (
    <PlaceholderModulo
      titulo="Gestión de usuarios"
      descripcion="Alta, baja, modificación y asignación de roles de acceso al sistema."
      requisitos={[
        'RF-31 — Gestión de roles: ROL-01 al ROL-09',
        'RF-30 — Registro de accesos y acciones críticas por usuario',
        'CU-10 — Alta, baja y modificación de cuentas',
        'CU-11 — Habilitación / deshabilitación de cuentas',
        'Configuración de parámetros operativos del sistema',
      ]}
    />
  )
}
