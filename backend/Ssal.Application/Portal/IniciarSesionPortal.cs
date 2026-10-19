using Ssal.Application.Comun;

namespace Ssal.Application.Portal;

public class IniciarSesionPortal(IPortalRepository repo, IHashDeContrasenas hash, IGeneradorTokenPortal token)
{
    private const string RolPortal = "PORTAL";

    public async Task<Resultado<LoginPortalResultadoDto>> EjecutarAsync(string email, string password)
    {
        var cuenta = await repo.ObtenerCuentaPorEmailAsync(email.Trim().ToLowerInvariant());

        if (cuenta is null || !hash.Verificar(password, cuenta.PasswordHash))
            return Resultado<LoginPortalResultadoDto>.Fallido("Credenciales incorrectas.");

        cuenta.UltimoAcceso = DateTime.UtcNow;
        await repo.GuardarCambiosAsync();

        return Resultado<LoginPortalResultadoDto>.Exitoso(new LoginPortalResultadoDto(
            token.GenerarPara(cuenta), cuenta.IdPortalUsuario, cuenta.Cliente.Nombre, cuenta.Cliente.Apellido,
            cuenta.Email, RolPortal, "Cliente externo", cuenta.ForzarCambioPwd));
    }
}
