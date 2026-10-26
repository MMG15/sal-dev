namespace Ssal.Application.Analisis;

public class CrearAnalisis(IAnalisisRepository repo)
{
    public async Task<Resultado<int>> EjecutarAsync(GuardarAnalisisCommand cmd)
    {
        // Si se envía nombre de grupo nuevo, crearlo
        var idGrupo = cmd.IdGrupo;
        if (idGrupo == 0 && !string.IsNullOrWhiteSpace(cmd.NuevoGrupo))
        {
            var grupo = repo.CrearGrupo(cmd.NuevoGrupo.Trim(), cmd.Area);
            await repo.GuardarCambiosAsync();
            idGrupo = grupo.IdGrupo;
        }

        if (await repo.ExisteCodigoAsync(cmd.Codigo, null))
            return Resultado<int>.Fallido($"Ya existe un análisis con el código '{cmd.Codigo}'.");

        var analisis = new Ssal.Domain.Analisis
        {
            IdGrupo = idGrupo,
            Codigo = cmd.Codigo.Trim(),
            Nombre = cmd.Nombre.Trim(),
            Area = cmd.Area,
            PrecioUsd = cmd.PrecioUsd,
            Unidad = cmd.Unidad,
            RangoMin = cmd.RangoMin,
            RangoMax = cmd.RangoMax,
            ValorEsperado = cmd.ValorEsperado,
            IdMetodo = cmd.IdMetodo,
            Tercerizado = cmd.Tercerizado,
            Activo = true
        };

        repo.Agregar(analisis);
        await repo.GuardarCambiosAsync();

        return Resultado<int>.Exitoso(analisis.IdAnalisis);
    }
}
