namespace Ssal.Application.PortalAccesos;

public interface IPortalAccesoRepository
{
    Task<Ssal.Domain.PortalUsuario?> ObtenerPorClienteAsync(int idCliente);

    Task<Ssal.Domain.Cliente?> ObtenerClienteAsync(int idCliente);

    Task<bool> EmailEnUsoPorOtroClienteAsync(string email, int idCliente);

    void Agregar(Ssal.Domain.PortalUsuario acceso);

    Task GuardarCambiosAsync();
}
