namespace Ssal.Application.Analisis;

public class ActualizarGrupo(IAnalisisRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, GuardarGrupoCommand cmd)
    {
        var grupo = await repo.ObtenerGrupoPorIdAsync(id);
        if (grupo is null) return Resultado.Fallido("No encontrado");

        grupo.TipoMuestra = cmd.TipoMuestra;
        grupo.Norma = cmd.Norma;
        grupo.CantidadMuestra = cmd.CantidadMuestra;

        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
