namespace Ssal.Application.Analisis;

public class CrearGrupo(IAnalisisRepository repo)
{
    static readonly string[] AreasValidas = ["MIC", "FQ"];

    public async Task<Resultado<GrupoCreadoDto>> EjecutarAsync(GuardarGrupoCommand cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd.CodigoAgrupador))
            return Resultado<GrupoCreadoDto>.Fallido("El nombre de la categoría es obligatorio.");

        if (cmd.Area is null || !AreasValidas.Contains(cmd.Area))
            return Resultado<GrupoCreadoDto>.Fallido("El área debe ser MIC o FQ.");

        var codigo = cmd.CodigoAgrupador.Trim();
        if (await repo.ExisteGrupoConCodigoAsync(codigo, cmd.Area))
            return Resultado<GrupoCreadoDto>.Fallido($"Ya existe una categoría '{codigo}' en el área {cmd.Area}.");

        var grupo = repo.CrearGrupo(codigo, cmd.Area);
        grupo.TipoMuestra = cmd.TipoMuestra;
        grupo.Norma = cmd.Norma;
        grupo.CantidadMuestra = cmd.CantidadMuestra;

        await repo.GuardarCambiosAsync();
        return Resultado<GrupoCreadoDto>.Exitoso(new GrupoCreadoDto(grupo.IdGrupo));
    }
}
