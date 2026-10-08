using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;
using Ssal.Api.Services;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/firmas")]
[Authorize]
public class FirmasController(SsalDbContext db) : ControllerBase
{
    // Lectura del módulo y rechazo: responsables de área y Administrador
    private const string RolesFirma = "ROL-01,ROL-08,ROL-09";
    // Firma del área: solo el responsable de MIC o de FQ
    private const string RolesResponsable = "ROL-08,ROL-09";
    // Aprobación final: Administrador general
    private const string RolesAprobacion = "ROL-01";
    // Reenvío tras rechazo: quien puede crear el documento (se valida por tipo más abajo)
    private const string RolesReenvio = "ROL-01,ROL-03,ROL-07,ROL-08,ROL-09";

    private static readonly string[] Tipos = ["informe", "presupuesto"];

    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    string RolActual => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    // ─── GET /api/firmas/documentos ───────────────────────────────────────
    [HttpGet("documentos")]
    [Authorize(Roles = RolesFirma)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? tipo,
        [FromQuery] string? estado,
        [FromQuery] string? area)
    {
        var docs = new List<DocInfo>();

        if (string.IsNullOrWhiteSpace(tipo) || tipo == "informe")
        {
            var informes = await db.Informes
                .Include(i => i.Muestra).ThenInclude(m => m.Resultados).ThenInclude(r => r.Analisis)
                .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente)
                .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
                .OrderByDescending(i => i.FechaGeneracion)
                .Take(200)
                .ToListAsync();
            docs.AddRange(informes.Select(MapInforme));
        }

        if (string.IsNullOrWhiteSpace(tipo) || tipo == "presupuesto")
        {
            var presupuestos = await db.Presupuestos
                .Include(p => p.Items).ThenInclude(i => i.Analisis)
                .Include(p => p.Consulta).ThenInclude(c => c.Cliente)
                .Include(p => p.Consulta).ThenInclude(c => c.Empresa)
                .OrderByDescending(p => p.Fecha)
                .Take(200)
                .ToListAsync();
            docs.AddRange(presupuestos.Select(MapPresupuesto));
        }

        var firmas = await db.FirmasDocumento.Include(f => f.Firmante).ToListAsync();
        var items = docs.Select(d =>
        {
            var eventos = firmas.Where(f => f.TipoDocumento == d.Tipo && f.IdDocumento == d.Id);
            var ciclo = CicloFirma.Calcular(eventos, d.Areas);
            return new
            {
                d.Tipo,
                d.Id,
                d.Codigo,
                d.Destinatario,
                d.Sse,
                d.Rotulo,
                d.Fecha,
                d.Areas,
                Estado = ciclo.Estado,
                AreasFirmadas = ciclo.AreasFirmadas.OrderBy(a => a).ToList()
            };
        })
        .Where(x => string.IsNullOrWhiteSpace(estado) || x.Estado == estado)
        .Where(x => string.IsNullOrWhiteSpace(area) || x.Areas.Contains(area))
        .OrderByDescending(x => x.Fecha)
        .ToList();

