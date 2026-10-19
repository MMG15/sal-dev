namespace Ssal.Application.Analisis;

// Para la pantalla de gestión: todos los análisis, incluyendo inactivos.
public class ListarTodosLosAnalisis(IAnalisisRepository repo)
{
    public async Task<List<AnalisisDetalleDto>> EjecutarAsync()
    {
        var items = await repo.ListarTodosConGrupoAsync();

        return items
            .OrderBy(a => a.Area)
            .ThenBy(a => a.Grupo.CodigoAgrupador)
            .ThenBy(a => a.Codigo)
            .Select(a => new AnalisisDetalleDto(
                a.IdAnalisis,
                a.Codigo,
                a.Nombre,
                a.Area,
                a.PrecioUsd,
                a.Unidad,
                a.RangoMin,
                a.RangoMax,
                a.ValorEsperado,
                a.Activo,
                new GrupoResumenDto(a.Grupo.IdGrupo, a.Grupo.CodigoAgrupador, a.Grupo.Area)))
            .ToList();
    }
}
