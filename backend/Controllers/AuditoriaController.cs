using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Auditoria;

namespace Ssal.Api.Controllers;

// Registro de accesos y acciones críticas (RF-30). Lo consultan el Administrador general y el Administrador de sistema.
[ApiController]
[Route("api/auditoria")]
[Authorize(Roles = "ROL-01,ROL-06")]
public class AuditoriaController(ListarAuditoria listar) : ControllerBase
{
    // ─── GET /api/auditoria ───────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? accion,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 50) =>
        Ok(await listar.EjecutarAsync(busqueda, accion, pagina, porPagina));
}
