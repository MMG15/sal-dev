namespace Ssal.Application.Analisis;

public class ListarGruposDeAnalisis(IAnalisisRepository repo)
{
    public async Task<List<GrupoDto>> EjecutarAsync()
    {
        var grupos = await repo.ListarGruposAsync();

        return grupos
            .OrderBy(g => g.Area)
            .ThenBy(g => g.CodigoAgrupador)
            .Select(g => new GrupoDto(g.IdGrupo, g.CodigoAgrupador, g.Descripcion, g.Area))
            .ToList();
    }
}
