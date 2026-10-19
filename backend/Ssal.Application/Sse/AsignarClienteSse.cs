namespace Ssal.Application.Sse;

public class AsignarClienteSse(ISseRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, AsignarClienteSseCommand cmd)
    {
        var s = await repo.ObtenerAsync(id);
        if (s is null) return Resultado.Fallido("No encontrado");

        if (cmd.IdCliente is not null && !await repo.ExisteClienteAsync(cmd.IdCliente.Value))
            return Resultado.Fallido("El cliente indicado no existe.");
        if (cmd.IdEmpresa is not null && !await repo.ExisteEmpresaAsync(cmd.IdEmpresa.Value))
            return Resultado.Fallido("La empresa indicada no existe.");

        s.IdCliente = cmd.IdCliente;
        s.IdEmpresa = cmd.IdEmpresa;
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
