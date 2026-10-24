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
    string? Metodo,
    bool Tercerizado,
    bool Activo,
    GrupoResumenDto Grupo);

public record GrupoResumenDto(int IdGrupo, string CodigoAgrupador, string Area);

public record GrupoDto(
    int IdGrupo, string CodigoAgrupador, string? Descripcion, string Area,
    string? TipoMuestra, string? Norma, string? CantidadMuestra);

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
    string? ValorEsperado,
    string? Metodo,
    bool Tercerizado);

public record GuardarGrupoCommand(string? TipoMuestra, string? Norma, string? CantidadMuestra);

// ─── Paquetes ───────────────────────────────────────────────────────────────

// Para el selector de presupuesto: solo paquetes activos, con el detalle de qué análisis incluyen.
public record PaqueteConAnalisisDto(
    int IdPaquete, string Nombre, string Area, string? TipoMuestra, string? Norma,
    string? CantidadMuestra, decimal PrecioUsd, IReadOnlyList<ItemAnalisisDto> Analisis);

// Para la pantalla de gestión: todos los paquetes, incluyendo inactivos.
public record PaqueteDetalleDto(
    int IdPaquete, string Nombre, string Area, string? TipoMuestra, string? Norma,
    string? CantidadMuestra, decimal PrecioUsd, bool Activo, IReadOnlyList<ItemAnalisisDto> Analisis);

public record GuardarPaqueteCommand(
    string Nombre, string Area, string? TipoMuestra, string? Norma, string? CantidadMuestra,
    decimal PrecioUsd, List<int> IdsAnalisis);
