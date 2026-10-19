using Ssal.Application.Comun;

namespace Ssal.Application.PortalAccesos;

public class GenerarOrestablecerAcceso(IPortalAccesoRepository repo, IHashDeContrasenas hash)
{
    public async Task<Resultado<AccesoConPasswordDto>> EjecutarAsync(int idCliente, GenerarAccesoCommand cmd, int idUsuarioActual)
    {
        var cliente = await repo.ObtenerClienteAsync(idCliente);
        if (cliente is null) return Resultado<AccesoConPasswordDto>.Fallido("No encontrado");

        var email = (cmd.Email ?? cliente.Email)?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return Resultado<AccesoConPasswordDto>.Fallido("Indicá un correo electrónico para el acceso del cliente.");

        if (await repo.EmailEnUsoPorOtroClienteAsync(email, idCliente))
            return Resultado<AccesoConPasswordDto>.Fallido("Ese correo ya tiene acceso al portal de otro cliente.");

        var temporal = Contrasenas.Temporal();
        var passwordHash = hash.Hashear(temporal);

        var acceso = await repo.ObtenerPorClienteAsync(idCliente);
        if (acceso is null)
        {
            repo.Agregar(new Ssal.Domain.PortalUsuario
            {
                IdCliente = idCliente,
                Email = email,
                PasswordHash = passwordHash,
                ForzarCambioPwd = true,
                Activo = true,
                CreadoPor = idUsuarioActual
            });
        }
        else
        {
            acceso.Email = email;
            acceso.PasswordHash = passwordHash;
            acceso.ForzarCambioPwd = true;
            acceso.Activo = true;
        }

        await repo.GuardarCambiosAsync();
        return Resultado<AccesoConPasswordDto>.Exitoso(new AccesoConPasswordDto(email, temporal));
    }
}
