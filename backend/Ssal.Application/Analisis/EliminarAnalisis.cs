namespace Ssal.Application.Analisis;

public class EliminarAnalisis(IAnalisisRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id)
    {
        var analisis = await repo.ObtenerPorIdAsync(id);
        if (analisis is null) return Resultado.Fallido("No encontrado");

        if (await repo.TieneUsoEnPresupuestosAsync(id))
            return Resultado.Fallido("No se puede eliminar: el análisis está incluido en uno o más presupuestos. Podés desactivarlo en su lugar.");

        repo.Eliminar(analisis);
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
