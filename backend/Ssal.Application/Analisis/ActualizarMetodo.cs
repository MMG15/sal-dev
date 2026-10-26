namespace Ssal.Application.Analisis;

public class ActualizarMetodo(IAnalisisRepository repo)
{
    static readonly string[] AreasValidas = ["MIC", "FQ", "AMBAS"];

    public async Task<Resultado> EjecutarAsync(int id, GuardarMetodoCommand cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd.Nombre))
            return Resultado.Fallido("El nombre del método es obligatorio.");
        if (!AreasValidas.Contains(cmd.Area))
            return Resultado.Fallido("El área debe ser MIC, FQ o AMBAS.");

        var metodo = await repo.ObtenerMetodoPorIdAsync(id);
        if (metodo is null) return Resultado.Fallido("No encontrado");

        metodo.Nombre = cmd.Nombre.Trim();
        metodo.Descripcion = cmd.Descripcion;
        metodo.Area = cmd.Area;

        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
