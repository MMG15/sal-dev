using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Presupuestos;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/presupuestos")]
[Authorize]
public class PresupuestosController(
    ListarPresupuestos listar,
    ObtenerPresupuesto obtener,
    CrearPresupuesto crear,
    DescargarPresupuestoPdf descargarPdf,
    EnviarPresupuesto enviar,
    CambiarEstadoPresupuesto cambiarEstado,
    ListarArchivosPresupuesto listarArchivos,
    SubirArchivoPresupuesto subirArchivo,
    DescargarArchivoPresupuesto descargarArchivo,
    EliminarArchivoPresupuesto eliminarArchivo) : ControllerBase
{
    // Roles con permiso de gestión (crear/editar/archivar) de presupuestos.
    private const string RolesGestion = "ROL-01,ROL-03";
    // Roles con permiso de lectura — se suma ROL-02 porque desde SSE (Administrativo)
    // hace falta poder listar y consultar presupuestos aceptados para vincularlos.
    private const string RolesLectura = "ROL-01,ROL-02,ROL-03";

    int UsuarioActual => int.Parse(
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

    // ─── POST /api/presupuestos ───────────────────────────────────────────
    // Crea o reemplaza el presupuesto de una consulta
    [HttpPost]
    [Authorize(Roles = RolesGestion)]
    public async Task<IActionResult> Crear([FromBody] GuardarPresupuestoCommand req)
    {
        var resultado = await crear.EjecutarAsync(req, UsuarioActual);
        if (!resultado.Ok) return BadRequest(new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── GET /api/presupuestos ────────────────────────────────────────────
    [HttpGet]
    [Authorize(Roles = RolesLectura)]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20) =>
        Ok(await listar.EjecutarAsync(busqueda, estado, pagina, porPagina));

    // ─── GET /api/presupuestos/{id} ───────────────────────────────────────
    [HttpGet("{id}")]
    [Authorize(Roles = RolesLectura)]
    public async Task<IActionResult> Detalle(int id)
    {
        var p = await obtener.EjecutarAsync(id);
        if (p is null) return NotFound();
        return Ok(p);
    }

    // ─── GET /api/presupuestos/{id}/pdf ──────────────────────────────────
    [HttpGet("{id}/pdf")]
    [Authorize(Roles = RolesLectura)]
    public async Task<IActionResult> DescargarPdf(int id)
    {
        var pdf = await descargarPdf.EjecutarAsync(id);
        if (pdf is null) return NotFound();
        return File(pdf.Contenido, "application/pdf", pdf.NombreArchivo);
    }

    // ─── POST /api/presupuestos/{id}/enviar ──────────────────────────────
    [HttpPost("{id:int}/enviar")]
    [Authorize(Roles = RolesGestion)]
    public async Task<IActionResult> Enviar(int id, [FromBody] EnviarPresupuestoCommand req)
    {
        var resultado = await enviar.EjecutarAsync(id, req, HttpContext.RequestAborted);
        if (!resultado.Ok)
            return resultado.Error switch
            {
                "No encontrado" => NotFound(),
                string e when e.StartsWith("No se pudo enviar el presupuesto por email") => StatusCode(StatusCodes.Status502BadGateway, new { error = e }),
                _ => BadRequest(new { error = resultado.Error })
            };
        return Ok(new { ok = true });
    }

    // ─── PUT /api/presupuestos/{id}/estado ───────────────────────────────
    [HttpPut("{id}/estado")]
    [Authorize(Roles = RolesGestion)]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoPresupuestoCommand req)
    {
        var ok = await cambiarEstado.EjecutarAsync(id, req);
        if (!ok) return NotFound();
        return NoContent();
    }

    // ─── GET /api/presupuestos/{id}/archivos ─────────────────────────────
    [HttpGet("{id}/archivos")]
    [Authorize(Roles = RolesLectura)]
    public async Task<IActionResult> ListarArchivos(int id) => Ok(await listarArchivos.EjecutarAsync(id));

    // ─── POST /api/presupuestos/{id}/archivos ────────────────────────────
    [HttpPost("{id}/archivos")]
    [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
    [Authorize(Roles = RolesGestion)]
    public async Task<IActionResult> SubirArchivo(int id, IFormFile archivo)
    {
        if (archivo is null)
            return BadRequest(new { error = "No se recibió ningún archivo." });

        await using var stream = archivo.OpenReadStream();
        var resultado = await subirArchivo.EjecutarAsync(
            id, archivo.FileName, archivo.ContentType, archivo.Length, stream, UsuarioActual, HttpContext.RequestAborted);

        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : BadRequest(new { error = resultado.Error });
        return Ok(resultado.Valor);
    }

    // ─── GET /api/presupuestos/{id}/archivos/{archivoId} ─────────────────
    [HttpGet("{id}/archivos/{archivoId}")]
    [Authorize(Roles = RolesLectura)]
    public async Task<IActionResult> DescargarArchivo(int id, int archivoId)
    {
        var resultado = await descargarArchivo.EjecutarAsync(id, archivoId);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : NotFound(new { error = resultado.Error });

        var a = resultado.Valor!;
        return File(a.Contenido, a.TipoMime, a.NombreOriginal);
    }

    // ─── DELETE /api/presupuestos/{id}/archivos/{archivoId} ──────────────
    [HttpDelete("{id}/archivos/{archivoId}")]
    [Authorize(Roles = RolesGestion)]
    public async Task<IActionResult> EliminarArchivo(int id, int archivoId)
    {
        var ok = await eliminarArchivo.EjecutarAsync(id, archivoId);
        if (!ok) return NotFound();
        return NoContent();
    }
}
