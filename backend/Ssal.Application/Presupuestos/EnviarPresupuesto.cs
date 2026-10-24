using Ssal.Application.Comun;

namespace Ssal.Application.Presupuestos;

// Manda el presupuesto por mail de verdad (SMTP, con el PDF adjunto), desde la casilla
// por la que entró la consulta original (si vino de un correo) o la primera habilitada.
// Así la respuesta del cliente (aceptando o no) vuelve a esa misma casilla y el
// importador la puede reconocer y vincular a esta consulta.
public class EnviarPresupuesto(
    IPresupuestoRepository repo,
    IPresupuestoPdfGenerator generadorPdf,
    IEnvioCorreos envio,
    ICifradoContrasenas descifrador)
{
    public async Task<Resultado> EjecutarAsync(int id, EnviarPresupuestoCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.Destinatario))
            return Resultado.Fallido("El destinatario no tiene un email registrado.");

        var p = await repo.ObtenerConConsultaAsync(id);
        if (p is null) return Resultado.Fallido("No encontrado");

        var idCuentaOrigen = await repo.ObtenerIdCuentaOrigenAsync(p.IdConsulta);
        var cuenta = idCuentaOrigen is not null ? await repo.ObtenerCuentaAsync(idCuentaOrigen.Value) : null;
        cuenta ??= await repo.ObtenerPrimeraCuentaHabilitadaAsync();
        if (cuenta is null)
            return Resultado.Fallido("No hay ninguna cuenta de correo configurada para enviar presupuestos.");

        var pFull = await repo.ObtenerCompletoAsync(id) ?? throw new InvalidOperationException("El presupuesto desapareció durante el envío.");
        var eventos = await repo.ObtenerEventosFirmaAsync(id);
        var pdfBytes = generadorPdf.Generar(pFull, Ssal.Domain.CicloFirma.EventosCicloActual(eventos));
        var nombreArchivo = $"{p.CodigoRpo ?? "presupuesto"}.pdf";

        string messageId;
        try
        {
            var passwordPlano = descifrador.Desencriptar(cuenta.PasswordCifrada);
            messageId = await envio.EnviarAsync(
                cuenta, passwordPlano, cmd.Destinatario, cmd.Asunto, cmd.Cuerpo,
                p.Consulta.ExternoId,
                [new AdjuntoCorreo(nombreArchivo, pdfBytes, "application/pdf")],
                ct);
        }
        catch (Exception ex)
        {
            return Resultado.Fallido($"No se pudo enviar el presupuesto por email: {ex.Message}");
        }

        p.MessageIdEnviado = messageId;
        if (p.Estado == "borrador") p.Estado = "enviado";
        await repo.GuardarCambiosAsync();

        return Resultado.Exitoso();
    }
}
