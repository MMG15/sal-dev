namespace Ssal.Application.Auth;

public class IniciarSesion(IAuthRepository repo, Comun.IHashDeContrasenas hash, IGeneradorToken token)
{
    public async Task<Resultado<LoginResultadoDto>> EjecutarAsync(string email, string password)
    {
        var usuario = await repo.ObtenerActivoPorEmailAsync(email);

        if (usuario is null || !hash.Verificar(password, usuario.PasswordHash))
        {
            repo.AgregarAuditoria(new Ssal.Domain.AuditoriaSistema
            {
                Accion = "login_fallido",
                Objetivo = email,
                Detalle = "Credenciales incorrectas"
            });
            await repo.GuardarCambiosAsync();
            return Resultado<LoginResultadoDto>.Fallido("Credenciales incorrectas.");
        }

        usuario.UltimoAcceso = DateTime.UtcNow;
        repo.AgregarAuditoria(new Ssal.Domain.AuditoriaSistema
        {
            IdUsuario = usuario.IdUsuario,
            Accion = "login_ok",
            Objetivo = usuario.Email
        });
        await repo.GuardarCambiosAsync();

        var tokenGenerado = token.GenerarPara(usuario);

        return Resultado<LoginResultadoDto>.Exitoso(new LoginResultadoDto(
            tokenGenerado, usuario.IdUsuario, usuario.Nombre, usuario.Apellido, usuario.Email,
            usuario.Rol.Codigo, usuario.Rol.Nombre, usuario.ForzarCambioPwd));
    }
}
