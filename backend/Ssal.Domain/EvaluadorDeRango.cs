namespace Ssal.Domain;

// Compara el valor cargado de un resultado contra el rango/valor esperado del análisis.
public static class EvaluadorDeRango
{
    public static bool? FueraDeRango(string? valor, Analisis analisis)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;

        if (analisis.RangoMin.HasValue || analisis.RangoMax.HasValue)
        {
            if (!decimal.TryParse(valor, out var numero)) return null;
            if (analisis.RangoMin.HasValue && numero < analisis.RangoMin.Value) return true;
            if (analisis.RangoMax.HasValue && numero > analisis.RangoMax.Value) return true;
            return false;
        }

        if (!string.IsNullOrWhiteSpace(analisis.ValorEsperado))
            return !string.Equals(valor.Trim(), analisis.ValorEsperado.Trim(), StringComparison.OrdinalIgnoreCase);

        return null;
    }
}
