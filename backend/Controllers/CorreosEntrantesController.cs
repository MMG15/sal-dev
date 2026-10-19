using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Correos;

namespace Ssal.Api.Controllers;

// Bandeja de correo del laboratorio (RF-06 / preparación): todo lo que llega al mail de
// consultas pasa primero por acá. Mismos roles que pueden gestionar Consultas, porque
// convertir un correo es, en los hechos, crear una Consulta.
[ApiController]
[Route("api/correos-entrantes")]
[Authorize(Roles = "ROL-01,ROL-02,ROL-03,ROL-07")]
public class CorreosEntrantesController(
    ListarCorreos listar,
    ObtenerCorreo obtener,
    ResponderCorreo responder,
    DescartarCorreo descartar,
    ConvertirCorreo convertir) : ControllerBase
{
    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/correos-entrantes ────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20) =>
        Ok(await listar.EjecutarAsync(busqueda, estado, pagina, porPagina));

    // ─── GET /api/correos-entrantes/{id} ───────────────────────────────────
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var correo = await obtener.EjecutarAsync(id);
        if (correo is null) return NotFound();
        return Ok(correo);
    }

    // ─── PUT /api/correos-entrantes/{id}/responder ─────────────────────────
    // Manda la respuesta de verdad, por SMTP, desde la misma casilla por la que entró el
    // correo (o la primera casilla habilitada si no se sabe cuál fue). Así la respuesta
    // queda en "Enviados" de esa cuenta y, si el cliente contesta, su respuesta vuelve a la
    // misma casilla que el importador revisa — no a la cuenta personal de quien respondió.
    [HttpPut("{id:int}/responder")]
    public async Task<IActionResult> Responder(int id, [FromBody] ResponderCorreoCommand req)
    {
        var resultado = await responder.EjecutarAsync(id, req, UsuarioActual, HttpContext.RequestAborted);
        if (!resultado.Ok)
            return resultado.Error switch
            {
                "No encontrado" => NotFound(),
                string e when e.StartsWith("No se pudo enviar la respuesta") => StatusCode(StatusCodes.Status502BadGateway, new { error = e }),
                _ => BadRequest(new { error = resultado.Error })
            };
        return NoContent();
    }

    // ─── PUT /api/correos-entrantes/{id}/descartar ─────────────────────────
    [HttpPut("{id:int}/descartar")]
    public async Task<IActionResult> Descartar(int id)
    {
        var resultado = await descartar.EjecutarAsync(id, UsuarioActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return NoContent();
    }

    // ─── POST /api/correos-entrantes/{id}/convertir ────────────────────────
    [HttpPost("{id:int}/convertir")]
    public async Task<IActionResult> Convertir(int id, [FromBody] ConvertirCorreoCommand req)
    {
        var resultado = await convertir.EjecutarAsync(id, req, UsuarioActual);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return Ok(resultado.Valor);
    }
}
