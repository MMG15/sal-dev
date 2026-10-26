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
    MetodoResumenDto? Metodo,
    bool Tercerizado,
    bool Activo,
    GrupoResumenDto Grupo);

public record MetodoResumenDto(int IdMetodo, string Nombre, string Area);

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
    int? IdMetodo,
    bool Tercerizado);

// CodigoAgrupador/Area solo se usan al crear (ActualizarGrupo no los toca — el nombre y el
// área de una categoría ya existente no se editan desde acá).
public record GuardarGrupoCommand(string? CodigoAgrupador, string? Area, string? TipoMuestra, string? Norma, string? CantidadMuestra);

public record GrupoCreadoDto(int IdGrupo);

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

// ─── Métodos ────────────────────────────────────────────────────────────────
// Norma + equipo + sensibilidad + condiciones de un análisis. Catálogo reutilizable: antes era
// texto libre repetido en cada análisis; ahora se carga una vez y se referencia desde Analisis
// y desde Presupuesto (para mostrar en el PDF qué método se va a aplicar).

// Para selectores (catálogo de análisis, presupuesto): solo métodos activos.
public record MetodoDto(int IdMetodo, string Nombre, string? Descripcion, string Area);

// Para la pantalla de gestión: todos los métodos, incluyendo inactivos.
public record MetodoDetalleDto(int IdMetodo, string Nombre, string? Descripcion, string Area, bool Activo);

public record GuardarMetodoCommand(string Nombre, string? Descripcion, string Area);

public record MetodoCreadoDto(int IdMetodo);
