namespace Ssal.Application.Facturas;

public record ListarFacturasResultado(int Total, int Pagina, int PorPagina, List<FacturaResumenDto> Items);

public class ListarFacturas(IFacturaRepository repo)
{
    public async Task<ListarFacturasResultado> EjecutarAsync(string? busqueda, string? estado, int pagina, int porPagina)
    {
        var (total, items) = await repo.BuscarAsync(busqueda, estado, pagina, porPagina);
        return new ListarFacturasResultado(total, pagina, porPagina, items);
    }
}
