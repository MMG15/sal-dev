using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ssal.Application.Presupuestos;
using Ssal.Domain;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure.Repositorios;

public class PresupuestoRepository(SsalDbContext db, IConfiguration config) : IPresupuestoRepository
{
    public async Task<(int Total, List<PresupuestoResumenDto> Items)> BuscarAsync(string? busqueda, string? estado, int pagina, int porPagina)
    {
        var q = db.Presupuestos
            .Include(p => p.Consulta).ThenInclude(c => c.Cliente)
            .Include(p => p.Consulta).ThenInclude(c => c.Empresa)
            .Include(p => p.CreadoPorUsuario)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            q = q.Where(p => p.Estado == estado);

        if (!string.IsNullOrWhiteSpace(busqueda))
        {
            var b = busqueda.ToLower();
            q = q.Where(p =>
                (p.CodigoRpo != null && p.CodigoRpo.ToLower().Contains(b)) ||
                (p.Consulta.Codigo != null && p.Consulta.Codigo.ToLower().Contains(b)) ||
                (p.Consulta.Cliente != null && (p.Consulta.Cliente.Nombre + " " + p.Consulta.Cliente.Apellido).ToLower().Contains(b)) ||
                (p.Consulta.Empresa != null && p.Consulta.Empresa.RazonSocial.ToLower().Contains(b)));
        }

        var total = await q.CountAsync();
        var items = await q
            .OrderByDescending(p => p.Fecha)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .Select(p => new PresupuestoResumenDto(
                p.IdPresupuesto, p.CodigoRpo, p.Estado, p.ImporteDolares, p.ImportePesos, p.Cotizacion, p.Fecha,
                p.Consulta.Codigo, p.Consulta.IdConsulta,
                p.Consulta.Cliente == null ? null : new ClienteNombreEmailDto(p.Consulta.Cliente.Nombre, p.Consulta.Cliente.Apellido, p.Consulta.Cliente.Email),
                p.Consulta.Empresa == null ? null : new EmpresaNombreDto(p.Consulta.Empresa.RazonSocial),
                new UsuarioNombreDto(p.CreadoPorUsuario.Nombre, p.CreadoPorUsuario.Apellido)))
            .ToListAsync();

        return (total, items);
    }

    public Task<Presupuesto?> ObtenerDetalleAsync(int id) =>
        db.Presupuestos
            .Include(x => x.Consulta).ThenInclude(c => c.Cliente).ThenInclude(cl => cl!.Empresa)
            .Include(x => x.Consulta).ThenInclude(c => c.Empresa)
            .Include(x => x.CreadoPorUsuario)
            .Include(x => x.Items).ThenInclude(i => i.Analisis).ThenInclude(a => a.Grupo)
            .FirstOrDefaultAsync(x => x.IdPresupuesto == id);

    public Task<Presupuesto?> ObtenerCompletoAsync(int id) =>
        db.Presupuestos
            .Include(x => x.Consulta).ThenInclude(c => c.Cliente).ThenInclude(cl => cl!.Empresa)
            .Include(x => x.Consulta).ThenInclude(c => c.Empresa)
            .Include(x => x.CreadoPorUsuario)
            .Include(x => x.Items).ThenInclude(i => i.Analisis)
            .FirstOrDefaultAsync(x => x.IdPresupuesto == id);

    public Task<Presupuesto?> ObtenerAsync(int id) => db.Presupuestos.FindAsync(id).AsTask();

    public Task<Presupuesto?> ObtenerConConsultaAsync(int id) =>
        db.Presupuestos.Include(x => x.Consulta).FirstOrDefaultAsync(x => x.IdPresupuesto == id);

    public Task<Consulta?> ObtenerConsultaAsync(int idConsulta) => db.Consultas.FindAsync(idConsulta).AsTask();

    public Task<Presupuesto?> ObtenerExistentePorConsultaAsync(int idConsulta) =>
        db.Presupuestos.Include(p => p.Items).FirstOrDefaultAsync(p => p.IdConsulta == idConsulta);

