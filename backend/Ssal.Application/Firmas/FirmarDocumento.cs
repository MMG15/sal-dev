namespace Ssal.Application.Firmas;

// Firma del responsable del área (primer paso, RF-32)
public class FirmarDocumento(IFirmaRepository repo, CargadorEstadoDocumento cargador, VerificadorPasswordUsuario verificador)
{
    public async Task<Resultado> EjecutarAsync(string tipo, int id, ConfirmarPasswordCommand cmd, int idUsuario, string rolActual)
    {
        var estado = await cargador.CargarAsync(tipo, id);
        if (estado is null) return Resultado.Fallido("No encontrado");
        var (doc, _, ciclo) = estado.Value;

        var area = MapeoDocumentos.AreaDelRol(rolActual);
        if (!MapeoDocumentos.CalcularPermisos(doc, ciclo, rolActual).Firmar || area is null)
            return Resultado.Fallido($"El documento no está pendiente de firma de {area ?? "tu área"}.");

        if (!await verificador.EsValidaAsync(cmd.Password, idUsuario))
            return Resultado.Fallido("Contraseña incorrecta.");

        repo.AgregarFirma(new Ssal.Domain.FirmaDocumento
        {
            IdFirmante = idUsuario,
            TipoDocumento = tipo,
            IdDocumento = id,
            TipoFirma = "responsable",
            Area = area,
            Estado = "firmado",
            FechaFirma = DateTime.UtcNow
        });
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
