namespace Ssal.Application.Sse;

public record ListarSseResultado(int Total, int Pagina, int PorPagina, List<SseResumenDto> Items);

public class ListarSse(ISseRepository repo)
{
    public async Task<ListarSseResultado> EjecutarAsync(string? busqueda, string? estado, string? area, int pagina, int porPagina)
    {
        var (total, items) = await repo.BuscarAsync(busqueda, estado, area, pagina, porPagina);
        return new ListarSseResultado(total, pagina, porPagina, items);
    }
}
