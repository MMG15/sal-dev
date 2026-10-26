using Microsoft.EntityFrameworkCore;
using Ssal.Application.Consultas;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class ConsultaRepository(SsalDbContext db) : IConsultaRepository
{
    public async Task<(int Total, List<Consulta> Items)> BuscarAsync(
        string? busqueda, string? estado, DateTime? limitePendiente, int pagina, int porPagina)
    {
        var q = db.Consultas
            .Include(c => c.Cliente)
            .Include(c => c.Empresa)
            .Include(c => c.Responsable)
            .Include(c => c.Presupuesto)
            .AsQueryable();

        if (limitePendiente.HasValue)
            q = q.Where(c => c.Estado == "recibida" && c.CreatedAt <= limitePendiente.Value);
        else if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(c => c.Estado == estado);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(c =>
                (c.Codigo != null && c.Codigo.ToLower().Contains(b)) ||
                (c.Cliente != null && (c.Cliente.Nombre + " " + c.Cliente.Apellido).ToLower().Contains(b)) ||
                (c.Empresa != null && c.Empresa.RazonSocial.ToLower().Contains(b)) ||
                (c.Descripcion != null && c.Descripcion.ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .ToListAsync();

        return (total, items);
    }

    public Task<Consulta?> ObtenerDetalleAsync(int id) =>
        db.Consultas
            .Include(x => x.Cliente).ThenInclude(cl => cl!.Empresa)
            .Include(x => x.Empresa)
            .Include(x => x.Responsable)
            .Include(x => x.Presupuesto).ThenInclude(p => p!.Items).ThenInclude(i => i.Analisis).ThenInclude(a => a.Grupo)
            .Include(x => x.Presupuesto).ThenInclude(p => p!.Paquetes).ThenInclude(pp => pp.Paquete).ThenInclude(pq => pq.Items).ThenInclude(pa => pa.Analisis)
            .Include(x => x.Presupuesto).ThenInclude(p => p!.Metodos).ThenInclude(pm => pm.Metodo)
            .FirstOrDefaultAsync(x => x.IdConsulta == id);

    public Task<Consulta?> ObtenerAsync(int id) => db.Consultas.FindAsync(id).AsTask();

    public Task<string?> ObtenerUltimoCodigoAsync(string prefijo) =>
        db.Consultas
            .Where(c => c.Codigo != null && c.Codigo.StartsWith(prefijo))
            .OrderByDescending(c => c.Codigo)
            .Select(c => c.Codigo)
            .FirstOrDefaultAsync();

    public void Agregar(Consulta consulta) => db.Consultas.Add(consulta);

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
