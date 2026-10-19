using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Analisis;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/analisis")]
[Authorize(Roles = "ROL-01,ROL-02,ROL-03,ROL-07,ROL-08,ROL-09")]
public class AnalisisController(
    ListarAnalisisActivos listarActivos,
    ListarTodosLosAnalisis listarTodos,
    ListarGruposDeAnalisis listarGrupos,
    CrearAnalisis crear,
    ActualizarAnalisis actualizar,
    ToggleActivoAnalisis toggleActivo,
    EliminarAnalisis eliminar) : ControllerBase
{
    // ─── GET /api/analisis ────────────────────────────────────────────────
    // Para el selector de presupuesto: solo activos, agrupados
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await listarActivos.EjecutarAsync());

    // ─── GET /api/analisis/todos ──────────────────────────────────────────
    // Para gestión: todos incluyendo inactivos
    [HttpGet("todos")]
    public async Task<IActionResult> Todos() => Ok(await listarTodos.EjecutarAsync());

    // ─── GET /api/analisis/grupos ─────────────────────────────────────────
    [HttpGet("grupos")]
    public async Task<IActionResult> Grupos() => Ok(await listarGrupos.EjecutarAsync());

    // ─── POST /api/analisis ───────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] GuardarAnalisisCommand req)
    {
        var resultado = await crear.EjecutarAsync(req);
        if (!resultado.Ok) return BadRequest(new { error = resultado.Error });
        return Ok(new { IdAnalisis = resultado.Valor });
    }

    // ─── PUT /api/analisis/{id} ───────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarAnalisisCommand req)
    {
        var resultado = await actualizar.EjecutarAsync(id, req);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── PATCH /api/analisis/{id}/activo ─────────────────────────────────
    [HttpPatch("{id}/activo")]
    public async Task<IActionResult> ToggleActivo(int id)
    {
        var resultado = await toggleActivo.EjecutarAsync(id);
        if (!resultado.Ok) return NotFound();
        return Ok(new { resultado.Valor.IdAnalisis, resultado.Valor.Activo });
    }

    // ─── DELETE /api/analisis/{id} ────────────────────────────────────────
    // Solo elimina si no tiene presupuestos asociados
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await eliminar.EjecutarAsync(id);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }
}
