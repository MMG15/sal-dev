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

    Ssal.Domain.GrupoAnalisis CrearGrupo(string codigoAgrupador, string area);

    void Agregar(Ssal.Domain.Analisis analisis);

    Task<bool> TieneUsoEnPresupuestosAsync(int idAnalisis);

    void Eliminar(Ssal.Domain.Analisis analisis);

    Task GuardarCambiosAsync();
}
