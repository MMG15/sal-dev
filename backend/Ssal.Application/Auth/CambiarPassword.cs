namespace Ssal.Application.Auth;

public class CambiarPassword(IAuthRepository repo, Comun.IHashDeContrasenas hash)
{
    public async Task<Resultado> EjecutarAsync(int idUsuario, string passwordActual, string? passwordNueva)
    {
        var usuario = await repo.ObtenerAsync(idUsuario);
        if (usuario is null) return Resultado.Fallido("No encontrado");

        if (!hash.Verificar(passwordActual, usuario.PasswordHash))
            return Resultado.Fallido("La contraseña actual no es correcta.");

        if (string.IsNullOrEmpty(passwordNueva) || passwordNueva.Length < 8)
            return Resultado.Fallido("La nueva contraseña debe tener al menos 8 caracteres.");

        usuario.PasswordHash = hash.Hashear(passwordNueva);
        usuario.ForzarCambioPwd = false;
        repo.AgregarAuditoria(new Ssal.Domain.AuditoriaSistema { IdUsuario = idUsuario, Accion = "cambio_password", Objetivo = usuario.Email });
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
