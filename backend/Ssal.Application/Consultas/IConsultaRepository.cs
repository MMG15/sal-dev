namespace Ssal.Application.Consultas;

public interface IConsultaRepository
{
    Task<(int Total, List<Ssal.Domain.Consulta> Items)> BuscarAsync(
        string? busqueda, string? estado, DateTime? limitePendiente, int pagina, int porPagina);

    Task<Ssal.Domain.Consulta?> ObtenerDetalleAsync(int id);

    Task<Ssal.Domain.Consulta?> ObtenerAsync(int id);

    Task<string?> ObtenerUltimoCodigoAsync(string prefijo);

    void Agregar(Ssal.Domain.Consulta consulta);

    Task GuardarCambiosAsync();
}
