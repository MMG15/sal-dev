using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Firmas;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/firmas")]
[Authorize]
public class FirmasController(
    ListarFirmas listar,
    ObtenerFirma obtener,
    FirmarDocumento firmar,
    AprobarDocumento aprobar,
    RechazarDocumento rechazar,
    ReenviarDocumento reenviar) : ControllerBase
{
    // Lectura del módulo y rechazo: responsables de área y Administrador
    private const string RolesFirma = "ROL-01,ROL-08,ROL-09";
    // Firma del área: solo el responsable de MIC o de FQ
    private const string RolesResponsable = "ROL-08,ROL-09";
    // Aprobación final: Administrador general
    private const string RolesAprobacion = "ROL-01";
    // Reenvío tras rechazo: quien puede crear el documento (se valida por tipo más abajo)
    private const string RolesReenvio = "ROL-01,ROL-03,ROL-07,ROL-08,ROL-09";

    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    string RolActual => User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

    // ─── GET /api/firmas/documentos ───────────────────────────────────────
    [HttpGet("documentos")]
    [Authorize(Roles = RolesFirma)]
    public async Task<IActionResult> Listar([FromQuery] string? tipo, [FromQuery] string? estado, [FromQuery] string? area) =>
        Ok(await listar.EjecutarAsync(tipo, estado, area));

    // ─── GET /api/firmas/documentos/{tipo}/{id} ───────────────────────────
    [HttpGet("documentos/{tipo}/{id:int}")]
    [Authorize(Roles = RolesFirma)]
    public async Task<IActionResult> Detalle(string tipo, int id)
    {
        var detalle = await obtener.EjecutarAsync(tipo, id, RolActual);
        if (detalle is null) return NotFound();
        return Ok(detalle);
    }

    // ─── POST /api/firmas/documentos/{tipo}/{id}/firmar ───────────────────
    [HttpPost("documentos/{tipo}/{id:int}/firmar")]
    [Authorize(Roles = RolesResponsable)]
    public async Task<IActionResult> Firmar(string tipo, int id, [FromBody] ConfirmarPasswordCommand req)
    {
        var resultado = await firmar.EjecutarAsync(tipo, id, req, UsuarioActual, RolActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── POST /api/firmas/documentos/{tipo}/{id}/aprobar ──────────────────
    [HttpPost("documentos/{tipo}/{id:int}/aprobar")]
    [Authorize(Roles = RolesAprobacion)]
    public async Task<IActionResult> Aprobar(string tipo, int id, [FromBody] ConfirmarPasswordCommand req)
    {
        var resultado = await aprobar.EjecutarAsync(tipo, id, req, UsuarioActual, RolActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── POST /api/firmas/documentos/{tipo}/{id}/rechazar ─────────────────
    [HttpPost("documentos/{tipo}/{id:int}/rechazar")]
    [Authorize(Roles = RolesFirma)]
    public async Task<IActionResult> Rechazar(string tipo, int id, [FromBody] RechazarCommand req)
    {
        var resultado = await rechazar.EjecutarAsync(tipo, id, req, UsuarioActual, RolActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── POST /api/firmas/documentos/{tipo}/{id}/reenviar ─────────────────
    [HttpPost("documentos/{tipo}/{id:int}/reenviar")]
    [Authorize(Roles = RolesReenvio)]
    public async Task<IActionResult> Reenviar(string tipo, int id)
    {
        var resultado = await reenviar.EjecutarAsync(tipo, id, UsuarioActual, RolActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }
}
