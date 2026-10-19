namespace Ssal.Application.Analisis;

public class ActualizarAnalisis(IAnalisisRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, GuardarAnalisisCommand cmd)
    {
        var analisis = await repo.ObtenerPorIdAsync(id);
        if (analisis is null) return Resultado.Fallido("No encontrado");

        // Código único (si cambió)
        if (cmd.Codigo != analisis.Codigo && await repo.ExisteCodigoAsync(cmd.Codigo, id))
            return Resultado.Fallido($"Ya existe un análisis con el código '{cmd.Codigo}'.");

        // Grupo nuevo si aplica
        var idGrupo = cmd.IdGrupo;
        if (idGrupo == 0 && !string.IsNullOrWhiteSpace(cmd.NuevoGrupo))
        {
            var grupo = repo.CrearGrupo(cmd.NuevoGrupo.Trim(), cmd.Area);
            await repo.GuardarCambiosAsync();
            idGrupo = grupo.IdGrupo;
        }

        analisis.Codigo = cmd.Codigo.Trim();
        analisis.Nombre = cmd.Nombre.Trim();
        analisis.Area = cmd.Area;
        analisis.PrecioUsd = cmd.PrecioUsd;
        analisis.Unidad = cmd.Unidad;
        analisis.RangoMin = cmd.RangoMin;
        analisis.RangoMax = cmd.RangoMax;
        analisis.ValorEsperado = cmd.ValorEsperado;
        analisis.IdGrupo = idGrupo;

        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
