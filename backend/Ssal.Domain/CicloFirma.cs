namespace Ssal.Domain;

// Calcula el estado de un documento a partir de su historial de firmas.
// Flujo: firma de cada área involucrada (responsable) → aprobación del Administrador.
// Un rechazo limpia las firmas previas; al reenviar hay que volver a firmar (RF-38).
public static class CicloFirma
{
    public const string PendienteFirma = "pendiente_firma";
    public const string Firmado = "firmado";     // firmado por todas las áreas, pendiente de aprobación
    public const string Rechazado = "rechazado";
    public const string Aprobado = "aprobado";

    public record Resultado(string Estado, HashSet<string> AreasFirmadas);

    public static Resultado Calcular(IEnumerable<FirmaDocumento> eventos, IReadOnlyCollection<string> areas)
    {
        var estado = PendienteFirma;
        var firmadas = new HashSet<string>();

        foreach (var e in eventos.OrderBy(e => e.IdFirma))
        {
            switch (e.TipoFirma)
            {
                case "responsable":
                    if (estado == PendienteFirma && e.Area is not null)
                    {
                        firmadas.Add(e.Area);
                        if (areas.All(firmadas.Contains)) estado = Firmado;
                    }
                    break;
                case "aprobacion":
                    if (estado == Firmado) estado = Aprobado;
                    break;
                case "rechazo":
                    estado = Rechazado;
                    firmadas.Clear();
                    break;
                case "reenvio":
                    if (estado == Rechazado) estado = PendienteFirma;
                    break;
            }
        }

        return new Resultado(estado, firmadas);
    }

    // Eventos del ciclo vigente: los posteriores al último rechazo.
    public static List<FirmaDocumento> EventosCicloActual(IEnumerable<FirmaDocumento> eventos)
    {
        var lista = eventos.OrderBy(e => e.IdFirma).ToList();
        var ultimoRechazo = lista.FindLastIndex(e => e.TipoFirma == "rechazo");
        return ultimoRechazo < 0 ? lista : lista.Skip(ultimoRechazo + 1).ToList();
    }
}
