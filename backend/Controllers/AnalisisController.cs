using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/analisis")]
[Authorize(Roles = "ROL-01,ROL-02,ROL-03,ROL-07,ROL-08,ROL-09")]
public class AnalisisController(SsalDbContext db) : ControllerBase
{
    // ─── GET /api/analisis ────────────────────────────────────────────────
    // Para el selector de presupuesto: solo activos, agrupados
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var grupos = await db.GruposAnalisis
            .Include(g => g.Analisis.Where(a => a.Activo))
            .OrderBy(g => g.Area)
            .ThenBy(g => g.CodigoAgrupador)
            .Select(g => new
            {
                g.IdGrupo,
                g.CodigoAgrupador,
                g.Descripcion,
                g.Area,
                Items = g.Analisis
                    .Where(a => a.Activo)
                    .OrderBy(a => a.Codigo)
                    .Select(a => new { a.IdAnalisis, a.Codigo, a.Nombre, a.Area, a.PrecioUsd })
            })
            .Where(g => g.Items.Any())
            .ToListAsync();

        return Ok(grupos);
    }

    // ─── GET /api/analisis/todos ──────────────────────────────────────────
    // Para gestión: todos incluyendo inactivos
    [HttpGet("todos")]
    public async Task<IActionResult> Todos()
    {
        var items = await db.Analisis
            .Include(a => a.Grupo)
            .OrderBy(a => a.Area)
            .ThenBy(a => a.Grupo.CodigoAgrupador)
            .ThenBy(a => a.Codigo)
            .Select(a => new
            {
                a.IdAnalisis,
                a.Codigo,
                a.Nombre,
                a.Area,
                a.PrecioUsd,
                a.Unidad,
                a.RangoMin,
                a.RangoMax,
                a.ValorEsperado,
                a.Activo,
                Grupo = new { a.Grupo.IdGrupo, a.Grupo.CodigoAgrupador, a.Grupo.Area }
            })
            .ToListAsync();

        return Ok(items);
    }

    // ─── GET /api/analisis/grupos ─────────────────────────────────────────
    [HttpGet("grupos")]
    public async Task<IActionResult> Grupos()
    {
        var grupos = await db.GruposAnalisis
            .OrderBy(g => g.Area)
            .ThenBy(g => g.CodigoAgrupador)
            .Select(g => new { g.IdGrupo, g.CodigoAgrupador, g.Descripcion, g.Area })
            .ToListAsync();

        return Ok(grupos);
    }

    // ─── POST /api/analisis ───────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] GuardarAnalisisRequest req)
    {
        // Si se envía nombre de grupo nuevo, crearlo
        int idGrupo = req.IdGrupo;
        if (idGrupo == 0 && !string.IsNullOrWhiteSpace(req.NuevoGrupo))
        {
            var grupo = new GrupoAnalisis
            {
                CodigoAgrupador = req.NuevoGrupo.Trim(),
                Area = req.Area,
                Descripcion = req.NuevoGrupo.Trim()
            };
            db.GruposAnalisis.Add(grupo);
            await db.SaveChangesAsync();
            idGrupo = grupo.IdGrupo;
        }

        if (await db.Analisis.AnyAsync(a => a.Codigo == req.Codigo))
            return BadRequest(new { error = $"Ya existe un análisis con el código '{req.Codigo}'." });

        var analisis = new Analisis
        {
            IdGrupo = idGrupo,
            Codigo = req.Codigo.Trim(),
            Nombre = req.Nombre.Trim(),
            Area = req.Area,
            PrecioUsd = req.PrecioUsd,
            Unidad = req.Unidad,
            RangoMin = req.RangoMin,
            RangoMax = req.RangoMax,
            ValorEsperado = req.ValorEsperado,
            Activo = true
        };

        db.Analisis.Add(analisis);
        await db.SaveChangesAsync();
        return Ok(new { analisis.IdAnalisis });
    }

    // ─── PUT /api/analisis/{id} ───────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarAnalisisRequest req)
    {
        var analisis = await db.Analisis.FindAsync(id);
        if (analisis is null) return NotFound();

        // Código único (si cambió)
        if (req.Codigo != analisis.Codigo &&
            await db.Analisis.AnyAsync(a => a.Codigo == req.Codigo && a.IdAnalisis != id))
            return BadRequest(new { error = $"Ya existe un análisis con el código '{req.Codigo}'." });

        // Grupo nuevo si aplica
        int idGrupo = req.IdGrupo;
        if (idGrupo == 0 && !string.IsNullOrWhiteSpace(req.NuevoGrupo))
        {
            var grupo = new GrupoAnalisis
            {
                CodigoAgrupador = req.NuevoGrupo.Trim(),
                Area = req.Area,
                Descripcion = req.NuevoGrupo.Trim()
            };
            db.GruposAnalisis.Add(grupo);
            await db.SaveChangesAsync();
            idGrupo = grupo.IdGrupo;
        }

        analisis.Codigo = req.Codigo.Trim();
        analisis.Nombre = req.Nombre.Trim();
        analisis.Area = req.Area;
        analisis.PrecioUsd = req.PrecioUsd;
        analisis.Unidad = req.Unidad;
        analisis.RangoMin = req.RangoMin;
        analisis.RangoMax = req.RangoMax;
        analisis.ValorEsperado = req.ValorEsperado;
        analisis.IdGrupo = idGrupo;

        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── PATCH /api/analisis/{id}/activo ─────────────────────────────────
    [HttpPatch("{id}/activo")]
    public async Task<IActionResult> ToggleActivo(int id)
    {
        var analisis = await db.Analisis.FindAsync(id);
        if (analisis is null) return NotFound();
        analisis.Activo = !analisis.Activo;
        await db.SaveChangesAsync();
        return Ok(new { analisis.IdAnalisis, analisis.Activo });
    }

    // ─── DELETE /api/analisis/{id} ────────────────────────────────────────
    // Solo elimina si no tiene presupuestos asociados
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var analisis = await db.Analisis.FindAsync(id);
        if (analisis is null) return NotFound();

        var tieneUso = await db.PresupuestosAnalisis.AnyAsync(p => p.IdAnalisis == id);
        if (tieneUso)
            return BadRequest(new { error = "No se puede eliminar: el análisis está incluido en uno o más presupuestos. Podés desactivarlo en su lugar." });

        db.Analisis.Remove(analisis);
        await db.SaveChangesAsync();
        return NoContent();
    }
}

public record GuardarAnalisisRequest(
    int IdGrupo,
    string? NuevoGrupo,
    string Codigo,
    string Nombre,
    string Area,
    decimal PrecioUsd,
    string? Unidad,
    decimal? RangoMin,
    decimal? RangoMax,
    string? ValorEsperado);
