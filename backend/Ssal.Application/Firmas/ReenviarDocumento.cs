namespace Ssal.Application.Firmas;

// El autor reenvía el documento corregido: vuelve a empezar el ciclo de firma
public class ReenviarDocumento(IFirmaRepository repo, CargadorEstadoDocumento cargador)
{
    public async Task<Resultado> EjecutarAsync(string tipo, int id, int idUsuario, string rolActual)
    {
        var estado = await cargador.CargarAsync(tipo, id);
        if (estado is null) return Resultado.Fallido("No encontrado");
        var (doc, _, ciclo) = estado.Value;

        if (!MapeoDocumentos.CalcularPermisos(doc, ciclo, rolActual).Reenviar)
            return Resultado.Fallido("El documento no está rechazado o tu rol no puede reenviarlo.");

        repo.AgregarFirma(new Ssal.Domain.FirmaDocumento
        {
            IdFirmante = idUsuario,
            TipoDocumento = tipo,
            IdDocumento = id,
            TipoFirma = "reenvio",
            Estado = "reenviado",
            FechaFirma = DateTime.UtcNow
        });
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
