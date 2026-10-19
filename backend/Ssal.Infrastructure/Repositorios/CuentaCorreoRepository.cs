using Microsoft.EntityFrameworkCore;
using Ssal.Application.CuentasCorreo;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class CuentaCorreoRepository(SsalDbContext db) : ICuentaCorreoRepository
{
    public async Task<List<CuentaCorreoDto>> ListarAsync() =>
        await db.CuentasCorreo
            .OrderBy(c => c.Nombre)
            .Select(c => new CuentaCorreoDto(
                c.IdCuenta, c.Nombre, c.Servidor, c.Puerto, c.Usuario,
                c.Habilitada, c.IntervaloMinutos, c.UltimaRevision, c.UltimoError))
            .ToListAsync();

    public void Agregar(CuentaCorreo cuenta) => db.CuentasCorreo.Add(cuenta);

    public Task<CuentaCorreo?> ObtenerAsync(int id) => db.CuentasCorreo.FindAsync(id).AsTask();

    public Task<bool> ExisteAsync(int id) => db.CuentasCorreo.AnyAsync(c => c.IdCuenta == id);

    public void Eliminar(CuentaCorreo cuenta) => db.CuentasCorreo.Remove(cuenta);

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
