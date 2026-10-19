namespace Ssal.Application.Empresas;

public class CrearEmpresa(IEmpresaRepository repo)
{
    public async Task<Resultado<EmpresaResponseDto>> EjecutarAsync(GuardarEmpresaCommand cmd)
    {
        if (!string.IsNullOrWhiteSpace(cmd.Cuit))
        {
            var duplicada = await repo.BuscarDuplicadaPorCuitAsync(cmd.Cuit.Trim(), excluirId: null);
            if (duplicada is not null)
                return Resultado<EmpresaResponseDto>.Fallido(
                    $"Ya existe una empresa con ese CUIT: {duplicada.RazonSocial} (ID {duplicada.IdEmpresa}).");
        }

        var empresa = new Ssal.Domain.Empresa
        {
            RazonSocial = cmd.RazonSocial,
            Cuit = cmd.Cuit,
            CondicionIva = cmd.CondicionIva,
            Email = cmd.Email,
            Telefono = cmd.Telefono,
            Direccion = cmd.Direccion
        };
        repo.Agregar(empresa);
        await repo.GuardarCambiosAsync();

        return Resultado<EmpresaResponseDto>.Exitoso(new EmpresaResponseDto(
            empresa.IdEmpresa, empresa.RazonSocial, empresa.Cuit, empresa.CondicionIva,
            empresa.Email, empresa.Telefono, empresa.Direccion, empresa.Estado, 0));
    }
}
