namespace Ssal.Application.Analisis;

// Para el selector de presupuesto: solo paquetes activos.
public class ListarPaquetesActivos(IAnalisisRepository repo)
{
    public async Task<List<PaqueteConAnalisisDto>> EjecutarAsync()
    {
        var paquetes = await repo.ListarPaquetesConAnalisisAsync();

        return paquetes
            .OrderBy(p => p.Area)
            .ThenBy(p => p.Nombre)
            .Select(MapeoPaquetes.MapConAnalisis)
            .ToList();
    }
}

internal static class MapeoPaquetes
{
    public static PaqueteConAnalisisDto MapConAnalisis(Ssal.Domain.Paquete p) => new(
        p.IdPaquete, p.Nombre, p.Area, p.TipoMuestra, p.Norma, p.CantidadMuestra, p.PrecioUsd,
        p.Items
            .Select(i => i.Analisis)
            .OrderBy(a => a.Codigo)
            .Select(a => new ItemAnalisisDto(a.IdAnalisis, a.Codigo, a.Nombre, a.Area, a.PrecioUsd))
            .ToList());

    public static PaqueteDetalleDto MapDetalle(Ssal.Domain.Paquete p) => new(
        p.IdPaquete, p.Nombre, p.Area, p.TipoMuestra, p.Norma, p.CantidadMuestra, p.PrecioUsd, p.Activo,
        p.Items
            .Select(i => i.Analisis)
            .OrderBy(a => a.Codigo)
            .Select(a => new ItemAnalisisDto(a.IdAnalisis, a.Codigo, a.Nombre, a.Area, a.PrecioUsd))
            .ToList());
}
