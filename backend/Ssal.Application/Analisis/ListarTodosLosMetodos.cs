namespace Ssal.Application.Analisis;

// Para la pantalla de gestión: todos los métodos, incluyendo inactivos.
public class ListarTodosLosMetodos(IAnalisisRepository repo)
{
    public async Task<List<MetodoDetalleDto>> EjecutarAsync()
    {
        var metodos = await repo.ListarTodosLosMetodosAsync();
        return metodos.Select(m => new MetodoDetalleDto(m.IdMetodo, m.Nombre, m.Descripcion, m.Area, m.Activo)).ToList();
    }
}
