using Microsoft.EntityFrameworkCore;
using Ssal.Application.Empresas;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class EmpresaRepository(SsalDbContext db) : IEmpresaRepository
{
    public async Task<List<Empresa>> BuscarConClientesAsync(string? busqueda)
    {
        var query = db.Empresas.Include(e => e.Clientes).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            query = query.Where(e => e.RazonSocial.ToLower().Contains(b) || (e.Cuit != null && e.Cuit.Contains(b)));
        }

        return await query.ToListAsync();
    }

    public Task<Empresa?> ObtenerConClientesAsync(int id) =>
        db.Empresas.Include(e => e.Clientes).FirstOrDefaultAsync(e => e.IdEmpresa == id);

    public Task<Empresa?> ObtenerAsync(int id) => db.Empresas.FindAsync(id).AsTask();

    public Task<Empresa?> BuscarDuplicadaPorCuitAsync(string cuit, int? excluirId) =>
        db.Empresas.FirstOrDefaultAsync(e => e.IdEmpresa != (excluirId ?? -1) && e.Cuit != null && e.Cuit == cuit);

    public void Agregar(Empresa empresa) => db.Empresas.Add(empresa);

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
