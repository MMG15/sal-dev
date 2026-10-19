using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Usuarios;

namespace Ssal.Api.Controllers;

// Gestión de usuarios del personal (CU-10). Solo el Administrador general (ROL-01) modifica cuentas.
[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "ROL-01")]
public class UsuariosController(
    ListarRoles listarRoles,
    ListarUsuarios listar,
    CrearUsuario crear,
    ActualizarUsuario actualizar,
    DesactivarUsuario desactivar,
    ActivarUsuario activar,
    RestablecerPassword restablecerPassword) : ControllerBase
{
    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/usuarios/roles ──────────────────────────────────────────
    [HttpGet("roles")]
    public async Task<IActionResult> Roles() => Ok(await listarRoles.EjecutarAsync());

    // ─── GET /api/usuarios ────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? busqueda, [FromQuery] string? estado) =>
        Ok(await listar.EjecutarAsync(busqueda, estado));

    // ─── POST /api/usuarios ───────────────────────────────────────────────
    // Alta con contraseña temporal (se muestra una sola vez; el usuario la cambia al ingresar)
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] GuardarUsuarioCommand req)
    {
        var resultado = await crear.EjecutarAsync(req, UsuarioActual);
        if (!resultado.Ok) return ErrorDeValidacion(resultado.Error!);
        return Ok(new { email = resultado.Valor!.Email, passwordTemporal = resultado.Valor.PasswordTemporal });
    }

    // ─── PUT /api/usuarios/{id} ───────────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarUsuarioCommand req)
    {
        var resultado = await actualizar.EjecutarAsync(id, req, UsuarioActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : ErrorDeValidacion(resultado.Error!);
        return NoContent();
    }

    // ─── POST /api/usuarios/{id}/desactivar ───────────────────────────────
    [HttpPost("{id:int}/desactivar")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var resultado = await desactivar.EjecutarAsync(id, UsuarioActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── POST /api/usuarios/{id}/activar ──────────────────────────────────
    [HttpPost("{id:int}/activar")]
    public async Task<IActionResult> Activar(int id)
    {
        var resultado = await activar.EjecutarAsync(id, UsuarioActual);
        if (!resultado.Ok) return NotFound();
        return NoContent();
    }

    // ─── POST /api/usuarios/{id}/restablecer-password ─────────────────────
    [HttpPost("{id:int}/restablecer-password")]
    public async Task<IActionResult> RestablecerPassword(int id)
    {
        var resultado = await restablecerPassword.EjecutarAsync(id, UsuarioActual);
        if (!resultado.Ok) return NotFound();
        return Ok(new { email = resultado.Valor!.Email, passwordTemporal = resultado.Valor.PasswordTemporal });
    }

    // "Ya existe un usuario con ese correo." es el único caso de los tres (nombre/apellido,
    // email inválido, rol inválido, duplicado) que el controller original devolvía como 409.
    private IActionResult ErrorDeValidacion(string error) =>
        error == "Ya existe un usuario con ese correo." ? Conflict(new { error }) : BadRequest(new { error });
}
