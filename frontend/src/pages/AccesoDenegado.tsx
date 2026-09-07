import { useNavigate } from 'react-router-dom'
import styles from './AccesoDenegado.module.css'

export default function AccesoDenegado() {
  const navigate = useNavigate()
  return (
    <div className={styles.contenedor}>
      <p className={styles.codigo}>403</p>
      <h2 className={styles.titulo}>Acceso denegado</h2>
      <p className={styles.desc}>Tu rol no tiene permisos para acceder a este módulo.</p>
      <button className={styles.btn} onClick={() => navigate('/inicio', { replace: true })}>
        Volver al inicio
      </button>
    </div>
  )
}
