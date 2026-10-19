namespace Ssal.Application.Presupuestos;

public class ListarArchivosPresupuesto(IPresupuestoRepository repo)
{
    public async Task<List<ArchivoPresupuestoDto>> EjecutarAsync(int idPresupuesto)
    {
        var archivos = await repo.ListarArchivosAsync(idPresupuesto);
        return archivos
            .OrderByDescending(a => a.SubidoEn)
            .Select(a => new ArchivoPresupuestoDto(
                a.Id, a.NombreOriginal, a.TipoMime, a.TamañoBytes, a.SubidoEn,
                $"{a.SubidoPorUsuario.Nombre} {a.SubidoPorUsuario.Apellido}"))
            .ToList();
    }
}

public class SubirArchivoPresupuesto(IPresupuestoRepository repo)
{
    private static readonly string[] ExtensionesPermitidas = [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg", ".zip"];

    public async Task<Resultado<ArchivoSubidoDto>> EjecutarAsync(
        int idPresupuesto, string nombreOriginal, string tipoMime, long longitud, Stream contenido, int idUsuario, CancellationToken ct)
    {
        if (longitud == 0)
            return Resultado<ArchivoSubidoDto>.Fallido("No se recibió ningún archivo.");

        var ext = Path.GetExtension(nombreOriginal).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(ext))
            return Resultado<ArchivoSubidoDto>.Fallido($"Tipo de archivo no permitido ({ext}).");

        var presupuesto = await repo.ObtenerAsync(idPresupuesto);
        if (presupuesto is null) return Resultado<ArchivoSubidoDto>.Fallido("No encontrado");

        var nombreAlmacenado = await repo.GuardarArchivoEnDiscoAsync(idPresupuesto, ext, contenido, ct);

        var registro = new Ssal.Domain.PresupuestoArchivo
        {
            IdPresupuesto = idPresupuesto,
            NombreOriginal = nombreOriginal,
            NombreAlmacenado = nombreAlmacenado,
            TipoMime = tipoMime,
            TamañoBytes = longitud,
            SubidoPor = idUsuario,
            SubidoEn = DateTime.UtcNow
        };
        repo.AgregarArchivo(registro);
        await repo.GuardarCambiosAsync();

        return Resultado<ArchivoSubidoDto>.Exitoso(new ArchivoSubidoDto(registro.Id, registro.NombreOriginal, registro.TamañoBytes));
    }
}

public class DescargarArchivoPresupuesto(IPresupuestoRepository repo)
{
    public async Task<Resultado<ArchivoContenidoDto>> EjecutarAsync(int idPresupuesto, int idArchivo)
    {
        var archivo = await repo.ObtenerArchivoAsync(idArchivo, idPresupuesto);
        if (archivo is null) return Resultado<ArchivoContenidoDto>.Fallido("No encontrado");

        var bytes = await repo.LeerArchivoDeDiscoAsync(idPresupuesto, archivo.NombreAlmacenado);
        if (bytes is null) return Resultado<ArchivoContenidoDto>.Fallido("Archivo no encontrado en disco.");

        return Resultado<ArchivoContenidoDto>.Exitoso(new ArchivoContenidoDto(bytes, archivo.TipoMime, archivo.NombreOriginal));
    }
}

public class EliminarArchivoPresupuesto(IPresupuestoRepository repo)
{
    public async Task<bool> EjecutarAsync(int idPresupuesto, int idArchivo)
    {
        var archivo = await repo.ObtenerArchivoAsync(idArchivo, idPresupuesto);
        if (archivo is null) return false;

        repo.EliminarArchivoDelDisco(idPresupuesto, archivo.NombreAlmacenado);
        repo.EliminarArchivoRegistro(archivo);
        await repo.GuardarCambiosAsync();
        return true;
    }
}
