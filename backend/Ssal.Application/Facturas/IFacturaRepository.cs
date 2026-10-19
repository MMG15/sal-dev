namespace Ssal.Application.Facturas;

public interface IFacturaRepository
{
    // Devuelve el DTO ya armado (no la entidad): el orden depende de cómo la base trata los
    // valores nulos de Cliente en el ORDER BY, así que se preserva igual que estaba, a nivel SQL.
    Task<(int Total, List<FacturaResumenDto> Items)> BuscarAsync(string? busqueda, string? estado, int pagina, int porPagina);

    Task<List<SseDisponibleDto>> ListarSsesSinFacturaAsync();

    Task<Ssal.Domain.Sse?> ObtenerSseAsync(int idSse);

    Task<bool> SseTieneFacturaActivaAsync(int idSse);

    Task<bool> ExisteNumeroAsync(string numero);

    void Agregar(Ssal.Domain.Factura factura);

    Task<Ssal.Domain.Factura?> ObtenerAsync(int id);

    Task GuardarCambiosAsync();
}
