namespace Ssal.Application.Comun;

public record AdjuntoCorreo(string NombreArchivo, byte[] Contenido, string TipoMime);

// La implementación real (SMTP/IMAP vía MailKit) vive en Ssal.Infrastructure.
public interface IEnvioCorreos
{
    Task<string> EnviarAsync(
        Ssal.Domain.CuentaCorreo cuenta,
        string passwordPlano,
        string destinatario,
        string asunto,
        string cuerpoTexto,
        string? inReplyTo,
        IReadOnlyList<AdjuntoCorreo>? adjuntos,
        CancellationToken ct);
}
