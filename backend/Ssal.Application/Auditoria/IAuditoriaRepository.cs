namespace Ssal.Application.Auditoria;

public record AuditoriaItemDto(long Id, DateTime Fecha, string? Actor, string Accion, string? Objetivo, string? Detalle);

// Es una consulta de reporte (paginada, con filtros): el repositorio devuelve directamente
// el DTO de lectura en vez de entidades, porque acá no hace falta mutar nada.
public interface IAuditoriaRepository
{
    Task<(int Total, List<AuditoriaItemDto> Items)> BuscarAsync(
        string? busqueda, string? accion, int pagina, int porPagina);
}
