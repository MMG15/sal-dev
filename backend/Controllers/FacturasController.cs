using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Facturas;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/facturas")]
[Authorize]
public class FacturasController(
    ListarFacturas listar,
    ListarSsesDisponibles listarSsesDisponibles,
    RegistrarFactura registrar,
    MarcarFacturaPagada marcarPagada) : ControllerBase
{
    // Emisión de la factura en TANGO: Responsable de Área (paso 10 del flujo PO-01) y Administrador.
    private const string RolesEmision = "ROL-01,ROL-08,ROL-09";
    // Consulta de facturas y registro de cobro: además Cobranzas (RF-26).
    private const string RolesFacturacion = "ROL-01,ROL-04,ROL-08,ROL-09";

    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/facturas ────────────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = RolesFacturacion)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 50) =>
        Ok(await listar.EjecutarAsync(busqueda, estado, pagina, porPagina));

    // ─── GET /api/facturas/sses-disponibles ───────────────────────────────
    [HttpGet("sses-disponibles")]
    [Authorize(Roles = RolesEmision)]
    public async Task<IActionResult> SsesDisponibles() => Ok(await listarSsesDisponibles.EjecutarAsync());

    // ─── POST /api/facturas ───────────────────────────────────────────────
    [HttpPost]
    [Authorize(Roles = RolesEmision)]
    public async Task<IActionResult> Registrar([FromBody] RegistrarFacturaCommand req)
    {
        var resultado = await registrar.EjecutarAsync(req, UsuarioActual);
        if (!resultado.Ok)
        {
            var esConflicto = resultado.Error!.Contains("ya tiene una factura") || resultado.Error.Contains("Ya existe una factura");
            return esConflicto ? Conflict(new { error = resultado.Error }) : BadRequest(new { error = resultado.Error });
        }
        return CreatedAtAction(nameof(Listar), new { id = resultado.Valor.IdFactura }, new { resultado.Valor.IdFactura, resultado.Valor.Numero });
    }

    // ─── PUT /api/facturas/{id}/pago ──────────────────────────────────────
    [HttpPut("{id:int}/pago")]
    [Authorize(Roles = RolesFacturacion)]
    public async Task<IActionResult> MarcarPagada(int id, [FromBody] MarcarPagadaCommand req)
    {
        var resultado = await marcarPagada.EjecutarAsync(id, req);
        if (!resultado.Ok)
            return resultado.Error == "No encontrada" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }
}
