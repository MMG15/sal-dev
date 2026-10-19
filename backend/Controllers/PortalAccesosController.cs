using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.PortalAccesos;

namespace Ssal.Api.Controllers;

// Alta y restablecimiento del acceso al portal desde la ficha del cliente (lo hace el personal, no el cliente).
[ApiController]
[Route("api/clientes/{idCliente:int}/acceso-portal")]
[Authorize(Roles = "ROL-01,ROL-02")]
public class PortalAccesosController(ObtenerEstadoAcceso obtenerEstado, GenerarOrestablecerAcceso generarOrestablecer) : ControllerBase
{
    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/clientes/{id}/acceso-portal ─────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Estado(int idCliente) => Ok(await obtenerEstado.EjecutarAsync(idCliente));

    // ─── POST /api/clientes/{id}/acceso-portal ────────────────────────────
    // Crea el acceso o, si ya existe, restablece la contraseña. La temporal se devuelve una sola vez.
    [HttpPost]
    public async Task<IActionResult> GenerarOrestablecer(int idCliente, [FromBody] GenerarAccesoCommand req)
    {
        var resultado = await generarOrestablecer.EjecutarAsync(idCliente, req, UsuarioActual);
        if (!resultado.Ok)
            return resultado.Error switch
            {
                "No encontrado" => NotFound(),
                "Ese correo ya tiene acceso al portal de otro cliente." => Conflict(new { error = resultado.Error }),
                _ => BadRequest(new { error = resultado.Error })
            };
        return Ok(new { email = resultado.Valor!.Email, passwordTemporal = resultado.Valor.PasswordTemporal });
    }
}
