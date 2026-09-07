using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController(SsalDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? busqueda)
    {
        var query = db.Clientes
            .Include(c => c.Empresa)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            query = query.Where(c =>
                c.Nombre.ToLower().Contains(b) ||
                c.Apellido.ToLower().Contains(b) ||
                (c.Email != null && c.Email.ToLower().Contains(b)));
        }

        var clientes = await query
            .OrderBy(c => c.Apellido)
            .ThenBy(c => c.Nombre)
            .Select(c => new ClienteResponse(
                c.IdCliente,
                c.Nombre,
                c.Apellido,
                c.Email,
                c.Telefono,
                c.Cuit,
                c.CondicionIva,
                c.UsuarioWeb,
                c.Estado,
                c.IdEmpresa,
                c.Empresa != null ? c.Empresa.RazonSocial : null,
                c.CreatedAt
            ))
            .ToListAsync();

        return Ok(clientes);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var c = await db.Clientes
            .Include(c => c.Empresa)
            .FirstOrDefaultAsync(c => c.IdCliente == id);
        if (c is null) return NotFound();
        return Ok(new ClienteResponse(c.IdCliente, c.Nombre, c.Apellido, c.Email,
            c.Telefono, c.Cuit, c.CondicionIva, c.UsuarioWeb, c.Estado, c.IdEmpresa, c.Empresa?.RazonSocial, c.CreatedAt));
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ClienteRequest req)
    {
        var duplicado = await BuscarDuplicado(req.Email, req.Cuit, excluirId: null);
        if (duplicado is not null) return Conflict(new { error = MensajeDuplicado(duplicado) });

        var cliente = new Cliente
        {
            IdEmpresa = req.IdEmpresa,
            Nombre = req.Nombre,
            Apellido = req.Apellido,
            Email = req.Email,
            Telefono = req.Telefono,
            Cuit = req.Cuit,
            CondicionIva = req.CondicionIva,
            UsuarioWeb = req.UsuarioWeb
        };
        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obtener), new { id = cliente.IdCliente }, cliente);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ClienteRequest req)
    {
        var cliente = await db.Clientes.FindAsync(id);
        if (cliente is null) return NotFound();

        var duplicado = await BuscarDuplicado(req.Email, req.Cuit, excluirId: id);
        if (duplicado is not null) return Conflict(new { error = MensajeDuplicado(duplicado) });

        cliente.IdEmpresa = req.IdEmpresa;
        cliente.Nombre = req.Nombre;
        cliente.Apellido = req.Apellido;
        cliente.Email = req.Email;
        cliente.Telefono = req.Telefono;
        cliente.Cuit = req.Cuit;
        cliente.CondicionIva = req.CondicionIva;
        cliente.UsuarioWeb = req.UsuarioWeb;
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Busca un cliente existente con el mismo email o CUIT/DNI (comparación insensible a mayúsculas/espacios).</summary>
    private async Task<Cliente?> BuscarDuplicado(string? email, string? cuit, int? excluirId)
    {
        var emailNorm = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLower();
        var cuitNorm = string.IsNullOrWhiteSpace(cuit) ? null : cuit.Trim();
        if (emailNorm is null && cuitNorm is null) return null;

        return await db.Clientes.FirstOrDefaultAsync(c =>
            c.IdCliente != (excluirId ?? -1) &&
            ((emailNorm != null && c.Email != null && c.Email.ToLower() == emailNorm) ||
             (cuitNorm != null && c.Cuit != null && c.Cuit == cuitNorm)));
    }

    private static string MensajeDuplicado(Cliente existente) =>
        $"Ya existe un cliente con esos datos: {existente.Nombre} {existente.Apellido} (ID {existente.IdCliente}).";
}

public record ClienteRequest(
    int? IdEmpresa,
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Cuit,
    string? CondicionIva,
    string? UsuarioWeb
);

public record ClienteResponse(
    int IdCliente,
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Cuit,
    string? CondicionIva,
    string? UsuarioWeb,
    string Estado,
    int? IdEmpresa,
    string? NombreEmpresa,
    DateTime CreatedAt
);
