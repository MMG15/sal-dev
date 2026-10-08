import { createTheme, type MantineColorsTuple } from '@mantine/core'

// Escala del teal institucional (base #0f7c86, mismo color que los tokens de index.css)
const teal: MantineColorsTuple = [
  '#e3f4f5',
  '#c5e9eb',
  '#9bdbdf',
  '#68c9cf',
  '#34b3bb',
  '#159aa3',
  '#0f7c86',
  '#0b6670',
  '#0b5f67',
  '#084a51',
]

export const theme = createTheme({
  primaryColor: 'teal',
  primaryShade: 6,
  colors: { teal },
  fontFamily: "'Segoe UI', system-ui, Roboto, sans-serif",
  // Escala de textos un poco mayor que la de por defecto de Mantine
  fontSizes: {
    xs: '0.85rem',
    sm: '1rem',
    md: '1.1rem',
    lg: '1.25rem',
    xl: '1.45rem',
  },
  defaultRadius: 'md',
  headings: { fontWeight: '600' },
  components: {
    // Las etiquetas de estado no deben cortarse con puntos suspensivos
    Badge: { styles: { label: { overflow: 'visible', textOverflow: 'clip' } } },
  },
})
