using Microsoft.EntityFrameworkCore;
using Ssal.Application.Correos;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class CorreoEntranteRepository(SsalDbContext db) : ICorreoEntranteRepository
{
    public async Task<(int Total, int Pendientes, List<CorreoResumenDto> Items)> BuscarAsync(string? busqueda, string? estado, int pagina, int porPagina)
    {
        var q = db.CorreosEntrantes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(c => c.Estado == estado);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(c =>
                (c.Asunto != null && c.Asunto.ToLower().Contains(b)) ||
                c.Remitente.ToLower().Contains(b) ||
                (c.RemitenteNombre != null && c.RemitenteNombre.ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(c => c.FechaRecibido)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(c => new CorreoResumenDto(
                c.IdCorreo, c.Remitente, c.RemitenteNombre, c.Asunto,
                c.Cuerpo != null && c.Cuerpo.Length > 160 ? c.Cuerpo.Substring(0, 160) : c.Cuerpo,
                c.FechaRecibido, c.Estado, c.IdConsultaGenerada, c.IdConsultaRelacionada))
            .ToListAsync();

        var pendientes = await db.CorreosEntrantes.CountAsync(c => c.Estado == "pendiente");

        return (total, pendientes, items);
    }

    public Task<CorreoEntrante?> ObtenerDetalleAsync(int id) =>
        db.CorreosEntrantes
            .Include(x => x.UsuarioAccion)
            .Include(x => x.ConsultaGenerada)
            .Include(x => x.ConsultaRelacionada).ThenInclude(cr => cr!.Presupuesto)
            .FirstOrDefaultAsync(x => x.IdCorreo == id);

    public Task<CorreoEntrante?> ObtenerConCuentaAsync(int id) =>
        db.CorreosEntrantes.Include(x => x.CuentaCorreo).FirstOrDefaultAsync(x => x.IdCorreo == id);

    public Task<CorreoEntrante?> ObtenerAsync(int id) => db.CorreosEntrantes.FindAsync(id).AsTask();

    public Task<CuentaCorreo?> ObtenerPrimeraCuentaHabilitadaAsync() =>
        db.CuentasCorreo.Where(x => x.Habilitada).OrderBy(x => x.IdCuenta).FirstOrDefaultAsync();

    public Task<string?> ObtenerUltimoCodigoConsultaAsync(string prefijo) =>
        db.Consultas
            .Where(c => c.Codigo != null && c.Codigo.StartsWith(prefijo))
            .OrderByDescending(c => c.Codigo)
            .Select(c => c.Codigo)
            .FirstOrDefaultAsync();

    public void AgregarConsulta(Consulta consulta) => db.Consultas.Add(consulta);

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
