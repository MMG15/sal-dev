namespace Ssal.Application.Analisis;

// La implementación real (Entity Framework Core) vive en Ssal.Infrastructure. Acá solo se
// describe qué necesitan los casos de uso — nada de LINQ-a-SQL ni de EF Core en esta firma.
public interface IAnalisisRepository
{
    // Grupos con sus análisis cargados (todos, activos e inactivos); el caso de uso decide
    // qué filtrar y en qué orden mostrarlo.
    Task<List<Ssal.Domain.GrupoAnalisis>> ListarGruposConAnalisisAsync();

    Task<List<Ssal.Domain.Analisis>> ListarTodosConGrupoAsync();

    Task<List<Ssal.Domain.GrupoAnalisis>> ListarGruposAsync();

    Task<bool> ExisteCodigoAsync(string codigo, int? excluirId);

    Task<Ssal.Domain.Analisis?> ObtenerPorIdAsync(int id);

    Task<Ssal.Domain.GrupoAnalisis?> ObtenerGrupoPorIdAsync(int id);

    Ssal.Domain.GrupoAnalisis CrearGrupo(string codigoAgrupador, string area);

    Task<bool> ExisteGrupoConCodigoAsync(string codigoAgrupador, string area);

    void Agregar(Ssal.Domain.Analisis analisis);

    Task<bool> TieneUsoEnPresupuestosAsync(int idAnalisis);

    void Eliminar(Ssal.Domain.Analisis analisis);

    // Paquetes: catálogo de combos reutilizables (precio propio, no es la suma de sus partes).
    Task<List<Ssal.Domain.Paquete>> ListarPaquetesConAnalisisAsync();

    Task<List<Ssal.Domain.Paquete>> ListarTodosLosPaquetesConAnalisisAsync();

    Task<Ssal.Domain.Paquete?> ObtenerPaquetePorIdAsync(int id);

    Task<List<Ssal.Domain.Analisis>> ObtenerAnalisisPorIdsAsync(List<int> ids);

    void AgregarPaquete(Ssal.Domain.Paquete paquete);

    void ReemplazarItemsDePaquete(Ssal.Domain.Paquete paquete, List<Ssal.Domain.Analisis> nuevosAnalisis);

    Task<bool> TienePaqueteUsoEnPresupuestosAsync(int idPaquete);

    void EliminarPaquete(Ssal.Domain.Paquete paquete);

    // Métodos: catálogo reutilizable de normas/equipos/técnicas (antes texto libre por análisis).
    Task<List<Ssal.Domain.Metodo>> ListarMetodosActivosAsync();

    Task<List<Ssal.Domain.Metodo>> ListarTodosLosMetodosAsync();

    Task<Ssal.Domain.Metodo?> ObtenerMetodoPorIdAsync(int id);

    void AgregarMetodo(Ssal.Domain.Metodo metodo);

    Task<bool> TieneMetodoUsoAsync(int idMetodo);

    void EliminarMetodo(Ssal.Domain.Metodo metodo);

    Task GuardarCambiosAsync();
}
