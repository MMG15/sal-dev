using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;
using Ssal.Api.Services;

namespace Ssal.Api.Controllers;

// Bandeja de correo del laboratorio (RF-06 / preparación): todo lo que llega al mail de
// consultas pasa primero por acá. Mismos roles que pueden gestionar Consultas, porque
// convertir un correo es, en los hechos, crear una Consulta.
[ApiController]
[Route("api/correos-entrantes")]
[Authorize(Roles = "ROL-01,ROL-02,ROL-03,ROL-07")]
public class CorreosEntrantesController(SsalDbContext db, IConfiguration config, EnvioCorreos envio) : ControllerBase
{
    private string ClaveCifrado => config["MailImport:ClaveCifrado"]
        ?? throw new InvalidOperationException("MailImport:ClaveCifrado no está configurada.");

    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/correos-entrantes ────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20)
    {
        var q = db.CorreosEntrantes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(c => c.Estado == estado);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(c =>
                (c.Asunto != null && c.Asunto.ToLower().Contains(b)) ||
                c.Remitente.ToLower().Contains(b) ||
                (c.RemitenteNombre != null && c.RemitenteNombre.ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(c => c.FechaRecibido)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(c => new
            {
                c.IdCorreo,
                c.Remitente,
                c.RemitenteNombre,
                c.Asunto,
                Resumen = c.Cuerpo != null && c.Cuerpo.Length > 160 ? c.Cuerpo.Substring(0, 160) : c.Cuerpo,
                c.FechaRecibido,
                c.Estado,
                c.IdConsultaGenerada,
                c.IdConsultaRelacionada
            })
            .ToListAsync();

        var pendientes = await db.CorreosEntrantes.CountAsync(c => c.Estado == "pendiente");

        return Ok(new { total, pagina, porPagina, pendientes, items });
    }

    // ─── GET /api/correos-entrantes/{id} ───────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var c = await db.CorreosEntrantes
            .Include(x => x.UsuarioAccion)
            .Include(x => x.ConsultaGenerada)
            .Include(x => x.ConsultaRelacionada).ThenInclude(cr => cr!.Presupuesto)
            .FirstOrDefaultAsync(x => x.IdCorreo == id);
        if (c is null) return NotFound();

        return Ok(new
        {
            c.IdCorreo,
            c.Remitente,
            c.RemitenteNombre,
            c.Asunto,
            c.Cuerpo,
            c.FechaRecibido,
            c.Estado,
            UsuarioAccion = c.UsuarioAccion == null ? null : new { c.UsuarioAccion.Nombre, c.UsuarioAccion.Apellido },
            c.FechaAccion,
            ConsultaGenerada = c.ConsultaGenerada == null ? null : new { c.ConsultaGenerada.IdConsulta, c.ConsultaGenerada.Codigo },
            ConsultaRelacionada = c.ConsultaRelacionada == null ? null : new
            {
                c.ConsultaRelacionada.IdConsulta,
                c.ConsultaRelacionada.Codigo,
                Presupuesto = c.ConsultaRelacionada.Presupuesto == null ? null : new
                {
                    c.ConsultaRelacionada.Presupuesto.IdPresupuesto,
                    c.ConsultaRelacionada.Presupuesto.CodigoRpo,
                    c.ConsultaRelacionada.Presupuesto.Estado
                }
            }
        });
    }

    // ─── PUT /api/correos-entrantes/{id}/responder ─────────────────────────
    // Manda la respuesta de verdad, por SMTP, desde la misma casilla por la que entró el
    // correo (o la primera casilla habilitada si no se sabe cuál fue). Así la respuesta
    // queda en "Enviados" de esa cuenta y, si el cliente contesta, su respuesta vuelve a la
    // misma casilla que el importador revisa — no a la cuenta personal de quien respondió.
    [HttpPut("{id:int}/responder")]
    public async Task<IActionResult> Responder(int id, [FromBody] ResponderCorreoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Cuerpo))
            return BadRequest(new { error = "Escribí el contenido de la respuesta." });

        var c = await db.CorreosEntrantes.Include(x => x.CuentaCorreo).FirstOrDefaultAsync(x => x.IdCorreo == id);
        if (c is null) return NotFound();
        if (c.Estado != "pendiente")
            return BadRequest(new { error = "Este correo ya fue procesado." });

        var cuenta = c.CuentaCorreo ?? await db.CuentasCorreo.Where(x => x.Habilitada).OrderBy(x => x.IdCuenta).FirstOrDefaultAsync();
        if (cuenta is null)
            return BadRequest(new { error = "No hay ninguna cuenta de correo configurada para enviar la respuesta." });

        var asunto = string.IsNullOrWhiteSpace(c.Asunto)
            ? "Re: consulta"
            : c.Asunto.StartsWith("Re:", StringComparison.OrdinalIgnoreCase) ? c.Asunto : $"Re: {c.Asunto}";

        string messageId;
        try
        {
            var passwordPlano = Cifrado.Desencriptar(cuenta.PasswordCifrada, ClaveCifrado);
            messageId = await envio.EnviarAsync(cuenta, passwordPlano, c.Remitente, asunto, req.Cuerpo, c.ExternoId, null, HttpContext.RequestAborted);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status502BadGateway, new { error = $"No se pudo enviar la respuesta: {ex.Message}" });
        }

        c.MessageIdEnviado = messageId;
        c.Estado = "respondido";
        c.IdUsuarioAccion = UsuarioActual;
        c.FechaAccion = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── PUT /api/correos-entrantes/{id}/descartar ─────────────────────────
    [HttpPut("{id:int}/descartar")]
    public async Task<IActionResult> Descartar(int id)
    {
        var c = await db.CorreosEntrantes.FindAsync(id);
        if (c is null) return NotFound();
        if (c.Estado != "pendiente")
            return BadRequest(new { error = "Este correo ya fue procesado." });

        c.Estado = "descartado";
        c.IdUsuarioAccion = UsuarioActual;
        c.FechaAccion = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── POST /api/correos-entrantes/{id}/convertir ────────────────────────
    // Crea una Consulta formal (mismo correlativo CF-AAAAMMDD-NNN que la carga manual).
    [HttpPost("{id:int}/convertir")]
    public async Task<IActionResult> Convertir(int id, [FromBody] ConvertirCorreoRequest req)
    {
        var correo = await db.CorreosEntrantes.FindAsync(id);
        if (correo is null) return NotFound();
        if (correo.Estado != "pendiente")
            return BadRequest(new { error = "Este correo ya fue procesado." });

        var hoy = DateTime.UtcNow;
        var prefijo = $"CF-{hoy:yyyyMMdd}-";
        var ultimoCodigo = await db.Consultas
            .Where(c => c.Codigo != null && c.Codigo.StartsWith(prefijo))
            .OrderByDescending(c => c.Codigo)
            .Select(c => c.Codigo)
            .FirstOrDefaultAsync();

        int siguiente = 1;
        if (ultimoCodigo is not null)
        {
            var partes = ultimoCodigo.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out var n))
                siguiente = n + 1;
        }
        var codigo = $"{prefijo}{siguiente:D3}";

        var descripcion = string.IsNullOrWhiteSpace(correo.Asunto)
            ? correo.Cuerpo
            : $"{correo.Asunto}\n\n{correo.Cuerpo}";

        var consulta = new Consulta
        {
            Codigo = codigo,
            Canal = "mail",
            Estado = "recibida",
            Descripcion = descripcion,
            Fecha = correo.FechaRecibido,
            CreatedAt = hoy,
            IdCliente = req.IdCliente,
            IdEmpresa = req.IdEmpresa,
            Origen = "automatico",
            ExternoId = correo.ExternoId
        };
        db.Consultas.Add(consulta);

        correo.Estado = "convertido";
        correo.IdUsuarioAccion = UsuarioActual;
        correo.FechaAccion = hoy;
        correo.ConsultaGenerada = consulta;

        await db.SaveChangesAsync();
        return Ok(new { consulta.IdConsulta, consulta.Codigo });
    }
}

public record ConvertirCorreoRequest(int? IdCliente, int? IdEmpresa);
public record ResponderCorreoRequest(string Cuerpo);
