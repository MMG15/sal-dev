using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;
using Ssal.Api.Services;

namespace Ssal.Api.Controllers;

// Gestión de usuarios del personal (CU-10). Solo el Administrador general (ROL-01) modifica cuentas.
[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "ROL-01")]
public class UsuariosController(SsalDbContext db) : ControllerBase
{
    // Los clientes externos no se asignan como personal: tienen su propio acceso al portal
    private const string RolNoAsignable = "ROL-05";

    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/usuarios/roles ──────────────────────────────────────────
    [HttpGet("roles")]
    public async Task<IActionResult> Roles()
    {
        var roles = await db.Roles
            .Where(r => r.Codigo != RolNoAsignable)
            .OrderBy(r => r.IdRol)
            .Select(r => new { r.IdRol, r.Codigo, r.Nombre, r.Descripcion })
            .ToListAsync();
        return Ok(roles);
    }

    // ─── GET /api/usuarios ────────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? busqueda, [FromQuery] string? estado)
    {
        var q = db.Usuarios.Include(u => u.Rol).AsQueryable();

        if (estado == "activos") q = q.Where(u => u.Activo);
        if (estado == "inactivos") q = q.Where(u => !u.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(u =>
                (u.Nombre + " " + u.Apellido).ToLower().Contains(b) ||
                u.Email.ToLower().Contains(b));
        }

        var items = await q
            .OrderBy(u => u.Apellido).ThenBy(u => u.Nombre)
            .Select(u => new
            {
                u.IdUsuario,
                u.Nombre,
                u.Apellido,
                u.Email,
                u.IdRol,
                RolCodigo = u.Rol.Codigo,
                RolNombre = u.Rol.Nombre,
                u.Activo,
                u.UltimoAcceso,
                u.ForzarCambioPwd
            })
            .ToListAsync();

        return Ok(items);
    }

    // ─── POST /api/usuarios ───────────────────────────────────────────────
    // Alta con contraseña temporal (se muestra una sola vez; el usuario la cambia al ingresar)
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] UsuarioRequest req)
    {
        var validacion = await ValidarDatos(req, idExcluido: null);
        if (validacion is not null) return validacion;

        var email = req.Email.Trim().ToLowerInvariant();
        var temporal = Contrasenas.Temporal();

        var usuario = new Usuario
        {
            Nombre = req.Nombre.Trim(),
            Apellido = req.Apellido.Trim(),
            Email = email,
            IdRol = req.IdRol,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporal),
            Activo = true,
            ForzarCambioPwd = true
        };
        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        Auditar("alta_usuario", email, $"Alta con rol {await CodigoRol(req.IdRol)}");
        await db.SaveChangesAsync();

        return Ok(new { email, passwordTemporal = temporal });
    }

    // ─── PUT /api/usuarios/{id} ───────────────────────────────────────────
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] UsuarioRequest req)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        var validacion = await ValidarDatos(req, idExcluido: id);
        if (validacion is not null) return validacion;

        // Evita que un administrador se quede sin acceso por error
        if (id == UsuarioActual && req.IdRol != usuario.IdRol)
            return BadRequest(new { error = "No podés cambiar tu propio rol." });

        var cambios = new List<string>();
        if (usuario.Nombre != req.Nombre.Trim() || usuario.Apellido != req.Apellido.Trim())
            cambios.Add("datos personales");
        if (usuario.Email != req.Email.Trim().ToLowerInvariant())
            cambios.Add("correo");

        var rolAnterior = usuario.IdRol;
        if (rolAnterior != req.IdRol)
        {
            cambios.Add($"rol {await CodigoRol(rolAnterior)} → {await CodigoRol(req.IdRol)}");
        }

        usuario.Nombre = req.Nombre.Trim();
        usuario.Apellido = req.Apellido.Trim();
        usuario.Email = req.Email.Trim().ToLowerInvariant();
        usuario.IdRol = req.IdRol;

        if (cambios.Count > 0)
        {
            var accion = rolAnterior != req.IdRol ? "cambio_rol" : "modificar_usuario";
            Auditar(accion, usuario.Email, string.Join("; ", cambios));
        }

        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── POST /api/usuarios/{id}/desactivar ───────────────────────────────
    [HttpPost("{id:int}/desactivar")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();
        if (id == UsuarioActual) return BadRequest(new { error = "No podés desactivar tu propia cuenta." });

        usuario.Activo = false;
        Auditar("desactivar_usuario", usuario.Email, null);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── POST /api/usuarios/{id}/activar ──────────────────────────────────
    [HttpPost("{id:int}/activar")]
    public async Task<IActionResult> Activar(int id)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        usuario.Activo = true;
        Auditar("activar_usuario", usuario.Email, null);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── POST /api/usuarios/{id}/restablecer-password ─────────────────────
    [HttpPost("{id:int}/restablecer-password")]
    public async Task<IActionResult> RestablecerPassword(int id)
    {
        var usuario = await db.Usuarios.FindAsync(id);
        if (usuario is null) return NotFound();

        var temporal = Contrasenas.Temporal();
        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporal);
        usuario.ForzarCambioPwd = true;

        Auditar("reset_password", usuario.Email, null);
        await db.SaveChangesAsync();
        return Ok(new { email = usuario.Email, passwordTemporal = temporal });
    }

    // ─── Helpers ──────────────────────────────────────────────────────────

    private async Task<IActionResult?> ValidarDatos(UsuarioRequest req, int? idExcluido)
    {
        if (string.IsNullOrWhiteSpace(req.Nombre) || string.IsNullOrWhiteSpace(req.Apellido))
            return BadRequest(new { error = "Nombre y apellido son obligatorios." });

        if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains('@'))
            return BadRequest(new { error = "Indicá un correo electrónico válido." });

        var rol = await db.Roles.FindAsync(req.IdRol);
        if (rol is null || rol.Codigo == RolNoAsignable)
            return BadRequest(new { error = "Rol no válido." });

        var email = req.Email.Trim().ToLowerInvariant();
        var duplicado = await db.Usuarios.AnyAsync(u => u.Email == email && u.IdUsuario != (idExcluido ?? -1));
        if (duplicado)
            return Conflict(new { error = "Ya existe un usuario con ese correo." });

        return null;
    }

    private async Task<string> CodigoRol(int idRol) =>
        (await db.Roles.FindAsync(idRol))?.Codigo ?? "?";

    private void Auditar(string accion, string objetivo, string? detalle)
    {
        db.Auditoria.Add(new AuditoriaSistema
        {
            IdUsuario = UsuarioActual,
            Accion = accion,
            Objetivo = objetivo,
            Detalle = detalle
        });
    }
}

public record UsuarioRequest(string Nombre, string Apellido, string Email, int IdRol);
