using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/facturas")]
[Authorize]
public class FacturasController(SsalDbContext db) : ControllerBase
{
    // Emisión de la factura en TANGO: Responsable de Área (paso 10 del flujo PO-01) y Administrador.
    private const string RolesEmision = "ROL-01,ROL-08,ROL-09";
    // Consulta de facturas y registro de cobro: además Cobranzas (RF-26).
    private const string RolesFacturacion = "ROL-01,ROL-04,ROL-08,ROL-09";

    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/facturas ────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = RolesFacturacion)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado, // emitida | pagada
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 50)
    {
        var q = db.Facturas
            .Include(f => f.Sse).ThenInclude(s => s.Cliente)
            .Include(f => f.Sse).ThenInclude(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .Include(f => f.Sse).ThenInclude(s => s.RotuloInterno)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(f => f.Estado == estado);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(f =>
                (f.Numero != null && f.Numero.ToLower().Contains(b)) ||
                (f.Sse.Codigo != null && f.Sse.Codigo.ToLower().Contains(b)) ||
                (f.Sse.Cliente != null && (f.Sse.Cliente.Nombre + " " + f.Sse.Cliente.Apellido).ToLower().Contains(b)) ||
                (f.Sse.RotuloInterno != null && f.Sse.RotuloInterno.NumeroUnico.ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderBy(f => f.Sse.Cliente!.Apellido)
            .ThenBy(f => f.Sse.Cliente!.Nombre)
            .ThenByDescending(f => f.Fecha)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(f => new
            {
                f.IdFactura,
                f.Numero,
                f.Estado,
                f.Fecha,
                f.FechaPago,
                f.ImportePesos,
                f.ImporteDolares,
                Sse = new { f.Sse.IdSse, f.Sse.Codigo, f.Sse.Area },
                Cliente = f.Sse.Cliente == null ? null : new { f.Sse.Cliente.Nombre, f.Sse.Cliente.Apellido },
                Empresa = f.Sse.Presupuesto == null ? null
                    : f.Sse.Presupuesto.Consulta.Empresa == null ? null
                    : new { f.Sse.Presupuesto.Consulta.Empresa.RazonSocial },
                Rotulo = f.Sse.RotuloInterno == null ? null : f.Sse.RotuloInterno.NumeroUnico
            })
            .ToListAsync();

        return Ok(new { total, pagina, porPagina, items });
    }

    // ─── GET /api/facturas/sses-disponibles ───────────────────────────────
    // SSE que todavía no tienen una factura activa, para elegir cuál facturar.
    [HttpGet("sses-disponibles")]
    [Authorize(Roles = RolesEmision)]
    public async Task<IActionResult> SsesDisponibles()
    {
        var items = await db.Sses
            .Where(s => !db.Facturas.Any(f => f.IdSse == s.IdSse && f.Estado != "anulada"))
            .Include(s => s.Cliente)
            .Include(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .Include(s => s.RotuloInterno)
            .OrderByDescending(s => s.Fecha)
            .Select(s => new
            {
                s.IdSse,
                s.Codigo,
                s.Area,
                s.Fecha,
                s.ImportePesos,
                s.ImporteDolares,
                Cliente = s.Cliente == null ? null : new { s.Cliente.Nombre, s.Cliente.Apellido },
                Empresa = s.Presupuesto == null ? null
                    : s.Presupuesto.Consulta.Empresa == null ? null
                    : new { s.Presupuesto.Consulta.Empresa.RazonSocial },
                Rotulo = s.RotuloInterno == null ? null : s.RotuloInterno.NumeroUnico
            })
            .ToListAsync();

        return Ok(items);
    }

    // ─── POST /api/facturas ───────────────────────────────────────────────
    // Registra la factura emitida en TANGO. Los importes se toman de la SSE (RF-24).
    [HttpPost]
    [Authorize(Roles = RolesEmision)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarFacturaRequest req)
    {
        var numero = req.Numero?.Trim();
        if (string.IsNullOrWhiteSpace(numero))
            return BadRequest(new { error = "El número de factura es obligatorio." });
        if (numero.Length > 30)
            return BadRequest(new { error = "El número de factura no puede superar los 30 caracteres." });

        var sse = await db.Sses.FindAsync(req.IdSse);
        if (sse is null) return BadRequest(new { error = "SSE no encontrada." });

        if (await db.Facturas.AnyAsync(f => f.IdSse == sse.IdSse && f.Estado != "anulada"))
            return Conflict(new { error = $"La SSE {sse.Codigo} ya tiene una factura registrada." });

        if (await db.Facturas.AnyAsync(f => f.Numero == numero))
            return Conflict(new { error = $"Ya existe una factura con el número {numero}." });

        var factura = new Factura
        {
            IdSse = sse.IdSse,
            CreadoPor = UsuarioActual,
            Numero = numero,
            ImportePesos = sse.ImportePesos,
            ImporteDolares = sse.ImporteDolares,
            Estado = "emitida",
            Fecha = req.Fecha?.ToUniversalTime() ?? DateTime.UtcNow
        };
        db.Facturas.Add(factura);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(Listar), new { id = factura.IdFactura }, new { factura.IdFactura, factura.Numero });
    }

    // ─── PUT /api/facturas/{id}/pago ──────────────────────────────────────
    // Marca la factura como pagada. El registro de pagos detallado queda para una etapa posterior.
    [HttpPut("{id:int}/pago")]
    [Authorize(Roles = RolesFacturacion)]
    public async Task<IActionResult> MarcarPagada(int id, [FromBody] MarcarPagadaRequest req)
    {
        var factura = await db.Facturas.FindAsync(id);
        if (factura is null) return NotFound();
        if (factura.Estado != "emitida")
            return BadRequest(new { error = "Solo se pueden marcar como pagadas las facturas emitidas." });

        factura.Estado = "pagada";
        factura.FechaPago = req.FechaPago?.ToUniversalTime() ?? DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record RegistrarFacturaRequest(int IdSse, string Numero, DateTime? Fecha);

public record MarcarPagadaRequest(DateTime? FechaPago);
