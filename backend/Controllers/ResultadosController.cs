using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Resultados;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/resultados")]
[Authorize(Roles = "ROL-01,ROL-07,ROL-08,ROL-09")]
public class ResultadosController(
    ListarMuestras listar,
    ObtenerMuestra obtener,
    GuardarInforme guardarInforme,
    DescargarInformePdf descargarPdf,
    CargarValorResultado cargarValor,
    ValidarResultado validar) : ControllerBase
{
    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/resultados/muestras ─────────────────────────────────────
    [HttpGet("muestras")]
    public async Task<IActionResult> ListarMuestras(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] string? area,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20) =>
        Ok(await listar.EjecutarAsync(busqueda, estado, area, pagina, porPagina));

    // ─── GET /api/resultados/muestras/{id} ────────────────────────────────
    [HttpGet("muestras/{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var m = await obtener.EjecutarAsync(id);
        if (m is null) return NotFound();
        return Ok(m);
    }

    // ─── POST /api/resultados/muestras/{id}/informe ────────────────────────
    [HttpPost("muestras/{id:int}/informe")]
    public async Task<IActionResult> GuardarInforme(int id)
    {
        var resultado = await guardarInforme.EjecutarAsync(id, UsuarioActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── GET /api/resultados/informes/{id}/pdf ─────────────────────────────
    [HttpGet("informes/{id:int}/pdf")]
    public async Task<IActionResult> DescargarPdf(int id)
    {
        var pdf = await descargarPdf.EjecutarAsync(id);
        if (pdf is null) return NotFound();
        return File(pdf.Contenido, "application/pdf", pdf.NombreArchivo);
    }

    // ─── PUT /api/resultados/{id} ──────────────────────────────────────────
    // Cargar o corregir el valor de un resultado (Analista / Resp. Área).
    [HttpPut("{id:int}")]
    public async Task<IActionResult> CargarValor(int id, [FromBody] CargarValorCommand req)
    {
        var resultado = await cargarValor.EjecutarAsync(id, req, UsuarioActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── PUT /api/resultados/{id}/validar ──────────────────────────────────
    // Responsable de Área valida o rechaza un resultado ya cargado.
    [HttpPut("{id:int}/validar")]
    public async Task<IActionResult> Validar(int id, [FromBody] ValidarResultadoCommand req)
    {
        var resultado = await validar.EjecutarAsync(id, req, UsuarioActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }
}
