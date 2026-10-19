namespace Ssal.Application.Clientes;

public class ActualizarCliente(IClienteRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, GuardarClienteCommand cmd)
    {
        var cliente = await repo.ObtenerAsync(id);
        if (cliente is null) return Resultado.Fallido("No encontrado");

        var duplicado = await repo.BuscarDuplicadoAsync(cmd.Email, cmd.Cuit, excluirId: id);
        if (duplicado is not null)
            return Resultado.Fallido($"Ya existe un cliente con esos datos: {duplicado.Nombre} {duplicado.Apellido} (ID {duplicado.IdCliente}).");

        cliente.IdEmpresa = cmd.IdEmpresa;
        cliente.Nombre = cmd.Nombre;
        cliente.Apellido = cmd.Apellido;
        cliente.Email = cmd.Email;
        cliente.Telefono = cmd.Telefono;
        cliente.Cuit = cmd.Cuit;
        cliente.CondicionIva = cmd.CondicionIva;
        cliente.UsuarioWeb = cmd.UsuarioWeb;

        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
