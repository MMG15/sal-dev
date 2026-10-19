namespace Ssal.Application.Empresas;

public class ActualizarEmpresa(IEmpresaRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, GuardarEmpresaCommand cmd)
    {
        var empresa = await repo.ObtenerAsync(id);
        if (empresa is null) return Resultado.Fallido("No encontrada");

        if (!string.IsNullOrWhiteSpace(cmd.Cuit))
        {
            var duplicada = await repo.BuscarDuplicadaPorCuitAsync(cmd.Cuit.Trim(), excluirId: id);
            if (duplicada is not null)
                return Resultado.Fallido($"Ya existe una empresa con ese CUIT: {duplicada.RazonSocial} (ID {duplicada.IdEmpresa}).");
        }

        empresa.RazonSocial = cmd.RazonSocial;
        empresa.Cuit = cmd.Cuit;
        empresa.CondicionIva = cmd.CondicionIva;
        empresa.Email = cmd.Email;
        empresa.Telefono = cmd.Telefono;
        empresa.Direccion = cmd.Direccion;
        empresa.UpdatedAt = DateTime.UtcNow;

        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
