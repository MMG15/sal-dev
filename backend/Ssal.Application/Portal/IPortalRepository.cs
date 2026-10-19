namespace Ssal.Application.Portal;

public interface IPortalRepository
{
    Task<Ssal.Domain.PortalUsuario?> ObtenerCuentaPorEmailAsync(string email);

    Task<Ssal.Domain.PortalUsuario?> ObtenerCuentaAsync(int id);

    Task<List<Ssal.Domain.Muestra>> ListarMuestrasDelClienteAsync(int idCliente);

    Task<List<Ssal.Domain.FirmaDocumento>> ObtenerFirmasDeInformesAsync(IEnumerable<int> idsInformes);

    // Con los includes que necesita el detalle (sin PDF): rótulo/SSE y resultados+análisis.
    Task<Ssal.Domain.Informe?> ObtenerInformeParaDetalleAsync(int id);

    // Con todos los includes que necesita el PDF.
    Task<Ssal.Domain.Informe?> ObtenerInformeCompletoAsync(int id);

    Task<bool> PerteneceAlClienteAsync(int idSse, int idCliente);

    Task GuardarCambiosAsync();
}

// La implementación real (JWT) vive en Ssal.Infrastructure.
public interface IGeneradorTokenPortal
{
    string GenerarPara(Ssal.Domain.PortalUsuario cuenta);
}
