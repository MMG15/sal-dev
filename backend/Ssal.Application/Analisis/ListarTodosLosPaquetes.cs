namespace Ssal.Application.Analisis;

// Para la pantalla de gestión: todos los paquetes, incluyendo inactivos.
public class ListarTodosLosPaquetes(IAnalisisRepository repo)
{
    public async Task<List<PaqueteDetalleDto>> EjecutarAsync()
    {
        var paquetes = await repo.ListarTodosLosPaquetesConAnalisisAsync();

        return paquetes
            .OrderBy(p => p.Area)
            .ThenBy(p => p.Nombre)
            .Select(MapeoPaquetes.MapDetalle)
            .ToList();
    }
}
