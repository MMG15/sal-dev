namespace Ssal.Application.Analisis;

// Para el selector de presupuesto: solo análisis activos, agrupados por grupo/área.
public class ListarAnalisisActivos(IAnalisisRepository repo)
{
    public async Task<List<GrupoConAnalisisDto>> EjecutarAsync()
    {
        var grupos = await repo.ListarGruposConAnalisisAsync();

        return grupos
            .OrderBy(g => g.Area)
            .ThenBy(g => g.CodigoAgrupador)
            .Select(g => new GrupoConAnalisisDto(
                g.IdGrupo,
                g.CodigoAgrupador,
                g.Descripcion,
                g.Area,
                g.Analisis
                    .Where(a => a.Activo)
                    .OrderBy(a => a.Codigo)
                    .Select(a => new ItemAnalisisDto(a.IdAnalisis, a.Codigo, a.Nombre, a.Area, a.PrecioUsd))
                    .ToList()))
            .Where(g => g.Items.Count > 0)
            .ToList();
    }
}
