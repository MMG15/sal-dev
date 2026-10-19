namespace Ssal.Application.Portal;

// Compartido por los casos de uso del portal: un informe solo se muestra/descarga cuando
// está aprobado (RF-36).
internal static class EstadoInforme
{
    public static bool EstaAprobado(int idInforme, List<string> areas, List<Ssal.Domain.FirmaDocumento> firmas) =>
        Ssal.Domain.CicloFirma.Calcular(firmas.Where(f => f.IdDocumento == idInforme), areas).Estado == Ssal.Domain.CicloFirma.Aprobado;

    public static string? ReferenciaTexto(Ssal.Domain.Analisis a)
    {
        if (a.ValorEsperado is not null) return $"Esperado: {a.ValorEsperado}";
        if (a.RangoMin is not null || a.RangoMax is not null)
            return $"{a.RangoMin?.ToString() ?? "—"} a {a.RangoMax?.ToString() ?? "—"}";
        return null;
    }
}
