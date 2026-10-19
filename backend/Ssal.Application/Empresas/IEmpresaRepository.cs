namespace Ssal.Application.Empresas;

public interface IEmpresaRepository
{
    Task<List<Ssal.Domain.Empresa>> BuscarConClientesAsync(string? busqueda);

    Task<Ssal.Domain.Empresa?> ObtenerConClientesAsync(int id);

    Task<Ssal.Domain.Empresa?> ObtenerAsync(int id);

    // excluirId: para no chocar contra sí misma al editar.
    Task<Ssal.Domain.Empresa?> BuscarDuplicadaPorCuitAsync(string cuit, int? excluirId);

    void Agregar(Ssal.Domain.Empresa empresa);

    Task GuardarCambiosAsync();
}
