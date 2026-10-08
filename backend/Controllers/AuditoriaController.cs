using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;

namespace Ssal.Api.Controllers;

// Registro de accesos y acciones críticas (RF-30). Lo consultan el Administrador general y el Administrador de sistema.
[ApiController]
[Route("api/auditoria")]
[Authorize(Roles = "ROL-01,ROL-06")]
public class AuditoriaController(SsalDbContext db) : ControllerBase
{
    // ─── GET /api/auditoria ───────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? accion,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 50)
    {
        var q = db.Auditoria.Include(a => a.Usuario).AsQueryable();

        if (!string.IsNullOrWhiteSpace(accion))
            q = q.Where(a => a.Accion == accion);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(a =>
                (a.Objetivo != null && a.Objetivo.ToLower().Contains(b)) ||
                (a.Usuario != null && (a.Usuario.Nombre + " " + a.Usuario.Apellido).ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(a => a.Fecha)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(a => new
            {
                a.Id,
                a.Fecha,
                Actor = a.Usuario == null ? null : a.Usuario.Nombre + " " + a.Usuario.Apellido,
                a.Accion,
                a.Objetivo,
                a.Detalle
            })
            .ToListAsync();

        return Ok(new { total, pagina, porPagina, items });
    }
}
