using Microsoft.EntityFrameworkCore;
using Ssal.Application.Auth;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class AuthRepository(SsalDbContext db) : IAuthRepository
{
    public Task<Usuario?> ObtenerActivoPorEmailAsync(string email) =>
        db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == email && u.Activo);

    public Task<Usuario?> ObtenerAsync(int id) => db.Usuarios.FindAsync(id).AsTask();

    public void AgregarAuditoria(AuditoriaSistema entrada) => db.Auditoria.Add(entrada);

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
