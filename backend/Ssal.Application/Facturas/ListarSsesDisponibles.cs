namespace Ssal.Application.Facturas;

public class ListarSsesDisponibles(IFacturaRepository repo)
{
    public Task<List<SseDisponibleDto>> EjecutarAsync() => repo.ListarSsesSinFacturaAsync();
}
