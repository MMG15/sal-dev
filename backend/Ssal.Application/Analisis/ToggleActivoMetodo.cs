namespace Ssal.Application.Analisis;

public class ToggleActivoMetodo(IAnalisisRepository repo)
{
    public async Task<Resultado<(int IdMetodo, bool Activo)>> EjecutarAsync(int id)
    {
        var metodo = await repo.ObtenerMetodoPorIdAsync(id);
        if (metodo is null) return Resultado<(int, bool)>.Fallido("No encontrado");

        metodo.Activo = !metodo.Activo;
        await repo.GuardarCambiosAsync();

        return Resultado<(int, bool)>.Exitoso((metodo.IdMetodo, metodo.Activo));
    }
}
