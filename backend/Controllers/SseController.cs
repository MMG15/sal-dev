using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Sse;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/sses")]
[Authorize(Roles = "ROL-01,ROL-02,ROL-07")]
public class SseController(
    ListarSse listar,
    ObtenerSse obtener,
    CrearSse crear,
    AsignarClienteSse asignarCliente,
    DescargarSsePdf descargarPdf,
    CambiarEstadoSse cambiarEstado,
    AsignarRotulo asignarRotulo,
    RegistrarMuestra registrarMuestra,
    ActualizarRotulo actualizarRotulo) : ControllerBase
{
    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/sses ────────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] string? area,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20) =>
        Ok(await listar.EjecutarAsync(busqueda, estado, area, pagina, porPagina));

    // ─── GET /api/sses/{id} ───────────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var sse = await obtener.EjecutarAsync(id);
        if (sse is null) return NotFound();
        return Ok(sse);
    }

    // ─── POST /api/sses ───────────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearSseCommand req)
    {
        var resultado = await crear.EjecutarAsync(req, UsuarioActual);
        return CreatedAtAction(nameof(Detalle), new { id = resultado.IdSse }, resultado);
    }

    // ─── PUT /api/sses/{id}/cliente ───────────────────────────────────────
    // La SSE es un documento que le queda tanto al laboratorio como al cliente: hace falta
    // poder asignarle (o corregirle) el cliente aunque ya esté creada.
    [HttpPut("{id}/cliente")]
    public async Task<IActionResult> AsignarCliente(int id, [FromBody] AsignarClienteSseCommand req)
    {
        var resultado = await asignarCliente.EjecutarAsync(id, req);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── GET /api/sses/{id}/pdf ───────────────────────────────────────────
    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DescargarPdf(int id)
    {
        var pdf = await descargarPdf.EjecutarAsync(id);
        if (pdf is null) return NotFound();
        return File(pdf.Contenido, "application/pdf", pdf.NombreArchivo);
    }

    // ─── PUT /api/sses/{id}/estado ────────────────────────────────────────
    [HttpPut("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoSseCommand req)
    {
        var ok = await cambiarEstado.EjecutarAsync(id, req);
        if (!ok) return NotFound();
        return NoContent();
    }

    // ─── POST /api/sses/{id}/rotulo ───────────────────────────────────────
    [HttpPost("{id}/rotulo")]
    public async Task<IActionResult> AsignarRotulo(int id, [FromBody] AsignarRotuloCommand req)
    {
        var resultado = await asignarRotulo.EjecutarAsync(id, req, UsuarioActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── POST /api/sses/{id}/muestra ─────────────────────────────────────
    [HttpPost("{id}/muestra")]
    public async Task<IActionResult> RegistrarMuestra(int id, [FromBody] RegistrarMuestraCommand req)
    {
        var resultado = await registrarMuestra.EjecutarAsync(id, req, UsuarioActual);
        if (!resultado.Ok) return BadRequest(new { error = resultado.Error });
        return Ok(new { IdMuestra = resultado.Valor });
    }

    // ─── PUT /api/sses/{id}/rotulo ────────────────────────────────────────
    // Corrige número/descripción/estado del rótulo. Operación crítica (RF-13/RF-14):
    // exige motivo y audita cada campo modificado por separado.
    [HttpPut("{id}/rotulo")]
    public async Task<IActionResult> ActualizarRotulo(int id, [FromBody] ActualizarRotuloCommand req)
    {
        var resultado = await actualizarRotulo.EjecutarAsync(id, req, UsuarioActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }
}
