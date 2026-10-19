using Microsoft.EntityFrameworkCore;
using Ssal.Application.Sse;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class SseRepository(SsalDbContext db) : ISseRepository
{
    public async Task<(int Total, List<SseResumenDto> Items)> BuscarAsync(string? busqueda, string? estado, string? area, int pagina, int porPagina)
    {
        var q = db.Sses
            .Include(s => s.Cliente)
            .Include(s => s.Empresa)
            .Include(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .Include(s => s.RotuloInterno)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(s => s.Estado == estado);

        if (!string.IsNullOrWhiteSpace(area))
            q = q.Where(s => s.Area == area);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(s =>
                (s.Codigo != null && s.Codigo.ToLower().Contains(b)) ||
                (s.CodigoRpo != null && s.CodigoRpo.ToLower().Contains(b)) ||
                (s.Cliente != null && (s.Cliente.Nombre + " " + s.Cliente.Apellido).ToLower().Contains(b)) ||
                (s.RotuloInterno != null && s.RotuloInterno.NumeroUnico.ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(s => s.Fecha)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(s => new SseResumenDto(
                s.IdSse, s.Codigo, s.CodigoRpo, s.Area, s.Estado, s.Fecha, s.FormaPago, s.ImporteDolares, s.ImportePesos,
                s.Cliente == null ? null : new ClienteResumenDto(s.Cliente.Nombre, s.Cliente.Apellido, s.Cliente.Email),
                s.Empresa != null ? new EmpresaResumenDto(s.Empresa.RazonSocial)
                    : s.Presupuesto != null && s.Presupuesto.Consulta.Empresa != null ? new EmpresaResumenDto(s.Presupuesto.Consulta.Empresa.RazonSocial)
                    : null,
                s.RotuloInterno == null ? null : new RotuloResumenDto(s.RotuloInterno.IdRotulo, s.RotuloInterno.NumeroUnico, s.RotuloInterno.Estado)))
            .ToListAsync();

        return (total, items);
    }

    public Task<Sse?> ObtenerDetalleAsync(int id) =>
        db.Sses
            .Include(x => x.Cliente).ThenInclude(c => c!.Empresa)
            .Include(x => x.Empresa)
            .Include(x => x.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Cliente)
            .Include(x => x.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .Include(x => x.Presupuesto).ThenInclude(p => p!.Items).ThenInclude(i => i.Analisis)
            .Include(x => x.CreadoPorUsuario)
            .Include(x => x.RotuloInterno).ThenInclude(r => r!.AsignadoPorUsuario)
            .Include(x => x.RotuloInterno).ThenInclude(r => r!.Muestra).ThenInclude(m => m!.RecibidoPorUsuario)
            .Include(x => x.RotuloInterno).ThenInclude(r => r!.Auditorias).ThenInclude(a => a.Usuario)
            .FirstOrDefaultAsync(x => x.IdSse == id);

    public Task<Sse?> ObtenerCompletoParaPdfAsync(int id) =>
        db.Sses
            .Include(x => x.Cliente).ThenInclude(c => c!.Empresa)
            .Include(x => x.Empresa)
            .Include(x => x.Presupuesto).ThenInclude(p => p!.Items).ThenInclude(i => i.Analisis)
            .Include(x => x.CreadoPorUsuario)
            .Include(x => x.RotuloInterno).ThenInclude(r => r!.Muestra)
            .FirstOrDefaultAsync(x => x.IdSse == id);

    public Task<Sse?> ObtenerAsync(int id) => db.Sses.FindAsync(id).AsTask();

    public Task<Sse?> ObtenerConRotuloAsync(int id) =>
        db.Sses.Include(s => s.RotuloInterno).FirstOrDefaultAsync(s => s.IdSse == id);

    public Task<bool> ExisteClienteAsync(int id) => db.Clientes.AnyAsync(c => c.IdCliente == id);

    public Task<bool> ExisteEmpresaAsync(int id) => db.Empresas.AnyAsync(e => e.IdEmpresa == id);

    public Task<string?> ObtenerCodigoRpoAsync(int idPresupuesto) =>
        db.Presupuestos.Where(p => p.IdPresupuesto == idPresupuesto).Select(p => p.CodigoRpo).FirstOrDefaultAsync();

    public Task<string?> ObtenerUltimoCodigoAsync(string prefijo) =>
        db.Sses
            .Where(s => s.Codigo != null && s.Codigo.StartsWith(prefijo))
            .OrderByDescending(s => s.Codigo)
            .Select(s => s.Codigo)
            .FirstOrDefaultAsync();

    public void Agregar(Sse sse) => db.Sses.Add(sse);

    public Task<bool> ExisteNumeroRotuloAsync(string numeroUnico) =>
        db.RotulosInternos.AnyAsync(r => r.NumeroUnico == numeroUnico);

    public Task<string?> ObtenerUltimoNumeroRotuloAsync(string prefijo) =>
        db.RotulosInternos
            .Where(r => r.NumeroUnico.StartsWith(prefijo))
            .OrderByDescending(r => r.NumeroUnico)
            .Select(r => r.NumeroUnico)
            .FirstOrDefaultAsync();

    public void AgregarRotulo(RotuloInterno rotulo) => db.RotulosInternos.Add(rotulo);

    public Task<RotuloInterno?> ObtenerRotuloConMuestraAsync(int idSse) =>
        db.RotulosInternos.Include(r => r.Muestra).FirstOrDefaultAsync(r => r.IdSse == idSse);

    public void AgregarMuestra(Muestra muestra) => db.Muestras.Add(muestra);

    public Task<Sse?> ObtenerConPresupuestoItemsAsync(int id) =>
        db.Sses.Include(s => s.Presupuesto).ThenInclude(p => p!.Items).FirstOrDefaultAsync(s => s.IdSse == id);

    public void AgregarResultado(Resultado resultado) => db.Resultados.Add(resultado);

    public Task<RotuloInterno?> ObtenerRotuloPorSseAsync(int idSse) =>
        db.RotulosInternos.FirstOrDefaultAsync(r => r.IdSse == idSse);

    public Task<bool> ExisteOtroRotuloConNumeroAsync(int idRotulo, string numeroUnico) =>
        db.RotulosInternos.AnyAsync(r => r.IdRotulo != idRotulo && r.NumeroUnico == numeroUnico);

    public void AgregarAuditoriasRotulo(IEnumerable<AuditoriaRotulo> cambios) => db.AuditoriasRotulo.AddRange(cambios);

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
