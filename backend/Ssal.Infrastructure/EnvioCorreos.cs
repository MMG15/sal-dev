using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Utils;
using Ssal.Application.Comun;
using Ssal.Domain;

namespace Ssal.Infrastructure;

// Manda correos reales usando la casilla configurada en CuentaCorreo (mismo usuario/contraseña
// de aplicación que ya se usa para IMAP). Se usa para responder consultas y para mandar
// presupuestos, de modo que el remitente real sea siempre la casilla del laboratorio: si no,
// la respuesta del cliente nunca vuelve a esa casilla y el sistema no la puede leer.
public class EnvioCorreos(ILogger<EnvioCorreos> logger) : IEnvioCorreos
{
    public async Task<string> EnviarAsync(
        CuentaCorreo cuenta,
        string passwordPlano,
        string destinatario,
        string asunto,
        string cuerpoTexto,
        string? inReplyTo,
        IReadOnlyList<AdjuntoCorreo>? adjuntos,
        CancellationToken ct)
    {
        var mensaje = new MimeMessage();
        mensaje.From.Add(new MailboxAddress("Laboratorio Control de Calidad \"Dr. Alberto Graffigna\"", cuenta.Usuario));
        mensaje.To.Add(MailboxAddress.Parse(destinatario));
        mensaje.Subject = asunto;
        mensaje.MessageId = MimeUtils.GenerateMessageId(cuenta.Usuario.Split('@').Last());

        if (!string.IsNullOrWhiteSpace(inReplyTo))
        {
            mensaje.InReplyTo = inReplyTo;
            mensaje.References.Add(inReplyTo);
        }

        var builder = new BodyBuilder { TextBody = cuerpoTexto };
        if (adjuntos is not null)
        {
            foreach (var adjunto in adjuntos)
                builder.Attachments.Add(adjunto.NombreArchivo, adjunto.Contenido, ContentType.Parse(adjunto.TipoMime));
        }
        mensaje.Body = builder.ToMessageBody();

        using (var smtp = new SmtpClient())
        {
            await smtp.ConnectAsync(cuenta.ServidorSmtp, cuenta.PuertoSmtp, SecureSocketOptions.StartTls, ct);
            await smtp.AuthenticateAsync(cuenta.Usuario, passwordPlano, ct);
            await smtp.SendAsync(mensaje, ct);
            await smtp.DisconnectAsync(true, ct);
        }

        // Gmail debería guardar sola una copia en "Enviados" al autenticar por SMTP con la misma
        // cuenta, pero no es un comportamiento garantizado (y no todos los proveedores lo hacen).
        // Para no depender de eso, la guardamos nosotros mismos por IMAP. Si esto falla, el mail
        // ya se mandó igual: no vale la pena hacer fallar todo el envío por esto.
        try
        {
            using var imap = new ImapClient();
            await imap.ConnectAsync(cuenta.Servidor, cuenta.Puerto, SecureSocketOptions.SslOnConnect, ct);
            await imap.AuthenticateAsync(cuenta.Usuario, passwordPlano, ct);
            var carpetaEnviados = imap.GetFolder(SpecialFolder.Sent) ?? imap.Inbox;
            await carpetaEnviados.OpenAsync(FolderAccess.ReadWrite, ct);
            await carpetaEnviados.AppendAsync(mensaje, MessageFlags.Seen, ct);
            await imap.DisconnectAsync(true, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "El mail a {Destinatario} se mandó bien, pero no se pudo guardar la copia en Enviados de {Cuenta}.", destinatario, cuenta.Usuario);
        }

        return mensaje.MessageId;
    }
}
