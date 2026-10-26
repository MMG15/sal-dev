namespace Ssal.Application.Analisis;

public class EliminarMetodo(IAnalisisRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id)
    {
        var metodo = await repo.ObtenerMetodoPorIdAsync(id);
        if (metodo is null) return Resultado.Fallido("No encontrado");

        if (await repo.TieneMetodoUsoAsync(id))
            return Resultado.Fallido("No se puede eliminar: el método está en uso en uno o más análisis o presupuestos. Podés desactivarlo en su lugar.");

        repo.EliminarMetodo(metodo);
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
