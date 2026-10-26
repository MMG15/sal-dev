namespace Ssal.Application.Analisis;

// Para selectores (catálogo de análisis, presupuesto): solo métodos activos.
public class ListarMetodosActivos(IAnalisisRepository repo)
{
    public async Task<List<MetodoDto>> EjecutarAsync()
    {
        var metodos = await repo.ListarMetodosActivosAsync();
        return metodos.Select(m => new MetodoDto(m.IdMetodo, m.Nombre, m.Descripcion, m.Area)).ToList();
    }
}
