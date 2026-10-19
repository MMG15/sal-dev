namespace Ssal.Application.Firmas;

// Aprobación final del Administrador general (RF-33)
public class AprobarDocumento(IFirmaRepository repo, CargadorEstadoDocumento cargador, VerificadorPasswordUsuario verificador)
{
    public async Task<Resultado> EjecutarAsync(string tipo, int id, ConfirmarPasswordCommand cmd, int idUsuario, string rolActual)
    {
        var estado = await cargador.CargarAsync(tipo, id);
        if (estado is null) return Resultado.Fallido("No encontrado");
        var (doc, _, ciclo) = estado.Value;

        if (!MapeoDocumentos.CalcularPermisos(doc, ciclo, rolActual).Aprobar)
            return Resultado.Fallido("El documento todavía no tiene todas las firmas de los responsables de área.");

        if (!await verificador.EsValidaAsync(cmd.Password, idUsuario))
            return Resultado.Fallido("Contraseña incorrecta.");

        repo.AgregarFirma(new Ssal.Domain.FirmaDocumento
        {
            IdFirmante = idUsuario,
            TipoDocumento = tipo,
            IdDocumento = id,
            TipoFirma = "aprobacion",
            Estado = "aprobado",
            FechaFirma = DateTime.UtcNow
        });
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
