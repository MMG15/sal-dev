using Microsoft.EntityFrameworkCore;
using Ssal.Application.Usuarios;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class UsuarioRepository(SsalDbContext db) : IUsuarioRepository
{
    public async Task<List<Rol>> ListarRolesAsignablesAsync(string rolNoAsignable) =>
        await db.Roles.Where(r => r.Codigo != rolNoAsignable).ToListAsync();

    public async Task<List<Usuario>> BuscarAsync(string? busqueda, string? estado)
    {
        var q = db.Usuarios.Include(u => u.Rol).AsQueryable();

        if (estado == "activos") q = q.Where(u => u.Activo);
        if (estado == "inactivos") q = q.Where(u => !u.Activo);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(u => (u.Nombre + " " + u.Apellido).ToLower().Contains(b) || u.Email.ToLower().Contains(b));
        }

        return await q.ToListAsync();
    }

    public Task<Usuario?> ObtenerAsync(int id) => db.Usuarios.FindAsync(id).AsTask();

    public Task<Rol?> ObtenerRolAsync(int idRol) => db.Roles.FindAsync(idRol).AsTask();

    public Task<bool> ExisteEmailAsync(string email, int? excluirId) =>
        db.Usuarios.AnyAsync(u => u.Email == email && u.IdUsuario != (excluirId ?? -1));

    public void Agregar(Usuario usuario) => db.Usuarios.Add(usuario);

    public void AgregarAuditoria(AuditoriaSistema entrada) => db.Auditoria.Add(entrada);

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
