using Microsoft.EntityFrameworkCore;
using Ssal.Application.Resultados;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class ResultadoRepository(SsalDbContext db) : IResultadoRepository
{
    public async Task<List<Muestra>> BuscarMuestrasAsync(string? busqueda, string? area)
    {
        var q = db.Muestras
            .Include(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente!.Empresa)
            .Include(m => m.Resultados).ThenInclude(r => r.Analisis)
            .Where(m => m.Estado != "rechazada" && m.Resultados.Any())
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(area))
            q = q.Where(m => m.Resultados.Any(r => r.Analisis.Area == area));

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(m =>
                m.RotuloInterno.NumeroUnico.ToLower().Contains(b) ||
                (m.RotuloInterno.Sse.Codigo != null && m.RotuloInterno.Sse.Codigo.ToLower().Contains(b)) ||
                (m.RotuloInterno.Sse.Cliente != null && (m.RotuloInterno.Sse.Cliente.Nombre + " " + m.RotuloInterno.Sse.Cliente.Apellido).ToLower().Contains(b)) ||
                (m.RotuloInterno.Sse.Cliente != null && m.RotuloInterno.Sse.Cliente.Empresa != null && m.RotuloInterno.Sse.Cliente.Empresa.RazonSocial.ToLower().Contains(b)));
        }

        return await q.ToListAsync();
    }

    public Task<Muestra?> ObtenerDetalleAsync(int id) =>
        db.Muestras
            .Include(x => x.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente!.Empresa)
            .Include(x => x.Resultados).ThenInclude(r => r.Analisis).ThenInclude(a => a.Grupo)
            .Include(x => x.Resultados).ThenInclude(r => r.CargadoPorUsuario)
            .Include(x => x.Resultados).ThenInclude(r => r.ValidadoPorUsuario)
            .Include(x => x.Informe)
            .FirstOrDefaultAsync(x => x.IdMuestra == id);

    public Task<Muestra?> ObtenerParaInformeAsync(int id) =>
        db.Muestras.Include(x => x.Resultados).Include(x => x.Informe).FirstOrDefaultAsync(x => x.IdMuestra == id);

    public Task<string?> ObtenerUltimoCodigoInformeAsync(string prefijo) =>
        db.Informes
            .Where(i => i.Codigo.StartsWith(prefijo))
            .OrderByDescending(i => i.Codigo)
            .Select(i => i.Codigo)
            .FirstOrDefaultAsync();

    public void AgregarInforme(Informe informe) => db.Informes.Add(informe);

    public Task<Informe?> ObtenerInformeCompletoAsync(int id) =>
        db.Informes
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Cliente!.Empresa)
            .Include(i => i.Muestra).ThenInclude(m => m.RotuloInterno).ThenInclude(r => r.Sse).ThenInclude(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .Include(i => i.Muestra).ThenInclude(m => m.Resultados).ThenInclude(r => r.Analisis).ThenInclude(a => a.Grupo)
            .Include(i => i.GeneradoPorUsuario)
            .FirstOrDefaultAsync(i => i.IdInforme == id);

    public async Task<List<FirmaDocumento>> ObtenerEventosFirmaAsync(string tipoDocumento, int idDocumento) =>
        await db.FirmasDocumento
            .Include(f => f.Firmante)
            .Where(f => f.TipoDocumento == tipoDocumento && f.IdDocumento == idDocumento)
            .OrderBy(f => f.IdFirma)
            .ToListAsync();

    public Task<Resultado?> ObtenerConAnalisisAsync(int id) =>
        db.Resultados.Include(x => x.Analisis).FirstOrDefaultAsync(x => x.IdResultado == id);

    public Task<Resultado?> ObtenerAsync(int id) => db.Resultados.FirstOrDefaultAsync(x => x.IdResultado == id);

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
