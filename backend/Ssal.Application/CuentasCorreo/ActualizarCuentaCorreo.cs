using Ssal.Application.Comun;

namespace Ssal.Application.CuentasCorreo;

// La contraseña es opcional: si no se manda, se conserva la que ya estaba.
public class ActualizarCuentaCorreo(ICuentaCorreoRepository repo, ICifradoContrasenas cifrado)
{
    public async Task<Resultado> EjecutarAsync(int id, GuardarCuentaCorreoCommand cmd)
    {
        var cuenta = await repo.ObtenerAsync(id);
        if (cuenta is null) return Resultado.Fallido("No encontrado");

        if (string.IsNullOrWhiteSpace(cmd.Nombre) || string.IsNullOrWhiteSpace(cmd.Usuario))
            return Resultado.Fallido("Nombre y usuario son obligatorios.");

        cuenta.Nombre = cmd.Nombre.Trim();
        cuenta.Servidor = string.IsNullOrWhiteSpace(cmd.Servidor) ? "imap.gmail.com" : cmd.Servidor.Trim();
        cuenta.Puerto = cmd.Puerto is > 0 ? cmd.Puerto.Value : 993;
        cuenta.Usuario = cmd.Usuario.Trim();
        cuenta.Habilitada = cmd.Habilitada ?? cuenta.Habilitada;
        cuenta.IntervaloMinutos = cmd.IntervaloMinutos is > 0 ? cmd.IntervaloMinutos.Value : cuenta.IntervaloMinutos;
        if (!string.IsNullOrWhiteSpace(cmd.Password))
            cuenta.PasswordCifrada = cifrado.Encriptar(LimpiadorPassword.Limpiar(cmd.Password));
        cuenta.UpdatedAt = DateTime.UtcNow;
        // Un cambio de credenciales o de casilla amerita volver a intentar desde cero
        cuenta.UltimoError = null;

        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
