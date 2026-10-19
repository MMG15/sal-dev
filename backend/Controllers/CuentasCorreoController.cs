using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.CuentasCorreo;

namespace Ssal.Api.Controllers;

// Administración de las casillas desde las que se importan consultas (RF-06, preparación).
// Mismos roles que ya administran el resto de "Usuarios": el administrador general y el
// administrador de sistema (es, en los hechos, una integración externa — RF del backoffice).
[ApiController]
[Route("api/cuentas-correo")]
[Authorize(Roles = "ROL-01,ROL-06")]
public class CuentasCorreoController(
    ListarCuentasCorreo listar,
    CrearCuentaCorreo crear,
    ActualizarCuentaCorreo actualizar,
    RevisarCuentaAhora revisarAhora,
    EliminarCuentaCorreo eliminar) : ControllerBase
{
    // ─── GET /api/cuentas-correo ────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar() => Ok(await listar.EjecutarAsync());

    // ─── POST /api/cuentas-correo ───────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] GuardarCuentaCorreoCommand req)
    {
        var resultado = await crear.EjecutarAsync(req);
        if (!resultado.Ok) return BadRequest(new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── PUT /api/cuentas-correo/{id} ────────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarCuentaCorreoCommand req)
    {
        var resultado = await actualizar.EjecutarAsync(id, req);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── POST /api/cuentas-correo/{id}/revisar-ahora ────────────────────────
    [HttpPost("{id:int}/revisar-ahora")]
    public async Task<IActionResult> RevisarAhora(int id)
    {
        var resultado = await revisarAhora.EjecutarAsync(id, HttpContext.RequestAborted);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : StatusCode(StatusCodes.Status502BadGateway, new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── DELETE /api/cuentas-correo/{id} ─────────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var ok = await eliminar.EjecutarAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}
