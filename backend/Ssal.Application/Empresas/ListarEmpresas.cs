namespace Ssal.Application.Empresas;

public class ListarEmpresas(IEmpresaRepository repo)
{
    public async Task<List<EmpresaResponseDto>> EjecutarAsync(string? busqueda)
    {
        var empresas = await repo.BuscarConClientesAsync(busqueda);

        return empresas
            .OrderBy(e => e.RazonSocial)
            .Select(e => new EmpresaResponseDto(
                e.IdEmpresa, e.RazonSocial, e.Cuit, e.CondicionIva, e.Email, e.Telefono, e.Direccion,
                e.Estado, e.Clientes.Count(c => c.Estado == "activo")))
            .ToList();
    }
}
