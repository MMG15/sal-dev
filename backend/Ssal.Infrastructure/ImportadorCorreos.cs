using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MimeKit;
using Ssal.Infrastructure.Data;
using Ssal.Domain;

namespace Ssal.Infrastructure;

// Trae los correos recientes de una casilla y los deja en la bandeja (CorreoEntrante) para que
// el personal decida si son un pedido real o no. No manda ni responde nada: solo lee.
public class ImportadorCorreos(SsalDbContext db, ILogger<ImportadorCorreos> logger)
{
    // Margen de seguridad sobre la última revisión: la mayoría de los servidores IMAP solo
    // soportan granularidad de día en el SINCE del protocolo, y además así no se pierde nada si
    // llegó un mail justo antes del corte. No importa que la ventana traiga mensajes ya
    // importados de vuelta — el antiduplicado real es el Message-Id contra la base, no esto.
    private static readonly TimeSpan MargenRevision = TimeSpan.FromDays(2);
    private static readonly TimeSpan VentanaPrimeraRevision = TimeSpan.FromDays(30);

    // Se le pasa la cuenta y su contraseña ya desencriptada (ver Cifrado.cs); nunca
    // guarda ni loguea la contraseña en texto plano.
    public async Task<int> ImportarAsync(CuentaCorreo cuenta, string passwordPlano, CancellationToken ct)
    {
        using var client = new ImapClient();
        await client.ConnectAsync(cuenta.Servidor, cuenta.Puerto, SecureSocketOptions.SslOnConnect, ct);
        await client.AuthenticateAsync(cuenta.Usuario, passwordPlano, ct);

        var inbox = client.Inbox;
        await inbox.OpenAsync(FolderAccess.ReadOnly, ct);

        // Antes se buscaba SearchQuery.NotSeen (no leído), pero eso depende del estado del
        // buzón: si alguien abre el mail directo en Gmail (sin pasar por el sistema), queda
        // "visto" y el importador lo ignoraba para siempre, sin reintento posible. Se busca por
        // fecha en su lugar; lo que ya está importado se filtra abajo por Message-Id.
        var desde = cuenta.UltimaRevision is not null
            ? cuenta.UltimaRevision.Value - MargenRevision
            : DateTime.UtcNow - VentanaPrimeraRevision;
        var candidatos = await inbox.SearchAsync(SearchQuery.DeliveredAfter(desde), ct);
        var importados = 0;

        foreach (var uid in candidatos)
        {
            try
            {
                var mensaje = await inbox.GetMessageAsync(uid, ct);
                var remitente = mensaje.From.Mailboxes.FirstOrDefault();
                // El Message-Id identifica al mail de forma única; si por algún motivo no viene,
                // armamos uno propio para no perder el antiduplicado.
                var externoId = !string.IsNullOrWhiteSpace(mensaje.MessageId)
                    ? mensaje.MessageId
                    : $"sin-message-id-{cuenta.IdCuenta}-{uid.Id}";

                var yaExiste = await db.CorreosEntrantes.AnyAsync(c => c.ExternoId == externoId, ct);
                if (yaExiste) continue;

                var idConsultaRelacionada = await BuscarConsultaRelacionadaAsync(mensaje, ct);

                db.CorreosEntrantes.Add(new CorreoEntrante
                {
                    ExternoId = externoId,
                    Remitente = remitente?.Address ?? "desconocido",
                    RemitenteNombre = remitente?.Name,
                    Asunto = mensaje.Subject,
                    Cuerpo = mensaje.TextBody ?? mensaje.HtmlBody,
                    FechaRecibido = mensaje.Date.UtcDateTime,
                    Estado = "pendiente",
                    IdCuentaCorreo = cuenta.IdCuenta,
                    IdConsultaRelacionada = idConsultaRelacionada
                });
                await db.SaveChangesAsync(ct);
                importados++;
            }
            catch (Exception ex)
            {
                // Un mail con formato raro no debe frenar la importación de los demás.
                logger.LogWarning(ex, "No se pudo importar el mensaje {Uid} de la cuenta {Cuenta}.", uid, cuenta.Usuario);
            }
        }

        await client.DisconnectAsync(true, ct);
        return importados;
    }

    // Si este mail es una respuesta a algo que el sistema mandó (un presupuesto, o una
    // respuesta a otro correo), lo detecta por las cabeceras In-Reply-To/References y
    // devuelve la consulta con la que hay que vincularlo. No crea ni modifica nada: solo
    // identifica la relación para que quede visible en la bandeja.
    private async Task<int?> BuscarConsultaRelacionadaAsync(MimeMessage mensaje, CancellationToken ct)
    {
        var candidatos = new List<string>();
        if (!string.IsNullOrWhiteSpace(mensaje.InReplyTo))
            candidatos.Add(mensaje.InReplyTo);
        candidatos.AddRange(mensaje.References);

        if (candidatos.Count == 0) return null;

        var presupuesto = await db.Presupuestos
            .Where(p => p.MessageIdEnviado != null && candidatos.Contains(p.MessageIdEnviado))
            .Select(p => new { p.IdConsulta })
            .FirstOrDefaultAsync(ct);
        if (presupuesto is not null) return presupuesto.IdConsulta;

        var correoRespondido = await db.CorreosEntrantes
            .Where(c => c.MessageIdEnviado != null && candidatos.Contains(c.MessageIdEnviado))
            .Select(c => new { c.IdConsultaGenerada })
            .FirstOrDefaultAsync(ct);
        if (correoRespondido?.IdConsultaGenerada is not null) return correoRespondido.IdConsultaGenerada;

        var consultaOriginal = await db.Consultas
            .Where(c => c.ExternoId != null && candidatos.Contains(c.ExternoId))
            .Select(c => new { c.IdConsulta })
            .FirstOrDefaultAsync(ct);
        return consultaOriginal?.IdConsulta;
    }
}
