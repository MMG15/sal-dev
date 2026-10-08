using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;
using System.Security.Cryptography;

namespace Ssal.Api.Controllers;

// Alta y restablecimiento del acceso al portal desde la ficha del cliente (lo hace el personal, no el cliente).
[ApiController]
[Route("api/clientes/{idCliente:int}/acceso-portal")]
[Authorize(Roles = "ROL-01,ROL-02")]
public class PortalAccesosController(SsalDbContext db) : ControllerBase
{
    private const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";

    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/clientes/{id}/acceso-portal ─────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Estado(int idCliente)
    {
        var acceso = await db.PortalUsuarios.FirstOrDefaultAsync(p => p.IdCliente == idCliente);
        if (acceso is null) return Ok(new { existe = false });

        return Ok(new
        {
            existe = true,
            acceso.Email,
            acceso.Activo,
            acceso.ForzarCambioPwd,
            acceso.UltimoAcceso
        });
    }

    // ─── POST /api/clientes/{id}/acceso-portal ────────────────────────────
    // Crea el acceso o, si ya existe, restablece la contraseña. La temporal se devuelve una sola vez.
    [HttpPost]
    public async Task<IActionResult> GenerarOrestablecer(int idCliente, [FromBody] GenerarAccesoRequest req)
    {
        var cliente = await db.Clientes.FindAsync(idCliente);
        if (cliente is null) return NotFound();

        var email = (req.Email ?? cliente.Email)?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { error = "Indicá un correo electrónico para el acceso del cliente." });

        var emailEnUso = await db.PortalUsuarios.AnyAsync(p => p.Email == email && p.IdCliente != idCliente);
        if (emailEnUso)
            return Conflict(new { error = "Ese correo ya tiene acceso al portal de otro cliente." });

        var temporal = RandomNumberGenerator.GetString(Alfabeto, 10);
        var hash = BCrypt.Net.BCrypt.HashPassword(temporal);

        var acceso = await db.PortalUsuarios.FirstOrDefaultAsync(p => p.IdCliente == idCliente);
        if (acceso is null)
        {
            db.PortalUsuarios.Add(new PortalUsuario
            {
                IdCliente = idCliente,
                Email = email,
                PasswordHash = hash,
                ForzarCambioPwd = true,
                Activo = true,
                CreadoPor = UsuarioActual
            });
        }
        else
        {
            acceso.Email = email;
            acceso.PasswordHash = hash;
            acceso.ForzarCambioPwd = true;
            acceso.Activo = true;
        }

        await db.SaveChangesAsync();
        return Ok(new { email, passwordTemporal = temporal });
    }
}

public record GenerarAccesoRequest(string? Email);
