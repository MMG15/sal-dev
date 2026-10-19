using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Auth;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IniciarSesion iniciarSesion, CambiarPassword cambiarPassword) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var resultado = await iniciarSesion.EjecutarAsync(req.Email, req.Password);
        if (!resultado.Ok) return Unauthorized(new { mensaje = resultado.Error });

        var r = resultado.Valor!;
        return Ok(new LoginResponse(r.Token, r.IdUsuario, r.Nombre, r.Apellido, r.Email, r.RolCodigo, r.RolNombre, r.ForzarCambioPwd));
    }

    // ─── POST /api/auth/cambiar-password ──────────────────────────────────
    // Cambio de la propia contraseña (también obligatorio tras un alta o restablecimiento)
    [HttpPost("cambiar-password")]
    [Authorize]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest req)
    {
        var id = int.Parse(
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var resultado = await cambiarPassword.EjecutarAsync(id, req.PasswordActual, req.PasswordNueva);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }
}

public record LoginRequest(string Email, string Password);

public record CambiarPasswordRequest(string PasswordActual, string? PasswordNueva);

public record LoginResponse(
    string Token,
    int IdUsuario,
    string Nombre,
    string Apellido,
    string Email,
    string RolCodigo,
    string RolNombre,
    bool ForzarCambioPwd
);
