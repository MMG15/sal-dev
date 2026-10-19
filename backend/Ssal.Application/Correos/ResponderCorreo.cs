using Ssal.Application.Comun;

namespace Ssal.Application.Correos;

// Manda la respuesta de verdad, por SMTP, desde la misma casilla por la que entró el
// correo (o la primera casilla habilitada si no se sabe cuál fue). Así la respuesta
// queda en "Enviados" de esa cuenta y, si el cliente contesta, su respuesta vuelve a la
// misma casilla que el importador revisa — no a la cuenta personal de quien respondió.
public class ResponderCorreo(ICorreoEntranteRepository repo, IEnvioCorreos envio, ICifradoContrasenas cifrado)
{
    public async Task<Resultado> EjecutarAsync(int id, ResponderCorreoCommand cmd, int idUsuario, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.Cuerpo))
            return Resultado.Fallido("Escribí el contenido de la respuesta.");

        var c = await repo.ObtenerConCuentaAsync(id);
        if (c is null) return Resultado.Fallido("No encontrado");
        if (c.Estado != "pendiente")
            return Resultado.Fallido("Este correo ya fue procesado.");

        var cuenta = c.CuentaCorreo ?? await repo.ObtenerPrimeraCuentaHabilitadaAsync();
        if (cuenta is null)
            return Resultado.Fallido("No hay ninguna cuenta de correo configurada para enviar la respuesta.");

        var asunto = string.IsNullOrWhiteSpace(c.Asunto)
            ? "Re: consulta"
            : c.Asunto.StartsWith("Re:", StringComparison.OrdinalIgnoreCase) ? c.Asunto : $"Re: {c.Asunto}";

        string messageId;
        try
        {
            var passwordPlano = cifrado.Desencriptar(cuenta.PasswordCifrada);
            messageId = await envio.EnviarAsync(cuenta, passwordPlano, c.Remitente, asunto, cmd.Cuerpo, c.ExternoId, null, ct);
        }
        catch (Exception ex)
        {
            return Resultado.Fallido($"No se pudo enviar la respuesta: {ex.Message}");
        }

        c.MessageIdEnviado = messageId;
        c.Estado = "respondido";
        c.IdUsuarioAccion = idUsuario;
        c.FechaAccion = DateTime.UtcNow;
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
