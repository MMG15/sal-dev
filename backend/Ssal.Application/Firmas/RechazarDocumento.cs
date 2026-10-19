namespace Ssal.Application.Firmas;

// Rechazo con motivo (RF-38 y E2). Limpia las firmas del ciclo.
public class RechazarDocumento(IFirmaRepository repo, CargadorEstadoDocumento cargador)
{
    public async Task<Resultado> EjecutarAsync(string tipo, int id, RechazarCommand cmd, int idUsuario, string rolActual)
    {
        var estado = await cargador.CargarAsync(tipo, id);
        if (estado is null) return Resultado.Fallido("No encontrado");
        var (doc, _, ciclo) = estado.Value;

        if (!MapeoDocumentos.CalcularPermisos(doc, ciclo, rolActual).Rechazar)
            return Resultado.Fallido("No podés rechazar este documento en su estado actual.");

        var motivo = cmd.Motivo?.Trim();
        if (string.IsNullOrWhiteSpace(motivo))
            return Resultado.Fallido("Indicá el motivo del rechazo.");

        repo.AgregarFirma(new Ssal.Domain.FirmaDocumento
        {
            IdFirmante = idUsuario,
            TipoDocumento = tipo,
            IdDocumento = id,
            TipoFirma = "rechazo",
            Estado = "rechazado",
            MotivoRechazo = motivo,
            FechaFirma = DateTime.UtcNow
        });
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