    public void EliminarConItems(Presupuesto existente)
    {
        db.RemoveRange(existente.Items);
        db.Remove(existente);
    }

    public async Task<Dictionary<int, Analisis>> ObtenerAnalisisPorIdsAsync(List<int> ids) =>
        await db.Analisis.Where(a => ids.Contains(a.IdAnalisis)).ToDictionaryAsync(a => a.IdAnalisis);

    public Task<string?> ObtenerUltimoCodigoAsync(string prefijo) =>
        db.Presupuestos
            .Where(p => p.CodigoRpo != null && p.CodigoRpo.StartsWith(prefijo))
            .OrderByDescending(p => p.CodigoRpo)
            .Select(p => p.CodigoRpo)
            .FirstOrDefaultAsync();

    public void Agregar(Presupuesto presupuesto) => db.Presupuestos.Add(presupuesto);

    public async Task<int?> ObtenerIdCuentaOrigenAsync(int idConsulta) =>
        await db.CorreosEntrantes
            .Where(c => c.IdConsultaGenerada == idConsulta)
            .Select(c => c.IdCuentaCorreo)
            .FirstOrDefaultAsync();

    public Task<CuentaCorreo?> ObtenerCuentaAsync(int idCuenta) => db.CuentasCorreo.FindAsync(idCuenta).AsTask();

    public Task<CuentaCorreo?> ObtenerPrimeraCuentaHabilitadaAsync() =>
        db.CuentasCorreo.Where(c => c.Habilitada).OrderBy(c => c.IdCuenta).FirstOrDefaultAsync();

    public async Task<List<PresupuestoArchivo>> ListarArchivosAsync(int idPresupuesto) =>
        await db.PresupuestosArchivos.Include(a => a.SubidoPorUsuario).Where(a => a.IdPresupuesto == idPresupuesto).ToListAsync();

    public void AgregarArchivo(PresupuestoArchivo archivo) => db.PresupuestosArchivos.Add(archivo);

    public Task<PresupuestoArchivo?> ObtenerArchivoAsync(int id, int idPresupuesto) =>
        db.PresupuestosArchivos.FirstOrDefaultAsync(a => a.Id == id && a.IdPresupuesto == idPresupuesto);

    public void EliminarArchivoRegistro(PresupuestoArchivo archivo) => db.PresupuestosArchivos.Remove(archivo);

    public async Task<string> GuardarArchivoEnDiscoAsync(int idPresupuesto, string extension, Stream contenido, CancellationToken ct)
    {
        var carpeta = CarpetaDe(idPresupuesto);
        Directory.CreateDirectory(carpeta);

        var nombreAlmacenado = $"{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(carpeta, nombreAlmacenado);

        using (var archivo = System.IO.File.Create(rutaCompleta))
            await contenido.CopyToAsync(archivo, ct);

        return nombreAlmacenado;
    }

    public async Task<byte[]?> LeerArchivoDeDiscoAsync(int idPresupuesto, string nombreAlmacenado)
    {
        var ruta = Path.Combine(CarpetaDe(idPresupuesto), nombreAlmacenado);
        if (!System.IO.File.Exists(ruta)) return null;
        return await System.IO.File.ReadAllBytesAsync(ruta);
    }

    public void EliminarArchivoDelDisco(int idPresupuesto, string nombreAlmacenado)
    {
        var ruta = Path.Combine(CarpetaDe(idPresupuesto), nombreAlmacenado);
        if (System.IO.File.Exists(ruta))
            System.IO.File.Delete(ruta);
    }

    private string CarpetaDe(int idPresupuesto)
    {
        var basePath = config["Storage:BasePath"] ?? "storage";
        return Path.Combine(Directory.GetCurrentDirectory(), basePath, "presupuestos", idPresupuesto.ToString());
    }

    public Task GuardarCambiosAsync() => db.SaveChangesAsync();
}
