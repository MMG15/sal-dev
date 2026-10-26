using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.DocumentosInstitucionales;

namespace Ssal.Api.Controllers;

// Biblioteca de documentos reutilizables (ISO 9001, procedimientos, disposiciones, habilitantes)
// que se cargan una vez y se adjuntan desde cualquier presupuesto. Subir/eliminar queda exclusivo
// del admin general y el admin de sistema (mismo criterio que Cuentas de correo); listar/descargar
// lo pueden usar los mismos roles que gestionan presupuestos, para poder elegir qué adjuntar.
[ApiController]
[Route("api/documentos-institucionales")]
public class DocumentosInstitucionalesController(
    ListarDocumentosInstitucionales listar,
    SubirDocumentoInstitucional subir,
    DescargarDocumentoInstitucional descargar,
    EliminarDocumentoInstitucional eliminar) : ControllerBase
{
    const string RolesLectura = "ROL-01,ROL-02,ROL-03,ROL-06";
    const string RolesAdministracion = "ROL-01,ROL-06";

    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── GET /api/documentos-institucionales ───────────────────────────────
    [HttpGet]
    [Authorize(Roles = RolesLectura)]
    public async Task<IActionResult> Listar() => Ok(await listar.EjecutarAsync());

    // ─── POST /api/documentos-institucionales ──────────────────────────────
    [HttpPost]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
    [Authorize(Roles = RolesAdministracion)]
    public async Task<IActionResult> Subir([FromForm] string nombre, IFormFile archivo)
    {
        if (archivo is null)
            return BadRequest(new { error = "No se recibió ningún archivo." });

        await using var stream = archivo.OpenReadStream();
        var resultado = await subir.EjecutarAsync(
            nombre, archivo.FileName, archivo.ContentType, archivo.Length, stream, UsuarioActual, HttpContext.RequestAborted);

        if (!resultado.Ok) return BadRequest(new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── GET /api/documentos-institucionales/{id} ──────────────────────────
    [HttpGet("{id:int}")]
    [Authorize(Roles = RolesLectura)]
    public async Task<IActionResult> Descargar(int id)
    {
        var resultado = await descargar.EjecutarAsync(id);
        if (!resultado.Ok) return NotFound(new { error = resultado.Error });

        var d = resultado.Valor!;
        return File(d.Contenido, d.TipoMime, d.NombreOriginal);
    }

    // ─── DELETE /api/documentos-institucionales/{id} ───────────────────────
    [HttpDelete("{id:int}")]
    [Authorize(Roles = RolesAdministracion)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var ok = await eliminar.EjecutarAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}
