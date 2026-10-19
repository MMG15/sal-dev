namespace Ssal.Application.Analisis;

public class ToggleActivoAnalisis(IAnalisisRepository repo)
{
    public async Task<Resultado<(int IdAnalisis, bool Activo)>> EjecutarAsync(int id)
    {
        var analisis = await repo.ObtenerPorIdAsync(id);
        if (analisis is null) return Resultado<(int, bool)>.Fallido("No encontrado");

        analisis.Activo = !analisis.Activo;
        await repo.GuardarCambiosAsync();

        return Resultado<(int, bool)>.Exitoso((analisis.IdAnalisis, analisis.Activo));
    }
}
