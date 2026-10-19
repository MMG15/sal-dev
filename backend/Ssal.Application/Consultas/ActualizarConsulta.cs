namespace Ssal.Application.Consultas;

public class ActualizarConsulta(IConsultaRepository repo)
{
    public async Task<bool> EjecutarAsync(int id, ActualizarConsultaCommand cmd)
    {
        var c = await repo.ObtenerAsync(id);
        if (c is null) return false;

        if (cmd.Canal is not null) c.Canal = cmd.Canal;
        if (cmd.Estado is not null) c.Estado = cmd.Estado;
        if (cmd.Descripcion is not null) c.Descripcion = cmd.Descripcion;
        if (cmd.IdCliente.HasValue) c.IdCliente = cmd.IdCliente;
        if (cmd.IdEmpresa.HasValue) c.IdEmpresa = cmd.IdEmpresa;
        if (cmd.IdResponsable.HasValue) c.IdResponsable = cmd.IdResponsable;

        await repo.GuardarCambiosAsync();
        return true;
    }
}
