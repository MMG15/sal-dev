// Muestra el área como texto legible: "AMBAS" se ve como "MIC y FQ"
export function etiquetaArea(area: string): string {
  return area === 'AMBAS' ? 'MIC y FQ' : area
}
