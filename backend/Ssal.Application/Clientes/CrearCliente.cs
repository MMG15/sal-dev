namespace Ssal.Application.Clientes;

public class CrearCliente(IClienteRepository repo)
{
    // Nota: devuelve la entidad de dominio tal cual (no el DTO), igual que hacía el
    // controller original — se preserva así a propósito en esta migración mecánica.
    public async Task<Resultado<Ssal.Domain.Cliente>> EjecutarAsync(GuardarClienteCommand cmd)
    {
        var duplicado = await repo.BuscarDuplicadoAsync(cmd.Email, cmd.Cuit, excluirId: null);
        if (duplicado is not null)
            return Resultado<Ssal.Domain.Cliente>.Fallido(
                $"Ya existe un cliente con esos datos: {duplicado.Nombre} {duplicado.Apellido} (ID {duplicado.IdCliente}).");

        var cliente = new Ssal.Domain.Cliente
        {
            IdEmpresa = cmd.IdEmpresa,
            Nombre = cmd.Nombre,
            Apellido = cmd.Apellido,
            Email = cmd.Email,
            Telefono = cmd.Telefono,
            Cuit = cmd.Cuit,
            CondicionIva = cmd.CondicionIva,
            UsuarioWeb = cmd.UsuarioWeb
        };
        repo.Agregar(cliente);
        await repo.GuardarCambiosAsync();

        return Resultado<Ssal.Domain.Cliente>.Exitoso(cliente);
    }
}
