using Microsoft.EntityFrameworkCore;
using Ssal.Application.Clientes;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class ClienteRepository(SsalDbContext db) : IClienteRepository
{
    public async Task<List<Cliente>> BuscarConEmpresaAsync(string? busqueda)
    {
        var query = db.Clientes.Include(c => c.Empresa).AsQueryable();

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            query = query.Where(c =>
                c.Nombre.ToLower().Contains(b) ||
                c.Apellido.ToLower().Contains(b) ||
                (c.Email != null && c.Email.ToLower().Contains(b)));
        }

        return await query.ToListAsync();
    }

    public Task<Cliente?> ObtenerConEmpresaAsync(int id) =>
        db.Clientes.Include(c => c.Empresa).FirstOrDefaultAsync(c => c.IdCliente == id);

    public Task<Cliente?> ObtenerAsync(int id) => db.Clientes.FindAsync(id).AsTask();

    public Task<Cliente?> BuscarDuplicadoAsync(string? email, string? cuit, int? excluirId)
    {
        var emailNorm = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLower();
        var cuitNorm = string.IsNullOrWhiteSpace(cuit) ? null : cuit.Trim();
        if (emailNorm is null && cuitNorm is null) return Task.FromResult<Cliente?>(null);

        return db.Clientes.FirstOrDefaultAsync(c =>
            c.IdCliente != (excluirId ?? -1) &&
            ((emailNorm != null && c.Email != null && c.Email.ToLower() == emailNorm) ||
             (cuitNorm != null && c.Cuit != null && c.Cuit == cuitNorm)));
    }

    public void Agregar(Cliente cliente) => db.Clientes.Add(cliente);

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
