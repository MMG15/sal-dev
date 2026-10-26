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
    EliminarAnalisis eliminar,
    CrearGrupo crearGrupo,
    ActualizarGrupo actualizarGrupo,
    ListarPaquetesActivos listarPaquetesActivos,
    ListarTodosLosPaquetes listarTodosLosPaquetes,
    CrearPaquete crearPaquete,
    ActualizarPaquete actualizarPaquete,
    ToggleActivoPaquete toggleActivoPaquete,
    EliminarPaquete eliminarPaquete,
    ListarMetodosActivos listarMetodosActivos,
    ListarTodosLosMetodos listarTodosLosMetodos,
    CrearMetodo crearMetodo,
    ActualizarMetodo actualizarMetodo,
    ToggleActivoMetodo toggleActivoMetodo,
    EliminarMetodo eliminarMetodo) : ControllerBase
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

    // ─── POST /api/analisis/grupos ──────────────────────────────────────────
    [HttpPost("grupos")]
    public async Task<IActionResult> CrearGrupo([FromBody] GuardarGrupoCommand req)
    {
        var resultado = await crearGrupo.EjecutarAsync(req);
        if (!resultado.Ok) return BadRequest(new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── PUT /api/analisis/grupos/{id} ─────────────────────────────────────
    [HttpPut("grupos/{id}")]
    public async Task<IActionResult> ActualizarGrupo(int id, [FromBody] GuardarGrupoCommand req)
    {
        var resultado = await actualizarGrupo.EjecutarAsync(id, req);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── GET /api/analisis/paquetes ────────────────────────────────────────
    // Para el selector de presupuesto: solo activos
    [HttpGet("paquetes")]
    public async Task<IActionResult> Paquetes() => Ok(await listarPaquetesActivos.EjecutarAsync());

    // ─── GET /api/analisis/paquetes/todos ──────────────────────────────────
    // Para gestión: todos incluyendo inactivos
    [HttpGet("paquetes/todos")]
    public async Task<IActionResult> PaquetesTodos() => Ok(await listarTodosLosPaquetes.EjecutarAsync());

    // ─── POST /api/analisis/paquetes ───────────────────────────────────────
    [HttpPost("paquetes")]
    public async Task<IActionResult> CrearPaquete([FromBody] GuardarPaqueteCommand req)
    {
        var resultado = await crearPaquete.EjecutarAsync(req);
        if (!resultado.Ok) return BadRequest(new { error = resultado.Error });
        return Ok(new { IdPaquete = resultado.Valor });
    }

    // ─── PUT /api/analisis/paquetes/{id} ───────────────────────────────────
    [HttpPut("paquetes/{id}")]
    public async Task<IActionResult> ActualizarPaquete(int id, [FromBody] GuardarPaqueteCommand req)
    {
        var resultado = await actualizarPaquete.EjecutarAsync(id, req);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── PATCH /api/analisis/paquetes/{id}/activo ──────────────────────────
    [HttpPatch("paquetes/{id}/activo")]
    public async Task<IActionResult> ToggleActivoPaquete(int id)
    {
        var resultado = await toggleActivoPaquete.EjecutarAsync(id);
        if (!resultado.Ok) return NotFound();
        return Ok(new { resultado.Valor.IdPaquete, resultado.Valor.Activo });
    }

    // ─── DELETE /api/analisis/paquetes/{id} ────────────────────────────────
    [HttpDelete("paquetes/{id}")]
    public async Task<IActionResult> EliminarPaquete(int id)
    {
        var resultado = await eliminarPaquete.EjecutarAsync(id);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── GET /api/analisis/metodos ─────────────────────────────────────────
    // Para el selector de presupuesto / análisis: solo activos
    [HttpGet("metodos")]
    public async Task<IActionResult> Metodos() => Ok(await listarMetodosActivos.EjecutarAsync());

    // ─── GET /api/analisis/metodos/todos ───────────────────────────────────
    // Para gestión: todos incluyendo inactivos
    [HttpGet("metodos/todos")]
    public async Task<IActionResult> MetodosTodos() => Ok(await listarTodosLosMetodos.EjecutarAsync());

    // ─── POST /api/analisis/metodos ────────────────────────────────────────
    [HttpPost("metodos")]
    public async Task<IActionResult> CrearMetodo([FromBody] GuardarMetodoCommand req)
    {
        var resultado = await crearMetodo.EjecutarAsync(req);
        if (!resultado.Ok) return BadRequest(new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── PUT /api/analisis/metodos/{id} ────────────────────────────────────
    [HttpPut("metodos/{id}")]
    public async Task<IActionResult> ActualizarMetodo(int id, [FromBody] GuardarMetodoCommand req)
    {
        var resultado = await actualizarMetodo.EjecutarAsync(id, req);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── PATCH /api/analisis/metodos/{id}/activo ───────────────────────────
    [HttpPatch("metodos/{id}/activo")]
    public async Task<IActionResult> ToggleActivoMetodo(int id)
    {
        var resultado = await toggleActivoMetodo.EjecutarAsync(id);
        if (!resultado.Ok) return NotFound();
        return Ok(new { resultado.Valor.IdMetodo, resultado.Valor.Activo });
    }

    // ─── DELETE /api/analisis/metodos/{id} ─────────────────────────────────
    [HttpDelete("metodos/{id}")]
    public async Task<IActionResult> EliminarMetodo(int id)
    {
        var resultado = await eliminarMetodo.EjecutarAsync(id);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }
}
