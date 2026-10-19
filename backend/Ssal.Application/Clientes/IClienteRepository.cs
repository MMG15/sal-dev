namespace Ssal.Application.Clientes;

public interface IClienteRepository
{
    Task<List<Ssal.Domain.Cliente>> BuscarConEmpresaAsync(string? busqueda);

    Task<Ssal.Domain.Cliente?> ObtenerConEmpresaAsync(int id);

    Task<Ssal.Domain.Cliente?> ObtenerAsync(int id);

    Task<Ssal.Domain.Cliente?> BuscarDuplicadoAsync(string? email, string? cuit, int? excluirId);

    void Agregar(Ssal.Domain.Cliente cliente);

    Task GuardarCambiosAsync();
}
