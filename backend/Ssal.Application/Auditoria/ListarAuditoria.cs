namespace Ssal.Application.Auditoria;

public record ListarAuditoriaResultado(int Total, int Pagina, int PorPagina, List<AuditoriaItemDto> Items);

public class ListarAuditoria(IAuditoriaRepository repo)
{
    public async Task<ListarAuditoriaResultado> EjecutarAsync(string? busqueda, string? accion, int pagina, int porPagina)
    {
        var (total, items) = await repo.BuscarAsync(busqueda, accion, pagina, porPagina);
        return new ListarAuditoriaResultado(total, pagina, porPagina, items);
    }
}
