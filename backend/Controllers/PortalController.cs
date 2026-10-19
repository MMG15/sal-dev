using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Portal;

namespace Ssal.Api.Controllers;

// Portal web del cliente externo (RF-22, CU-07). Solo lectura sobre sus propios pedidos.
// Un informe se muestra y descarga únicamente cuando está aprobado (RF-36).
[ApiController]
[Route("api/portal")]
public class PortalController(
    IniciarSesionPortal iniciarSesion,
    CambiarPasswordPortal cambiarPassword,
    ListarPedidos listarPedidos,
    ObtenerInformeDetalle obtenerDetalle,
    DescargarInformePortalPdf descargarPdf) : ControllerBase
{
    private const string RolPortal = "PORTAL";

    int CuentaActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── POST /api/portal/login ───────────────────────────────────────────
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var resultado = await iniciarSesion.EjecutarAsync(req.Email, req.Password);
        if (!resultado.Ok) return Unauthorized(new { mensaje = resultado.Error });

        var r = resultado.Valor!;
        return Ok(new
        {
            token = r.Token,
            idUsuario = r.IdUsuario,
            nombre = r.Nombre,
            apellido = r.Apellido,
            email = r.Email,
            rolCodigo = r.RolCodigo,
            rolNombre = r.RolNombre,
            forzarCambioPwd = r.ForzarCambioPwd
        });
    }

    // ─── POST /api/portal/cambiar-password ────────────────────────────────
    [HttpPost("cambiar-password")]
    [Authorize(Roles = RolPortal)]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordPortalCommand req)
    {
        var resultado = await cambiarPassword.EjecutarAsync(CuentaActual, req);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── GET /api/portal/pedidos ──────────────────────────────────────────
    [HttpGet("pedidos")]
    [Authorize(Roles = RolPortal)]
    public async Task<IActionResult> Pedidos()
    {
        var items = await listarPedidos.EjecutarAsync(CuentaActual);
        if (items is null) return NotFound();
        return Ok(items);
    }

    // ─── GET /api/portal/informes/{id} ────────────────────────────────────
    [HttpGet("informes/{id:int}")]
    [Authorize(Roles = RolPortal)]
    public async Task<IActionResult> Detalle(int id)
    {
        var (dto, existe, aprobado) = await obtenerDetalle.EjecutarAsync(id, CuentaActual);
        if (!existe) return NotFound();
        if (!aprobado) return StatusCode(StatusCodes.Status403Forbidden, new { error = "Este informe todavía no está disponible." });
        return Ok(dto);
    }

    // ─── GET /api/portal/informes/{id}/pdf ────────────────────────────────
    [HttpGet("informes/{id:int}/pdf")]
    [Authorize(Roles = RolPortal)]
    public async Task<IActionResult> Pdf(int id)
    {
        var (pdf, perteneceAlCliente) = await descargarPdf.EjecutarAsync(id, CuentaActual);
        if (!perteneceAlCliente) return NotFound();
        if (pdf is null) return StatusCode(StatusCodes.Status403Forbidden, new { error = "Este informe todavía no está disponible." });
        return File(pdf.Contenido, "application/pdf", pdf.NombreArchivo);
    }
}
