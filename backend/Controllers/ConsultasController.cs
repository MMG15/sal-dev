using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Consultas;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/consultas")]
[Authorize(Roles = "ROL-01,ROL-02,ROL-03,ROL-07")]
public class ConsultasController(
    ListarConsultas listar,
    ObtenerConsulta obtener,
    CrearConsulta crear,
    ActualizarConsulta actualizar) : ControllerBase
{
    // ─── GET /api/consultas ───────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] string? busqueda,
        [FromQuery] string? estado,
        [FromQuery] int pagina = 1,
        [FromQuery] int porPagina = 20) =>
        Ok(await listar.EjecutarAsync(busqueda, estado, pagina, porPagina));

    // ─── GET /api/consultas/{id} ──────────────────────────────────────────
    [HttpGet("{id}")]
    public async Task<IActionResult> Detalle(int id)
    {
        var consulta = await obtener.EjecutarAsync(id);
        if (consulta is null) return NotFound();
        return Ok(consulta);
    }

    // ─── POST /api/consultas ──────────────────────────────────────────────
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearConsultaCommand req)
    {
        var (idConsulta, codigo) = await crear.EjecutarAsync(req);
        return CreatedAtAction(nameof(Detalle), new { id = idConsulta }, new { IdConsulta = idConsulta, Codigo = codigo });
    }

    // ─── PUT /api/consultas/{id} ──────────────────────────────────────────
    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarConsultaCommand req)
    {
        var ok = await actualizar.EjecutarAsync(id, req);
        if (!ok) return NotFound();
        return NoContent();
    }
}
