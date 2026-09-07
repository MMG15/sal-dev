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
            return Unauthorized(new { mensaje = "Credenciales incorrectas." });

        usuario.UltimoAcceso = DateTime.UtcNow;
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
