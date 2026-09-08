using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ssal.Api.Data;
using Ssal.Api.Models;

namespace Ssal.Api.Controllers;

[ApiController]
[Route("api/empresas")]
[Authorize(Roles = "ROL-01,ROL-02,ROL-07")]
public class EmpresasController(SsalDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] string? busqueda)
    {
        var query = db.Empresas.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            query = query.Where(e =>
                e.RazonSocial.ToLower().Contains(b) ||
                (e.Cuit != null && e.Cuit.Contains(b)));
        }

        var empresas = await query
            .OrderBy(e => e.RazonSocial)
            .Select(e => new EmpresaResponse(
                e.IdEmpresa,
                e.RazonSocial,
                e.Cuit,
                e.CondicionIva,
                e.Email,
                e.Telefono,
                e.Direccion,
                e.Estado,
                e.Clientes.Count(c => c.Estado == "activo")
            ))
            .ToListAsync();

        return Ok(empresas);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var empresa = await db.Empresas
            .Include(e => e.Clientes)
            .FirstOrDefaultAsync(e => e.IdEmpresa == id);
        if (empresa is null) return NotFound();
        return Ok(empresa);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] EmpresaRequest req)
    {
        var duplicada = await BuscarDuplicada(req.Cuit, excluirId: null);
        if (duplicada is not null) return Conflict(new { error = MensajeDuplicado(duplicada) });

        var empresa = new Empresa
        {
            RazonSocial = req.RazonSocial,
            Cuit = req.Cuit,
            CondicionIva = req.CondicionIva,
            Email = req.Email,
            Telefono = req.Telefono,
            Direccion = req.Direccion
        };
        db.Empresas.Add(empresa);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(Obtener), new { id = empresa.IdEmpresa },
            new EmpresaResponse(empresa.IdEmpresa, empresa.RazonSocial, empresa.Cuit, empresa.CondicionIva,
                empresa.Email, empresa.Telefono, empresa.Direccion, empresa.Estado, 0));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] EmpresaRequest req)
    {
        var empresa = await db.Empresas.FindAsync(id);
        if (empresa is null) return NotFound();

        var duplicada = await BuscarDuplicada(req.Cuit, excluirId: id);
        if (duplicada is not null) return Conflict(new { error = MensajeDuplicado(duplicada) });

        empresa.RazonSocial = req.RazonSocial;
        empresa.Cuit = req.Cuit;
        empresa.CondicionIva = req.CondicionIva;
        empresa.Email = req.Email;
        empresa.Telefono = req.Telefono;
        empresa.Direccion = req.Direccion;
        empresa.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Busca una empresa existente con el mismo CUIT (comparación insensible a espacios).</summary>
    private async Task<Empresa?> BuscarDuplicada(string? cuit, int? excluirId)
    {
        var cuitNorm = string.IsNullOrWhiteSpace(cuit) ? null : cuit.Trim();
        if (cuitNorm is null) return null;

        return await db.Empresas.FirstOrDefaultAsync(e =>
            e.IdEmpresa != (excluirId ?? -1) && e.Cuit != null && e.Cuit == cuitNorm);
    }

    private static string MensajeDuplicado(Empresa existente) =>
        $"Ya existe una empresa con ese CUIT: {existente.RazonSocial} (ID {existente.IdEmpresa}).";
}

public record EmpresaRequest(
    string RazonSocial,
    string? Cuit,
    string? CondicionIva,
    string? Email,
    string? Telefono,
    string? Direccion
);

public record EmpresaResponse(
    int IdEmpresa,
    string RazonSocial,
    string? Cuit,
    string? CondicionIva,
    string? Email,
    string? Telefono,
    string? Direccion,
    string Estado,
    int CantidadClientes
);
