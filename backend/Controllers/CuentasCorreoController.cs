using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.CuentasCorreo;

namespace Ssal.Api.Controllers;

// Administración de las casillas desde las que se importan consultas (RF-06, preparación).
// Crear/Actualizar/Eliminar tocan la contraseña de la casilla, así que quedan exclusivos del
// administrador general y el administrador de sistema. Listar y "revisar ahora" no exponen ni
// tocan ninguna credencial (el DTO de listado no incluye la contraseña), así que están abiertos
// a cualquier rol que también tenga acceso a "Correo entrante" (ver frontend/src/config/modulos.ts) —
// si no, esos usuarios dependen de un admin para forzar la revisión de la bandeja.
[ApiController]
[Route("api/cuentas-correo")]
public class CuentasCorreoController(
    ListarCuentasCorreo listar,
    CrearCuentaCorreo crear,
    ActualizarCuentaCorreo actualizar,
    RevisarCuentaAhora revisarAhora,
    EliminarCuentaCorreo eliminar) : ControllerBase
{
    const string RolesLectura = "ROL-01,ROL-02,ROL-03,ROL-06,ROL-07";
    const string RolesAdministracion = "ROL-01,ROL-06";

    // ─── GET /api/cuentas-correo ────────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = RolesLectura)]
    public async Task<IActionResult> Listar() => Ok(await listar.EjecutarAsync());

    // ─── POST /api/cuentas-correo ───────────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = RolesAdministracion)]
    public async Task<IActionResult> Crear([FromBody] GuardarCuentaCorreoCommand req)
    {
        var resultado = await crear.EjecutarAsync(req);
        if (!resultado.Ok) return BadRequest(new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── PUT /api/cuentas-correo/{id} ────────────────────────────────────────
    [HttpPut("{id:int}")]
    [Authorize(Roles = RolesAdministracion)]
    public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarCuentaCorreoCommand req)
    {
        var resultado = await actualizar.EjecutarAsync(id, req);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── POST /api/cuentas-correo/{id}/revisar-ahora ────────────────────────
    [HttpPost("{id:int}/revisar-ahora")]
    [Authorize(Roles = RolesLectura)]
    public async Task<IActionResult> RevisarAhora(int id)
    {
        var resultado = await revisarAhora.EjecutarAsync(id, HttpContext.RequestAborted);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : StatusCode(StatusCodes.Status502BadGateway, new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── DELETE /api/cuentas-correo/{id} ─────────────────────────────────────
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RolesAdministracion)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var ok = await eliminar.EjecutarAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}
