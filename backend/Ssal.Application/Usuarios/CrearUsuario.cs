using Ssal.Application.Comun;

namespace Ssal.Application.Usuarios;

public class CrearUsuario(IUsuarioRepository repo, IHashDeContrasenas hash)
{
    public async Task<Resultado<UsuarioConPasswordDto>> EjecutarAsync(GuardarUsuarioCommand cmd, int idUsuarioActual)
    {
        var error = await new ValidadorUsuario(repo).ValidarAsync(cmd, idExcluido: null);
        if (error is not null) return Resultado<UsuarioConPasswordDto>.Fallido(error);

        var email = cmd.Email.Trim().ToLowerInvariant();
        var temporal = Contrasenas.Temporal();

        var usuario = new Ssal.Domain.Usuario
        {
            Nombre = cmd.Nombre.Trim(),
            Apellido = cmd.Apellido.Trim(),
            Email = email,
            IdRol = cmd.IdRol,
            PasswordHash = hash.Hashear(temporal),
            Activo = true,
            ForzarCambioPwd = true
        };
        repo.Agregar(usuario);
        await repo.GuardarCambiosAsync();

        var rol = await repo.ObtenerRolAsync(cmd.IdRol);
        repo.AgregarAuditoria(new Ssal.Domain.AuditoriaSistema
        {
            IdUsuario = idUsuarioActual,
            Accion = "alta_usuario",
            Objetivo = email,
            Detalle = $"Alta con rol {rol?.Codigo ?? "?"}"
        });
        await repo.GuardarCambiosAsync();

        return Resultado<UsuarioConPasswordDto>.Exitoso(new UsuarioConPasswordDto(email, temporal));
    }
}
