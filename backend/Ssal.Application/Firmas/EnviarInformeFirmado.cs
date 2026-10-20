using Ssal.Application.Comun;

namespace Ssal.Application.Firmas;

// Entrega del informe ya aprobado (todas las firmas de área + aprobación del administrador)
// al cliente o a la empresa de la SSE, por mail con el PDF firmado adjunto.
public class EnviarInformeFirmado(
    IFirmaRepository repo,
    CargadorEstadoDocumento cargador,
    Ssal.Application.Resultados.IInformePdfGenerator generadorPdf,
    IEnvioCorreos envio,
    ICifradoContrasenas descifrador)
{
    public async Task<Resultado> EjecutarAsync(int idInforme, EnviarInformeFirmadoCommand cmd, CancellationToken ct)
    {
        var estado = await cargador.CargarAsync("informe", idInforme);
        if (estado is null) return Resultado.Fallido("No encontrado");
        var (_, eventos, ciclo) = estado.Value;

        if (ciclo.Estado != Ssal.Domain.CicloFirma.Aprobado)
            return Resultado.Fallido("El informe todavía no está aprobado.");

        if (string.IsNullOrWhiteSpace(cmd.Destinatario))
            return Resultado.Fallido("El cliente/empresa de esta SSE no tiene un email registrado.");

        var cuenta = await repo.ObtenerPrimeraCuentaHabilitadaAsync();
        if (cuenta is null)
            return Resultado.Fallido("No hay ninguna cuenta de correo configurada para enviar.");

        var informe = await repo.ObtenerInformeCompletoAsync(idInforme)
            ?? throw new InvalidOperationException("El informe desapareció durante el envío.");
        var pdfBytes = generadorPdf.Generar(informe, ciclo.Estado, Ssal.Domain.CicloFirma.EventosCicloActual(eventos));
        var nombreArchivo = $"{informe.Codigo}.pdf";

        try
        {
            var passwordPlano = descifrador.Desencriptar(cuenta.PasswordCifrada);
            await envio.EnviarAsync(
                cuenta, passwordPlano, cmd.Destinatario, cmd.Asunto, cmd.Cuerpo,
                null,
                [new AdjuntoCorreo(nombreArchivo, pdfBytes, "application/pdf")],
                ct);
        }
        catch (Exception ex)
        {
            return Resultado.Fallido($"No se pudo enviar el informe por email: {ex.Message}");
        }

        return Resultado.Exitoso();
    }
}
