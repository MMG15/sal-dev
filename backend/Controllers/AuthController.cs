using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Ssal.Api.Data;
using Ssal.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(SsalDbContext db, IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var usuario = await db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Email == req.Email && u.Activo);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(req.Password, usuario.PasswordHash))
        {
            db.Auditoria.Add(new AuditoriaSistema
            {
                Accion = "login_fallido",
                Objetivo = req.Email,
                Detalle = "Credenciales incorrectas"
            });
            await db.SaveChangesAsync();
            return Unauthorized(new { mensaje = "Credenciales incorrectas." });
        }

        usuario.UltimoAcceso = DateTime.UtcNow;
        db.Auditoria.Add(new AuditoriaSistema
        {
            IdUsuario = usuario.IdUsuario,
            Accion = "login_ok",
            Objetivo = usuario.Email
        });
        await db.SaveChangesAsync();

        return Ok(new LoginResponse(
            Token: GenerarToken(usuario),
            IdUsuario: usuario.IdUsuario,
            Nombre: usuario.Nombre,
            Apellido: usuario.Apellido,
            Email: usuario.Email,
            RolCodigo: usuario.Rol.Codigo,
            RolNombre: usuario.Rol.Nombre,
            ForzarCambioPwd: usuario.ForzarCambioPwd
        ));
    }

    // ─── POST /api/auth/cambiar-password ──────────────────────────────────
    // Cambio de la propia contraseña (también obligatorio tras un alta o restablecimiento)
    [HttpPost("cambiar-password")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest req)
    {
        var id = int.Parse(
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        if (!BCrypt.Net.BCrypt.Verify(req.PasswordActual, usuario.PasswordHash))
            return BadRequest(new { error = "La contraseña actual no es correcta." });

        if (string.IsNullOrEmpty(req.PasswordNueva) || req.PasswordNueva.Length < 8)
            return BadRequest(new { error = "La nueva contraseña debe tener al menos 8 caracteres." });

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.PasswordNueva);
        usuario.ForzarCambioPwd = false;
        db.Auditoria.Add(new AuditoriaSistema { IdUsuario = id, Accion = "cambio_password", Objetivo = usuario.Email });
        await db.SaveChangesAsync();
        return NoContent();
    }

    private string GenerarToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.IdUsuario.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Rol.Codigo),
            new Claim("nombre", usuario.Nombre),
            new Claim("apellido", usuario.Apellido),
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpiresInMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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
