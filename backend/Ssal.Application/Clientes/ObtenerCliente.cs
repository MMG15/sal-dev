namespace Ssal.Application.Clientes;

public class ObtenerCliente(IClienteRepository repo)
{
    public async Task<ClienteResponseDto?> EjecutarAsync(int id)
    {
        var c = await repo.ObtenerConEmpresaAsync(id);
        if (c is null) return null;

        return new ClienteResponseDto(
            c.IdCliente, c.Nombre, c.Apellido, c.Email, c.Telefono, c.Cuit, c.CondicionIva,
            c.UsuarioWeb, c.Estado, c.IdEmpresa, c.Empresa?.RazonSocial, c.CreatedAt);
    }
}
