import styles from './PlaceholderModulo.module.css'

interface Props {
  titulo: string
  descripcion: string
  requisitos: string[]
}

export default function PlaceholderModulo({ titulo, descripcion, requisitos }: Props) {
  return (
    <div className={styles.contenedor}>
      <span className={styles.badge}>En construcción</span>
      <h2 className={styles.titulo}>{titulo}</h2>
      <p className={styles.desc}>{descripcion}</p>
      <div className={styles.lista}>
        <p className={styles.listaTitle}>Funcionalidades planificadas</p>
        <ul>
          {requisitos.map(r => (
            <li key={r}>{r}</li>
          ))}
        </ul>
      </div>
    </div>
  )
}
