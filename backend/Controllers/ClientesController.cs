using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ssal.Application.Clientes;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize(Roles = "ROL-01,ROL-02,ROL-07")]
public class ClientesController(
    ListarClientes listar,
    ObtenerCliente obtener,
    CrearCliente crear,
    ActualizarCliente actualizar) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? busqueda) => Ok(await listar.EjecutarAsync(busqueda));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var cliente = await obtener.EjecutarAsync(id);
        if (cliente is null) return NotFound();
        return Ok(cliente);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] GuardarClienteCommand req)
    {
        var resultado = await crear.EjecutarAsync(req);
        if (!resultado.Ok) return Conflict(new { error = resultado.Error });
        return CreatedAtAction(nameof(Obtener), new { id = resultado.Valor!.IdCliente }, resultado.Valor);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarClienteCommand req)
    {
        var resultado = await actualizar.EjecutarAsync(id, req);
        if (!resultado.Ok)
            return resultado.Error == "No encontrado" ? NotFound() : Conflict(new { error = resultado.Error });
        return NoContent();
    }
}
