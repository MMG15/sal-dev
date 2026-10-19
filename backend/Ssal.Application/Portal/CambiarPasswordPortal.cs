using Ssal.Application.Comun;

namespace Ssal.Application.Portal;

public class CambiarPasswordPortal(IPortalRepository repo, IHashDeContrasenas hash)
{
    public async Task<Resultado> EjecutarAsync(int idCuenta, CambiarPasswordPortalCommand cmd)
    {
        var cuenta = await repo.ObtenerCuentaAsync(idCuenta);
        if (cuenta is null) return Resultado.Fallido("No encontrado");

        if (!hash.Verificar(cmd.PasswordActual, cuenta.PasswordHash))
            return Resultado.Fallido("La contraseña actual no es correcta.");

        if (string.IsNullOrEmpty(cmd.PasswordNueva) || cmd.PasswordNueva.Length < 8)
            return Resultado.Fallido("La nueva contraseña debe tener al menos 8 caracteres.");

        cuenta.PasswordHash = hash.Hashear(cmd.PasswordNueva);
        cuenta.ForzarCambioPwd = false;
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
