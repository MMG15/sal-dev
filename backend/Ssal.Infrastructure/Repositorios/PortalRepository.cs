using Microsoft.EntityFrameworkCore;
using Ssal.Application.Portal;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class PortalRepository(SsalDbContext db) : IPortalRepository
{
    public Task<PortalUsuario?> ObtenerCuentaPorEmailAsync(string email) =>
        db.PortalUsuarios.Include(p => p.Cliente).FirstOrDefaultAsync(p => p.Email == email && p.Activo);

    public Task<PortalUsuario?> ObtenerCuentaAsync(int id) => db.PortalUsuarios.FindAsync(id).AsTask();

    public async Task<List<Muestra>> ListarMuestrasDelClienteAsync(int idCliente)
    {
        var sseIds = SsesDelCliente(idCliente).Select(s => s.IdSse);

        return await db.Muestras
            .Include(m => m.RotuloInterno).ThenInclude(r => r.Sse)
            .Include(m => m.Informe)
            .Include(m => m.Resultados).ThenInclude(r => r.Analisis)
            .Where(m => sseIds.Contains(m.RotuloInterno.IdSse))
            .OrderByDescending(m => m.FechaRecepcion)
            .ToListAsync();
    }

    public async Task<List<FirmaDocumento>> ObtenerFirmasDeInformesAsync(IEnumerable<int> idsInformes)
    {
        var ids = idsInformes.ToList();
        // Incluye Firmante: lo necesita el PDF (muestra "Firmado por fulano") aunque
        // Pedidos/Detalle no lo usen — no cambia el resultado para esos otros casos.
        return await db.FirmasDocumento
            .Include(f => f.Firmante)
            .Where(f => f.TipoDocumento == "informe" && ids.Contains(f.IdDocumento))
            .OrderBy(f => f.IdFirma)
            .ToListAsync();
    }

    public Task<Informe?> ObtenerInformeParaDetalleAsync(int id) =>
        db.Informes
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse)
            .Include(i => i.Muestra).ThenInclude(m => m.Resultados).ThenInclude(r => r.Analisis)
            .FirstOrDefaultAsync(i => i.IdInforme == id);

    public Task<Informe?> ObtenerInformeCompletoAsync(int id) =>
        db.Informes
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente!.Empresa)
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .Include(i => i.Muestra).ThenInclude(m => m.Resultados).ThenInclude(r => r.Analisis).ThenInclude(a => a.Grupo)
            .Include(i => i.GeneradoPorUsuario)
            .FirstOrDefaultAsync(i => i.IdInforme == id);

    public Task<bool> PerteneceAlClienteAsync(int idSse, int idCliente) =>
        SsesDelCliente(idCliente).AnyAsync(s => s.IdSse == idSse);

    // SSE del cliente: por cliente asignado a la SSE o por la consulta del presupuesto
    private IQueryable<Sse> SsesDelCliente(int idCliente) =>
        db.Sses.Where(s => s.IdCliente == idCliente
            || (s.Presupuesto != null && s.Presupuesto.Consulta.IdCliente == idCliente));

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
