using Microsoft.EntityFrameworkCore;
using Ssal.Application.Firmas;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class FirmaRepository(SsalDbContext db) : IFirmaRepository
{
    public async Task<List<Informe>> ListarInformesAsync() =>
        await db.Informes
            .Include(i => i.Muestra).ThenInclude(m => m.Resultados).ThenInclude(r => r.Analisis)
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente)
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .OrderByDescending(i => i.FechaGeneracion)
            .Take(200)
            .ToListAsync();

    public async Task<List<Presupuesto>> ListarPresupuestosAsync() =>
        await db.Presupuestos
            .Include(p => p.Items).ThenInclude(i => i.Analisis)
            .Include(p => p.Consulta).ThenInclude(c => c.Cliente)
            .Include(p => p.Consulta).ThenInclude(c => c.Empresa)
            .OrderByDescending(p => p.Fecha)
            .Take(200)
            .ToListAsync();

    public async Task<List<FirmaDocumento>> ListarTodosLosEventosAsync() =>
        await db.FirmasDocumento.Include(f => f.Firmante).ToListAsync();

    public Task<Informe?> ObtenerInformeAsync(int id) =>
        db.Informes
            .Include(i => i.Muestra).ThenInclude(m => m.Resultados).ThenInclude(r => r.Analisis)
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente)
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .FirstOrDefaultAsync(i => i.IdInforme == id);

    public Task<Presupuesto?> ObtenerPresupuestoAsync(int id) =>
        db.Presupuestos
            .Include(p => p.Items).ThenInclude(i => i.Analisis)
            .Include(p => p.Consulta).ThenInclude(c => c.Cliente)
            .Include(p => p.Consulta).ThenInclude(c => c.Empresa)
            .FirstOrDefaultAsync(p => p.IdPresupuesto == id);

    public async Task<List<FirmaDocumento>> ObtenerEventosAsync(string tipoDocumento, int idDocumento) =>
        await db.FirmasDocumento
            .Include(f => f.Firmante)
            .Where(f => f.TipoDocumento == tipoDocumento && f.IdDocumento == idDocumento)
            .OrderBy(f => f.IdFirma)
            .ToListAsync();

    public void AgregarFirma(FirmaDocumento firma) => db.FirmasDocumento.Add(firma);

    public Task<Usuario?> ObtenerUsuarioAsync(int id) => db.Usuarios.FindAsync(id).AsTask();

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
