namespace Ssal.Application.Analisis;

public class CrearMetodo(IAnalisisRepository repo)
{
    static readonly string[] AreasValidas = ["MIC", "FQ", "AMBAS"];

    public async Task<Resultado<MetodoCreadoDto>> EjecutarAsync(GuardarMetodoCommand cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd.Nombre))
            return Resultado<MetodoCreadoDto>.Fallido("El nombre del método es obligatorio.");
        if (!AreasValidas.Contains(cmd.Area))
            return Resultado<MetodoCreadoDto>.Fallido("El área debe ser MIC, FQ o AMBAS.");

        var metodo = new Ssal.Domain.Metodo
        {
            Nombre = cmd.Nombre.Trim(),
            Descripcion = cmd.Descripcion,
            Area = cmd.Area,
            Activo = true
        };

        repo.AgregarMetodo(metodo);
        await repo.GuardarCambiosAsync();

        return Resultado<MetodoCreadoDto>.Exitoso(new MetodoCreadoDto(metodo.IdMetodo));
    }
}
