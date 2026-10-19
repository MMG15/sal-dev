using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;
using Ssal.Api.Services;

namespace Ssal.Api.Controllers;

// Administración de las casillas desde las que se importan consultas (RF-06, preparación).
// Mismos roles que ya administran el resto de "Usuarios": el administrador general y el
// administrador de sistema (es, en los hechos, una integración externa — RF del backoffice).
[ApiController]
[Route("api/cuentas-correo")]
[Authorize(Roles = "ROL-01,ROL-06")]
public class CuentasCorreoController(SsalDbContext db, IConfiguration config) : ControllerBase
{
    private string ClaveCifrado => config["MailImport:ClaveCifrado"]
        ?? throw new InvalidOperationException("MailImport:ClaveCifrado no está configurada.");

    // ─── GET /api/cuentas-correo ────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var items = await db.CuentasCorreo
            .OrderBy(c => c.Nombre)
            .Select(c => new
            {
                c.IdCuenta,
                c.Nombre,
                c.Servidor,
                c.Puerto,
                c.Usuario,
                c.Habilitada,
                c.IntervaloMinutos,
                c.UltimaRevision,
                c.UltimoError
            })
            .ToListAsync();

        return Ok(items);
    }

    // ─── POST /api/cuentas-correo ───────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CuentaCorreoRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Nombre) || string.IsNullOrWhiteSpace(req.Usuario))
            return BadRequest(new { error = "Nombre y usuario son obligatorios." });
        if (string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { error = "Ingresá la contraseña de aplicación de la casilla." });

        var cuenta = new CuentaCorreo
        {
            Nombre = req.Nombre.Trim(),
            Servidor = string.IsNullOrWhiteSpace(req.Servidor) ? "imap.gmail.com" : req.Servidor.Trim(),
            Puerto = req.Puerto is > 0 ? req.Puerto.Value : 993,
            Usuario = req.Usuario.Trim(),
            PasswordCifrada = Cifrado.Encriptar(LimpiarPassword(req.Password), ClaveCifrado),
            Habilitada = req.Habilitada ?? true,
            IntervaloMinutos = req.IntervaloMinutos is > 0 ? req.IntervaloMinutos.Value : 5
        };
        db.CuentasCorreo.Add(cuenta);
        await db.SaveChangesAsync();
        return Ok(new { cuenta.IdCuenta });
    }

    // ─── PUT /api/cuentas-correo/{id} ────────────────────────────────────────
    // La contraseña es opcional: si no se manda, se conserva la que ya estaba.
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] CuentaCorreoRequest req)
    {
        var cuenta = await db.CuentasCorreo.FindAsync(id);
        if (cuenta is null) return NotFound();

        if (string.IsNullOrWhiteSpace(req.Nombre) || string.IsNullOrWhiteSpace(req.Usuario))
            return BadRequest(new { error = "Nombre y usuario son obligatorios." });

        cuenta.Nombre = req.Nombre.Trim();
        cuenta.Servidor = string.IsNullOrWhiteSpace(req.Servidor) ? "imap.gmail.com" : req.Servidor.Trim();
        cuenta.Puerto = req.Puerto is > 0 ? req.Puerto.Value : 993;
        cuenta.Usuario = req.Usuario.Trim();
        cuenta.Habilitada = req.Habilitada ?? cuenta.Habilitada;
        cuenta.IntervaloMinutos = req.IntervaloMinutos is > 0 ? req.IntervaloMinutos.Value : cuenta.IntervaloMinutos;
        if (!string.IsNullOrWhiteSpace(req.Password))
            cuenta.PasswordCifrada = Cifrado.Encriptar(LimpiarPassword(req.Password), ClaveCifrado);
        cuenta.UpdatedAt = DateTime.UtcNow;
        // Un cambio de credenciales o de casilla amerita volver a intentar desde cero
        cuenta.UltimoError = null;

        await db.SaveChangesAsync();
        return NoContent();
    }

    // ─── POST /api/cuentas-correo/{id}/revisar-ahora ────────────────────────
    [HttpPost("{id:int}/revisar-ahora")]
    public async Task<IActionResult> RevisarAhora(int id)
    {
        var existe = await db.CuentasCorreo.AnyAsync(c => c.IdCuenta == id);
        if (!existe) return NotFound();

        var (importados, error) = await ImportadorCorreosBackgroundService.RevisarUnaCuentaAsync(
            HttpContext.RequestServices, id, ClaveCifrado, HttpContext.RequestAborted);

        if (error is not null)
            return StatusCode(StatusCodes.Status502BadGateway, new { error });

        return Ok(new { importados });
    }

    // ─── DELETE /api/cuentas-correo/{id} ─────────────────────────────────────
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var cuenta = await db.CuentasCorreo.FindAsync(id);
        if (cuenta is null) return NotFound();

        db.CuentasCorreo.Remove(cuenta);
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Google muestra la contraseña de aplicación separada en grupos de 4 letras para que
    // sea fácil de leer, pero eso no es parte de la contraseña real. Además, copiar texto
    // desde una página puede traer caracteres invisibles que no son espacios comunes
    // (separadores "no-break", etc.) y que un filtro de solo espacios no detecta. Por eso
    // nos quedamos únicamente con letras y números, que es lo único que una contraseña de
    // aplicación de Google puede tener.
    private static string LimpiarPassword(string password) =>
        new string(password.Where(char.IsLetterOrDigit).ToArray());
}

public record CuentaCorreoRequest(
    string Nombre,
    string? Servidor,
    int? Puerto,
    string Usuario,
    string? Password,
    bool? Habilitada,
    int? IntervaloMinutos);