        return Ok(items);
    }

    // ─── GET /api/firmas/documentos/{tipo}/{id} ───────────────────────────
    [HttpGet("documentos/{tipo}/{id:int}")]
    [Authorize(Roles = RolesFirma)]
    public async Task<IActionResult> Detalle(string tipo, int id)
    {
        var estado = await CargarEstado(tipo, id);
        if (estado is null) return NotFound();
        var (doc, eventos, ciclo) = estado.Value;

        return Ok(new
        {
            documento = doc,
            estado = ciclo.Estado,
            areasFirmadas = ciclo.AreasFirmadas.OrderBy(a => a).ToList(),
            permisos = CalcularPermisos(doc, ciclo),
            historial = eventos.Select(e => new
            {
                e.IdFirma,
                e.TipoFirma,
                e.Area,
                e.Estado,
                e.MotivoRechazo,
                e.FechaFirma,
                Firmante = $"{e.Firmante.Nombre} {e.Firmante.Apellido}"
            })
        });
    }

    // ─── POST /api/firmas/documentos/{tipo}/{id}/firmar ───────────────────
    // Firma del responsable del área (primer paso, RF-32)
    [HttpPost("documentos/{tipo}/{id:int}/firmar")]
    [Authorize(Roles = RolesResponsable)]
    public async Task<IActionResult> Firmar(string tipo, int id, [FromBody] ConfirmarPasswordRequest req)
    {
        var estado = await CargarEstado(tipo, id);
        if (estado is null) return NotFound();
        var (doc, _, ciclo) = estado.Value;

        var area = AreaDelRol(RolActual);
        if (!CalcularPermisos(doc, ciclo).Firmar || area is null)
            return BadRequest(new { error = $"El documento no está pendiente de firma de {area ?? "tu área"}." });

        if (!await PasswordValida(req.Password))
            return BadRequest(new { error = "Contraseña incorrecta." });

        db.FirmasDocumento.Add(new FirmaDocumento
        {
            IdFirmante = UsuarioActual,
            TipoDocumento = tipo,
            IdDocumento = id,
            TipoFirma = "responsable",
            Area = area,
            Estado = "firmado",
            FechaFirma = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── POST /api/firmas/documentos/{tipo}/{id}/aprobar ──────────────────
    // Aprobación final del Administrador general (RF-33)
    [HttpPost("documentos/{tipo}/{id:int}/aprobar")]
    [Authorize(Roles = RolesAprobacion)]
    public async Task<IActionResult> Aprobar(string tipo, int id, [FromBody] ConfirmarPasswordRequest req)
    {
        var estado = await CargarEstado(tipo, id);
        if (estado is null) return NotFound();
        var (doc, _, ciclo) = estado.Value;

        if (!CalcularPermisos(doc, ciclo).Aprobar)
            return BadRequest(new { error = "El documento todavía no tiene todas las firmas de los responsables de área." });

        if (!await PasswordValida(req.Password))
            return BadRequest(new { error = "Contraseña incorrecta." });

        db.FirmasDocumento.Add(new FirmaDocumento
        {
            IdFirmante = UsuarioActual,
            TipoDocumento = tipo,
            IdDocumento = id,
            TipoFirma = "aprobacion",
            Estado = "aprobado",
            FechaFirma = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── POST /api/firmas/documentos/{tipo}/{id}/rechazar ─────────────────
    // Rechazo con motivo (RF-38 y E2). Limpia las firmas del ciclo.
    [HttpPost("documentos/{tipo}/{id:int}/rechazar")]
    [Authorize(Roles = RolesFirma)]
    public async Task<IActionResult> Rechazar(string tipo, int id, [FromBody] RechazarRequest req)
    {
        var estado = await CargarEstado(tipo, id);
        if (estado is null) return NotFound();
        var (doc, _, ciclo) = estado.Value;

        if (!CalcularPermisos(doc, ciclo).Rechazar)
            return BadRequest(new { error = "No podés rechazar este documento en su estado actual." });

        var motivo = req.Motivo?.Trim();
        if (string.IsNullOrWhiteSpace(motivo))
            return BadRequest(new { error = "Indicá el motivo del rechazo." });

        db.FirmasDocumento.Add(new FirmaDocumento
        {
            IdFirmante = UsuarioActual,
            TipoDocumento = tipo,
            IdDocumento = id,
            TipoFirma = "rechazo",
            Estado = "rechazado",
            MotivoRechazo = motivo,
            FechaFirma = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── POST /api/firmas/documentos/{tipo}/{id}/reenviar ─────────────────
    // El autor reenvía el documento corregido: vuelve a empezar el ciclo de firma
    [HttpPost("documentos/{tipo}/{id:int}/reenviar")]
    [Authorize(Roles = RolesReenvio)]
    public async Task<IActionResult> Reenviar(string tipo, int id)
    {
        var estado = await CargarEstado(tipo, id);
        if (estado is null) return NotFound();
        var (doc, _, ciclo) = estado.Value;

        if (!CalcularPermisos(doc, ciclo).Reenviar)
            return BadRequest(new { error = "El documento no está rechazado o tu rol no puede reenviarlo." });

        db.FirmasDocumento.Add(new FirmaDocumento
        {
            IdFirmante = UsuarioActual,
            TipoDocumento = tipo,
            IdDocumento = id,
            TipoFirma = "reenvio",
            Estado = "reenviado",
            FechaFirma = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private async Task<(DocInfo doc, List<FirmaDocumento> eventos, CicloFirma.Resultado ciclo)?> CargarEstado(string tipo, int id)
    {
        if (!Tipos.Contains(tipo)) return null;

        DocInfo? doc = tipo == "informe" ? await CargarInforme(id) : await CargarPresupuesto(id);
        if (doc is null) return null;

        var eventos = await db.FirmasDocumento
            .Include(f => f.Firmante)
            .Where(f => f.TipoDocumento == tipo && f.IdDocumento == id)
            .OrderBy(f => f.IdFirma)
            .ToListAsync();

        return (doc, eventos, CicloFirma.Calcular(eventos, doc.Areas));
    }

    private async Task<DocInfo?> CargarInforme(int id)
    {
        var informe = await db.Informes
            .Include(i => i.Muestra).ThenInclude(m => m.Resultados).ThenInclude(r => r.Analisis)
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente)
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .FirstOrDefaultAsync(i => i.IdInforme == id);
        return informe is null ? null : MapInforme(informe);
    }

    private async Task<DocInfo?> CargarPresupuesto(int id)
    {
        var p = await db.Presupuestos
            .Include(p => p.Items).ThenInclude(i => i.Analisis)
            .Include(p => p.Consulta).ThenInclude(c => c.Cliente)
            .Include(p => p.Consulta).ThenInclude(c => c.Empresa)
            .FirstOrDefaultAsync(p => p.IdPresupuesto == id);
        return p is null ? null : MapPresupuesto(p);
    }

    private static DocInfo MapInforme(Informe i)
    {
        var sse = i.Muestra.RotuloInterno.Sse;
        return new DocInfo(
            "informe",
            i.IdInforme,
            i.Codigo,
            DestinatarioSse(sse),
            sse.Codigo,
            i.Muestra.RotuloInterno.NumeroUnico,
            i.FechaGeneracion,
            i.Muestra.Resultados.Select(r => r.Analisis.Area).Distinct().OrderBy(a => a).ToList());
    }

    private static DocInfo MapPresupuesto(Presupuesto p)
    {
        var cliente = p.Consulta.Cliente;
        var destinatario = cliente is not null
            ? $"{cliente.Nombre} {cliente.Apellido}"
            : p.Consulta.Empresa?.RazonSocial ?? "—";
        return new DocInfo(
            "presupuesto",
            p.IdPresupuesto,
            p.CodigoRpo ?? $"PRES-{p.IdPresupuesto}",
            destinatario,
            null,
            null,
            p.Fecha,
            p.Items.Select(i => i.Analisis.Area).Distinct().OrderBy(a => a).ToList());
    }

    private static string DestinatarioSse(Sse s) =>
        s.Cliente is not null ? $"{s.Cliente.Nombre} {s.Cliente.Apellido}"
        : s.Presupuesto?.Consulta.Empresa?.RazonSocial ?? "—";

    private static string? AreaDelRol(string rol) => rol switch
    {
        "ROL-08" => "MIC",
        "ROL-09" => "FQ",
        _ => null
    };

    private Permisos CalcularPermisos(DocInfo doc, CicloFirma.Resultado ciclo)
    {
        var rol = RolActual;
        var area = AreaDelRol(rol);
        var esResponsable = area is not null;
        var esAdmin = rol == "ROL-01";

        var puedeFirmar = esResponsable
            && ciclo.Estado == CicloFirma.PendienteFirma
            && doc.Areas.Contains(area!)
            && !ciclo.AreasFirmadas.Contains(area!);

        var puedeAprobar = esAdmin && ciclo.Estado == CicloFirma.Firmado;

        var puedeRechazar = (esAdmin && ciclo.Estado == CicloFirma.Firmado)
            || (esResponsable && ciclo.Estado == CicloFirma.PendienteFirma && doc.Areas.Contains(area!));

        var rolesReenvio = doc.Tipo == "informe"
            ? new[] { "ROL-01", "ROL-07", "ROL-08", "ROL-09" }
            : new[] { "ROL-01", "ROL-03" };
        var puedeReenviar = ciclo.Estado == CicloFirma.Rechazado && rolesReenvio.Contains(rol);

        return new Permisos(puedeFirmar, puedeAprobar, puedeRechazar, puedeReenviar);
    }

    private async Task<bool> PasswordValida(string? password)
    {
        if (string.IsNullOrEmpty(password)) return false;
        var usuario = await db.Usuarios.FindAsync(UsuarioActual);
        return usuario is not null && BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash);
    }
}

public record DocInfo(
    string Tipo,
    int Id,
    string Codigo,
    string Destinatario,
    string? Sse,
    string? Rotulo,
    DateTime Fecha,
    List<string> Areas);

public record Permisos(bool Firmar, bool Aprobar, bool Rechazar, bool Reenviar);

public record ConfirmarPasswordRequest(string? Password);

public record RechazarRequest(string? Motivo);
