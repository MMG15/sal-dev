namespace Ssal.Application.Presupuestos;

public record ListarPresupuestosResultado(int Total, int Pagina, int PorPagina, List<PresupuestoResumenDto> Items);

public class ListarPresupuestos(IPresupuestoRepository repo)
{
    public async Task<ListarPresupuestosResultado> EjecutarAsync(string? busqueda, string? estado, int pagina, int porPagina)
    {
        var (total, items) = await repo.BuscarAsync(busqueda, estado, pagina, porPagina);
        return new ListarPresupuestosResultado(total, pagina, porPagina, items);
    }
}
