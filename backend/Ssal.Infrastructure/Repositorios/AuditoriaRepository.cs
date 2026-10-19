using Microsoft.EntityFrameworkCore;
using Ssal.Application.Auditoria;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class AuditoriaRepository(SsalDbContext db) : IAuditoriaRepository
{
    public async Task<(int Total, List<AuditoriaItemDto> Items)> BuscarAsync(
        string? busqueda, string? accion, int pagina, int porPagina)
    {
        var q = db.Auditoria.Include(a => a.Usuario).AsQueryable();

        if (!string.IsNullOrWhiteSpace(accion))
            q = q.Where(a => a.Accion == accion);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(a =>
                (a.Objetivo != null && a.Objetivo.ToLower().Contains(b)) ||
                (a.Usuario != null && (a.Usuario.Nombre + " " + a.Usuario.Apellido).ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(a => a.Fecha)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(a => new AuditoriaItemDto(
                a.Id,
                a.Fecha,
                a.Usuario == null ? null : a.Usuario.Nombre + " " + a.Usuario.Apellido,
                a.Accion,
                a.Objetivo,
                a.Detalle))
            .ToListAsync();

        return (total, items);
    }
}
