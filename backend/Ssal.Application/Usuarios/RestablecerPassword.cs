using Ssal.Application.Comun;

namespace Ssal.Application.Usuarios;

public class RestablecerPassword(IUsuarioRepository repo, IHashDeContrasenas hash)
{
    public async Task<Resultado<UsuarioConPasswordDto>> EjecutarAsync(int id, int idUsuarioActual)
    {
        var usuario = await repo.ObtenerAsync(id);
        if (usuario is null) return Resultado<UsuarioConPasswordDto>.Fallido("No encontrado");

        var temporal = Contrasenas.Temporal();
        usuario.PasswordHash = hash.Hashear(temporal);
        usuario.ForzarCambioPwd = true;

        repo.AgregarAuditoria(new Ssal.Domain.AuditoriaSistema
        {
            IdUsuario = idUsuarioActual,
            Accion = "reset_password",
            Objetivo = usuario.Email,
            Detalle = null
        });
        await repo.GuardarCambiosAsync();

        return Resultado<UsuarioConPasswordDto>.Exitoso(new UsuarioConPasswordDto(usuario.Email, temporal));
    }
}
