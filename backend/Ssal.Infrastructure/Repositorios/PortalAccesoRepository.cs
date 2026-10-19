using Microsoft.EntityFrameworkCore;
using Ssal.Application.PortalAccesos;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class PortalAccesoRepository(SsalDbContext db) : IPortalAccesoRepository
{
    public Task<PortalUsuario?> ObtenerPorClienteAsync(int idCliente) =>
        db.PortalUsuarios.FirstOrDefaultAsync(p => p.IdCliente == idCliente);

    public Task<Cliente?> ObtenerClienteAsync(int idCliente) => db.Clientes.FindAsync(idCliente).AsTask();

    public Task<bool> EmailEnUsoPorOtroClienteAsync(string email, int idCliente) =>
        db.PortalUsuarios.AnyAsync(p => p.Email == email && p.IdCliente != idCliente);

    public void Agregar(PortalUsuario acceso) => db.PortalUsuarios.Add(acceso);

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
