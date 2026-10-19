using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Empresas;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/empresas")]
[Authorize(Roles = "ROL-01,ROL-02,ROL-07")]
public class EmpresasController(
    ListarEmpresas listar,
    ObtenerEmpresa obtener,
    CrearEmpresa crear,
    ActualizarEmpresa actualizar) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? busqueda) => Ok(await listar.EjecutarAsync(busqueda));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var empresa = await obtener.EjecutarAsync(id);
        if (empresa is null) return NotFound();
        return Ok(empresa);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] GuardarEmpresaCommand req)
    {
        var resultado = await crear.EjecutarAsync(req);
        if (!resultado.Ok) return Conflict(new { error = resultado.Error });
        return CreatedAtAction(nameof(Obtener), new { id = resultado.Valor!.IdEmpresa }, resultado.Valor);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarEmpresaCommand req)
    {
        var resultado = await actualizar.EjecutarAsync(id, req);
        if (!resultado.Ok)
            return resultado.Error == "No encontrada" ? NotFound() : Conflict(new { error = resultado.Error });
        return NoContent();
    }
}
