namespace Ssal.Application.Correos;

public interface ICorreoEntranteRepository
{
    Task<(int Total, int Pendientes, List<CorreoResumenDto> Items)> BuscarAsync(string? busqueda, string? estado, int pagina, int porPagina);

    Task<Ssal.Domain.CorreoEntrante?> ObtenerDetalleAsync(int id);

    Task<Ssal.Domain.CorreoEntrante?> ObtenerConCuentaAsync(int id);

    Task<Ssal.Domain.CorreoEntrante?> ObtenerAsync(int id);

    Task<Ssal.Domain.CuentaCorreo?> ObtenerPrimeraCuentaHabilitadaAsync();

    Task<string?> ObtenerUltimoCodigoConsultaAsync(string prefijo);

    void AgregarConsulta(Ssal.Domain.Consulta consulta);

    Task GuardarCambiosAsync();
}
