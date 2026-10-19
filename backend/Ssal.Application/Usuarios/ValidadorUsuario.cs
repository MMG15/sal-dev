namespace Ssal.Application.Usuarios;

// Compartido por CrearUsuario y ActualizarUsuario. El mensaje "Ya existe un usuario con ese
// correo." es justamente el que el controller reconoce para devolver 409 en vez de 400 — si se
// le cambia el texto acá, hay que actualizarlo también ahí.
internal class ValidadorUsuario(IUsuarioRepository repo)
{
    public const string RolNoAsignable = "ROL-05"; // clientes externos: tienen su propio acceso al portal

    public async Task<string?> ValidarAsync(GuardarUsuarioCommand cmd, int? idExcluido)
    {
        if (string.IsNullOrWhiteSpace(cmd.Nombre) || string.IsNullOrWhiteSpace(cmd.Apellido))
            return "Nombre y apellido son obligatorios.";

        if (string.IsNullOrWhiteSpace(cmd.Email) || !cmd.Email.Contains('@'))
            return "Indicá un correo electrónico válido.";

        var rol = await repo.ObtenerRolAsync(cmd.IdRol);
        if (rol is null || rol.Codigo == RolNoAsignable)
            return "Rol no válido.";

        var email = cmd.Email.Trim().ToLowerInvariant();
        if (await repo.ExisteEmailAsync(email, idExcluido))
            return "Ya existe un usuario con ese correo.";

        return null;
    }
}
