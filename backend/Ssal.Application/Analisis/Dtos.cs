namespace Ssal.Application.Analisis;

// Para el selector de presupuesto: solo análisis activos, agrupados.
public record GrupoConAnalisisDto(
    int IdGrupo,
    string CodigoAgrupador,
    string? Descripcion,
    string Area,
    IReadOnlyList<ItemAnalisisDto> Items);

public record ItemAnalisisDto(int IdAnalisis, string Codigo, string Nombre, string Area, decimal PrecioUsd);

// Para la pantalla de gestión: todos los análisis, incluyendo inactivos.
public record AnalisisDetalleDto(
    int IdAnalisis,
    string Codigo,
    string Nombre,
    string Area,
    decimal PrecioUsd,
    string? Unidad,
    decimal? RangoMin,
    decimal? RangoMax,
    string? ValorEsperado,
    bool Activo,
    GrupoResumenDto Grupo);

public record GrupoResumenDto(int IdGrupo, string CodigoAgrupador, string Area);

public record GrupoDto(int IdGrupo, string CodigoAgrupador, string? Descripcion, string Area);

// Entrada compartida por Crear y Actualizar — mismo shape que ya usaba el controller.
public record GuardarAnalisisCommand(
    int IdGrupo,
    string? NuevoGrupo,
    string Codigo,
    string Nombre,
    string Area,
    decimal PrecioUsd,
    string? Unidad,
    decimal? RangoMin,
    decimal? RangoMax,
    string? ValorEsperado);
