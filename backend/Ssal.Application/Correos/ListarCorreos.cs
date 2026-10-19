namespace Ssal.Application.Correos;

public record ListarCorreosResultado(int Total, int Pagina, int PorPagina, int Pendientes, List<CorreoResumenDto> Items);

public class ListarCorreos(ICorreoEntranteRepository repo)
{
    public async Task<ListarCorreosResultado> EjecutarAsync(string? busqueda, string? estado, int pagina, int porPagina)
    {
        var (total, pendientes, items) = await repo.BuscarAsync(busqueda, estado, pagina, porPagina);
        return new ListarCorreosResultado(total, pagina, porPagina, pendientes, items);
    }
}
