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
[Route("api/resultados")]
[Authorize(Roles = "ROL-01,ROL-07,ROL-08,ROL-09")]
public class ResultadosController(SsalDbContext db) : ControllerBase
{
    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/resultados/muestras ─────────────────────────────────────
    // Lista de muestras con resultados, con resumen de avance de carga/validación.
    [HttpGet("muestras")]
    public async Task<IActionResult> ListarMuestras(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado, // pendientes | por_validar | completos
        [FromQuery] string? area,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20)
    {
        var q = db.Muestras
            .Include(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente!.Empresa)
            .Include(m => m.Resultados).ThenInclude(r => r.Analisis)
            .Where(m => m.Estado != "rechazada" && m.Resultados.Any())
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(area))
            q = q.Where(m => m.Resultados.Any(r => r.Analisis.Area == area));

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(m =>
                m.RotuloInterno.NumeroUnico.ToLower().Contains(b) ||
                (m.RotuloInterno.Sse.Codigo != null && m.RotuloInterno.Sse.Codigo.ToLower().Contains(b)) ||
                (m.RotuloInterno.Sse.Cliente != null && (m.RotuloInterno.Sse.Cliente.Nombre + " " + m.RotuloInterno.Sse.Cliente.Apellido).ToLower().Contains(b)) ||
                (m.RotuloInterno.Sse.Cliente != null && m.RotuloInterno.Sse.Cliente.Empresa != null && m.RotuloInterno.Sse.Cliente.Empresa.RazonSocial.ToLower().Contains(b)));
        }

        var todas = await q.ToListAsync();

        var proyectadas = todas.Select(m => new
        {
            m.IdMuestra,
            NumeroRotulo = m.RotuloInterno.NumeroUnico,
            CodigoSse = m.RotuloInterno.Sse.Codigo,
            Cliente = m.RotuloInterno.Sse.Cliente == null ? null : new { m.RotuloInterno.Sse.Cliente.Nombre, m.RotuloInterno.Sse.Cliente.Apellido },
            Empresa = m.RotuloInterno.Sse.Cliente == null || m.RotuloInterno.Sse.Cliente.Empresa == null ? null : new { m.RotuloInterno.Sse.Cliente.Empresa.RazonSocial },
            Areas = m.Resultados.Select(r => r.Analisis.Area).Distinct().OrderBy(a => a).ToList(),
            Total = m.Resultados.Count,
            Pendientes = m.Resultados.Count(r => r.Estado == "pendiente" || r.Estado == "rechazado"),
            PorValidar = m.Resultados.Count(r => r.Estado == "cargado"),
            Validados = m.Resultados.Count(r => r.Estado == "validado"),
            m.FechaRecepcion,
            EstadoGeneral = m.Resultados.All(r => r.Estado == "validado") ? "completo"
                : m.Resultados.Any(r => r.Estado == "cargado") ? "por_validar"
                : "pendiente"
        }).ToList();

        var filtradas = estado switch
        {
            "pendientes" => proyectadas.Where(p => p.EstadoGeneral == "pendiente"),
            "por_validar" => proyectadas.Where(p => p.EstadoGeneral == "por_validar"),
            "completos" => proyectadas.Where(p => p.EstadoGeneral == "completo"),
            _ => proyectadas.AsEnumerable()
        };

        var ordenadas = filtradas.OrderByDescending(p => p.FechaRecepcion).ToList();
        var total = ordenadas.Count;
        var items = ordenadas.Skip((pagina - 1) * porPagina).Take(porPagina).ToList();

        return Ok(new { total, pagina, porPagina, items });
    }

    // ─── GET /api/resultados/muestras/{id} ────────────────────────────────
    [HttpGet("muestras/{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var m = await db.Muestras
            .Include(x => x.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente!.Empresa)
            .Include(x => x.Resultados).ThenInclude(r => r.Analisis).ThenInclude(a => a.Grupo)
            .Include(x => x.Resultados).ThenInclude(r => r.CargadoPorUsuario)
            .Include(x => x.Resultados).ThenInclude(r => r.ValidadoPorUsuario)
            .Include(x => x.Informe)
            .FirstOrDefaultAsync(x => x.IdMuestra == id);

        if (m is null) return NotFound();

        return Ok(new
        {
            m.IdMuestra,
            m.Tipo,
            m.Estado,
            m.FechaRecepcion,
            NumeroRotulo = m.RotuloInterno.NumeroUnico,
            CodigoSse = m.RotuloInterno.Sse.Codigo,
            Cliente = m.RotuloInterno.Sse.Cliente == null ? null : new { m.RotuloInterno.Sse.Cliente.Nombre, m.RotuloInterno.Sse.Cliente.Apellido },
            Empresa = m.RotuloInterno.Sse.Cliente == null || m.RotuloInterno.Sse.Cliente.Empresa == null ? null : new { m.RotuloInterno.Sse.Cliente.Empresa.RazonSocial },
            Informe = m.Informe == null ? null : new { m.Informe.IdInforme, m.Informe.Codigo, m.Informe.FechaGeneracion },
            TodoValidado = m.Resultados.Count > 0 && m.Resultados.All(r => r.Estado == "validado"),
            Resultados = m.Resultados
                .OrderBy(r => r.Analisis.Area).ThenBy(r => r.Analisis.Nombre)
                .Select(r => new
                {
                    r.IdResultado,
                    r.Valor,
                    r.Estado,
                    r.FechaCarga,
                    r.MotivoRechazo,
                    CargadoPor = r.CargadoPorUsuario == null ? null : new { r.CargadoPorUsuario.Nombre, r.CargadoPorUsuario.Apellido },
                    ValidadoPor = r.ValidadoPorUsuario == null ? null : new { r.ValidadoPorUsuario.Nombre, r.ValidadoPorUsuario.Apellido },
                    r.FechaValidacion,
                    Analisis = new
                    {
                        r.Analisis.IdAnalisis,
                        r.Analisis.Codigo,
                        r.Analisis.Nombre,
                        r.Analisis.Area,
                        r.Analisis.Unidad,
                        r.Analisis.RangoMin,
                        r.Analisis.RangoMax,
                        r.Analisis.ValorEsperado,
                        GrupoNombre = r.Analisis.Grupo.CodigoAgrupador
                    },
                    FueraDeRango = EvaluarFueraDeRango(r.Valor, r.Analisis)
                })
        });
    }

    // ─── POST /api/resultados/muestras/{id}/informe ────────────────────────
    // Guarda (finaliza) el informe de resultados de una muestra. Requiere que
    // todos los análisis estén validados. Genera el código RPO 01-04.
    [HttpPost("muestras/{id:int}/informe")]
    public async Task<IActionResult> GuardarInforme(int id)
    {
        var m = await db.Muestras
            .Include(x => x.Resultados)
            .Include(x => x.Informe)
            .FirstOrDefaultAsync(x => x.IdMuestra == id);

        if (m is null) return NotFound();
        if (m.Informe is not null)
            return BadRequest(new { error = "Esta muestra ya tiene un informe guardado." });
        if (m.Resultados.Count == 0 || !m.Resultados.All(r => r.Estado == "validado"))
            return BadRequest(new { error = "Todos los análisis deben estar validados antes de guardar el informe." });

        var hoy = DateTime.UtcNow;
        var prefijo = $"INF-{hoy:yyyyMMdd}-";
        var ultimo = await db.Informes
            .Where(i => i.Codigo.StartsWith(prefijo))
            .OrderByDescending(i => i.Codigo)
            .Select(i => i.Codigo)
            .FirstOrDefaultAsync();

        int siguiente = 1;
        if (ultimo is not null)
        {
            var partes = ultimo.Split('-');
            if (partes.Length == 3 && int.TryParse(partes[2], out var n))
                siguiente = n + 1;
        }

        var informe = new Informe
        {
            IdMuestra = id,
            GeneradoPor = UsuarioActual,
            Codigo = $"{prefijo}{siguiente:D3}",
            FechaGeneracion = hoy
        };

        db.Informes.Add(informe);
        m.Estado = "completada";

        await db.SaveChangesAsync();
        return Ok(new { informe.IdInforme, informe.Codigo });
    }

    // ─── GET /api/resultados/informes/{id}/pdf ─────────────────────────────
    [HttpGet("informes/{id:int}/pdf")]
    public async Task<IActionResult> DescargarPdf(int id)
    {
        var informe = await db.Informes
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente!.Empresa)
            .Include(i => i.Muestra).ThenInclude(m => m.Resultados).ThenInclude(r => r.Analisis).ThenInclude(a => a.Grupo)
            .Include(i => i.GeneradoPorUsuario)
            .FirstOrDefaultAsync(i => i.IdInforme == id);

        if (informe is null) return NotFound();

        var pdfBytes = InformePdfBuilder.Generar(informe);
        return File(pdfBytes, "application/pdf", $"{informe.Codigo}.pdf");
    }

    // ─── PUT /api/resultados/{id} ──────────────────────────────────────────
    // Cargar o corregir el valor de un resultado (Analista / Resp. Área).
    [HttpPut("{id:int}")]
    public async Task<IActionResult> CargarValor(int id, [FromBody] CargarValorRequest req)
    {
        var r = await db.Resultados.Include(x => x.Analisis).FirstOrDefaultAsync(x => x.IdResultado == id);
        if (r is null) return NotFound();

        if (string.IsNullOrWhiteSpace(req.Valor))
            return BadRequest(new { error = "El valor no puede estar vacío." });

        r.Valor = req.Valor.Trim();
        r.Estado = "cargado";
        r.CargadoPor = UsuarioActual;
        r.FechaCarga = DateTime.UtcNow;
        // Al recargar un resultado (ej. tras rechazo), se limpia la validación anterior.
        r.ValidadoPor = null;
        r.FechaValidacion = null;
        r.MotivoRechazo = null;

        await db.SaveChangesAsync();

        return Ok(new { r.IdResultado, r.Estado, fueraDeRango = EvaluarFueraDeRango(r.Valor, r.Analisis) });
    }

    // ─── PUT /api/resultados/{id}/validar ──────────────────────────────────
    // Responsable de Área valida o rechaza un resultado ya cargado.
    [HttpPut("{id:int}/validar")]
    public async Task<IActionResult> Validar(int id, [FromBody] ValidarResultadoRequest req)
    {
        var r = await db.Resultados.FirstOrDefaultAsync(x => x.IdResultado == id);
        if (r is null) return NotFound();
        if (r.Estado != "cargado")
            return BadRequest(new { error = "Solo se pueden validar resultados que ya fueron cargados." });

        if (req.Aprobado)
        {
            r.Estado = "validado";
            r.MotivoRechazo = null;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(req.MotivoRechazo))
                return BadRequest(new { error = "Indicá el motivo del rechazo." });
            r.Estado = "rechazado";
            r.MotivoRechazo = req.MotivoRechazo.Trim();
        }
        r.ValidadoPor = UsuarioActual;
        r.FechaValidacion = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Compara el valor cargado contra el rango/valor esperado del análisis. Null si no hay referencia definida.</summary>
    private static bool? EvaluarFueraDeRango(string? valor, Analisis analisis)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;

        if (analisis.RangoMin.HasValue || analisis.RangoMax.HasValue)
        {
            if (!decimal.TryParse(valor, out var numero)) return null;
            if (analisis.RangoMin.HasValue && numero < analisis.RangoMin.Value) return true;
            if (analisis.RangoMax.HasValue && numero > analisis.RangoMax.Value) return true;
            return false;
        }

        if (!string.IsNullOrWhiteSpace(analisis.ValorEsperado))
            return !string.Equals(valor.Trim(), analisis.ValorEsperado.Trim(), StringComparison.OrdinalIgnoreCase);

        return null;
    }
}

public record CargarValorRequest(string Valor);

public record ValidarResultadoRequest(bool Aprobado, string? MotivoRechazo);
