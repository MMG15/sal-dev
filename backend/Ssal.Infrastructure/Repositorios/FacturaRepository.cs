using Microsoft.EntityFrameworkCore;
using Ssal.Application.Facturas;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class FacturaRepository(SsalDbContext db) : IFacturaRepository
{
    public async Task<(int Total, List<FacturaResumenDto> Items)> BuscarAsync(string? busqueda, string? estado, int pagina, int porPagina)
    {
        var q = db.Facturas
            .Include(f => f.Sse).ThenInclude(s => s.Cliente)
            .Include(f => f.Sse).ThenInclude(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .Include(f => f.Sse).ThenInclude(s => s.RotulosInternos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(f => f.Estado == estado);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(f =>
                (f.Numero != null && f.Numero.ToLower().Contains(b)) ||
                (f.Sse.Codigo != null && f.Sse.Codigo.ToLower().Contains(b)) ||
                (f.Sse.Cliente != null && (f.Sse.Cliente.Nombre + " " + f.Sse.Cliente.Apellido).ToLower().Contains(b)) ||
                f.Sse.RotulosInternos.Any(r => r.NumeroUnico.ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderBy(f => f.Sse.Cliente!.Apellido)
            .ThenBy(f => f.Sse.Cliente!.Nombre)
            .ThenByDescending(f => f.Fecha)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(f => new FacturaResumenDto(
                f.IdFactura, f.Numero, f.Estado, f.Fecha, f.FechaPago, f.ImportePesos, f.ImporteDolares,
                new SseResumenDto(f.Sse.IdSse, f.Sse.Codigo, f.Sse.Area),
                f.Sse.Cliente == null ? null : new ClienteNombreDto(f.Sse.Cliente.Nombre, f.Sse.Cliente.Apellido),
                f.Sse.Presupuesto == null || f.Sse.Presupuesto.Consulta.Empresa == null
                    ? null : new EmpresaNombreDto(f.Sse.Presupuesto.Consulta.Empresa.RazonSocial),
                f.Sse.RotulosInternos.Count == 0 ? null : string.Join(", ", f.Sse.RotulosInternos.OrderBy(r => r.NumeroUnico).Select(r => r.NumeroUnico))))
            .ToListAsync();

        return (total, items);
    }

    public async Task<List<SseDisponibleDto>> ListarSsesSinFacturaAsync() =>
        await db.Sses
            .Where(s => !db.Facturas.Any(f => f.IdSse == s.IdSse && f.Estado != "anulada"))
            .Include(s => s.Cliente)
            .Include(s => s.Presupuesto).ThenInclude(p => p!.Consulta).ThenInclude(c => c.Empresa)
            .Include(s => s.RotulosInternos)
            .OrderByDescending(s => s.Fecha)
            .Select(s => new SseDisponibleDto(
                s.IdSse, s.Codigo, s.Area, s.Fecha, s.ImportePesos, s.ImporteDolares,
                s.Cliente == null ? null : new ClienteNombreDto(s.Cliente.Nombre, s.Cliente.Apellido),
                s.Presupuesto == null || s.Presupuesto.Consulta.Empresa == null
                    ? null : new EmpresaNombreDto(s.Presupuesto.Consulta.Empresa.RazonSocial),
                s.RotulosInternos.Count == 0 ? null : string.Join(", ", s.RotulosInternos.OrderBy(r => r.NumeroUnico).Select(r => r.NumeroUnico))))
            .ToListAsync();

    public Task<Sse?> ObtenerSseAsync(int idSse) => db.Sses.FindAsync(idSse).AsTask();

    public Task<bool> SseTieneFacturaActivaAsync(int idSse) =>
        db.Facturas.AnyAsync(f => f.IdSse == idSse && f.Estado != "anulada");

    public Task<bool> ExisteNumeroAsync(string numero) => db.Facturas.AnyAsync(f => f.Numero == numero);

    public void Agregar(Factura factura) => db.Facturas.Add(factura);

    public Task<Factura?> ObtenerAsync(int id) => db.Facturas.FindAsync(id).AsTask();

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
