import { useNavigate } from 'react-router-dom'
import { useSesion } from '../context/SesionContext'
import { modulosParaRol } from '../config/modulos'
import { MODULO_ICONOS } from '../layout/icons'
import styles from './InicioPage.module.css'

const ACENTO: Record<string, string> = {
  clientes: styles.acentoInfo,
  consultas: styles.acentoDorado,
  presupuestos: styles.acentoAzul,
  sse: styles.acentoTeal,
  analisis: styles.acentoTeal,
  resultados: styles.acentoInfo,
  facturacion: styles.acentoDorado,
  firma: styles.acentoBurdeo,
  usuarios: styles.acentoAzul,
  portal: styles.acentoTeal,
}

function formatearFecha(fecha: Date) {
  const texto = fecha.toLocaleDateString('es-AR', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  })
  return texto.charAt(0).toUpperCase() + texto.slice(1)
}

export default function InicioPage() {
  const { sesion } = useSesion()
  const navigate = useNavigate()
  const modulos = modulosParaRol(sesion.rolCodigo).filter(m => m.id !== 'inicio')

  return (
    <div>
      <div className={styles.encabezado}>
        <div>
          <h2 className={styles.bienvenida}>Bienvenido, {sesion.nombre}</h2>
          <p className={styles.subtitulo}>{sesion.rolNombre}</p>
        </div>
        <span className={styles.fecha}>{formatearFecha(new Date())}</span>
      </div>

      <p className={styles.seccionTitulo}>Módulos disponibles</p>
      <div className={styles.grilla}>
        {modulos.map(m => (
          <button
            key={m.id}
            className={`${styles.tarjeta} ${!m.listo ? styles.pendiente : ''}`}
            onClick={() => navigate(m.path)}
            disabled={!m.listo}
          >
            <span className={`${styles.icono} ${ACENTO[m.id] ?? ''}`}>{MODULO_ICONOS[m.id]}</span>
            <span className={styles.tarjetaTextos}>
              <span className={styles.tarjetaLabelFila}>
                <span className={styles.tarjetaLabel}>{m.label}</span>
                {!m.listo && <span className={styles.proximamente}>Pronto</span>}
              </span>
              <span className={styles.tarjetaDesc}>{m.descripcion}</span>
            </span>
          </button>
        ))}
      </div>
    </div>
  )
}
