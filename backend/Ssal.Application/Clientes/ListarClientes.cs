namespace Ssal.Application.Clientes;

public class ListarClientes(IClienteRepository repo)
{
    public async Task<List<ClienteResponseDto>> EjecutarAsync(string? busqueda)
    {
        var clientes = await repo.BuscarConEmpresaAsync(busqueda);

        return clientes
            .OrderBy(c => c.Apellido)
            .ThenBy(c => c.Nombre)
            .Select(c => new ClienteResponseDto(
                c.IdCliente, c.Nombre, c.Apellido, c.Email, c.Telefono, c.Cuit, c.CondicionIva,
                c.UsuarioWeb, c.Estado, c.IdEmpresa, c.Empresa != null ? c.Empresa.RazonSocial : null, c.CreatedAt))
            .ToList();
    }
}
