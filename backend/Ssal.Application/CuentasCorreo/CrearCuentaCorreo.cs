using Ssal.Application.Comun;

namespace Ssal.Application.CuentasCorreo;

public class CrearCuentaCorreo(ICuentaCorreoRepository repo, ICifradoContrasenas cifrado)
{
    public async Task<Resultado<CuentaCreadaDto>> EjecutarAsync(GuardarCuentaCorreoCommand cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd.Nombre) || string.IsNullOrWhiteSpace(cmd.Usuario))
            return Resultado<CuentaCreadaDto>.Fallido("Nombre y usuario son obligatorios.");
        if (string.IsNullOrWhiteSpace(cmd.Password))
            return Resultado<CuentaCreadaDto>.Fallido("Ingresá la contraseña de aplicación de la casilla.");

        var cuenta = new Ssal.Domain.CuentaCorreo
        {
            Nombre = cmd.Nombre.Trim(),
            Servidor = string.IsNullOrWhiteSpace(cmd.Servidor) ? "imap.gmail.com" : cmd.Servidor.Trim(),
            Puerto = cmd.Puerto is > 0 ? cmd.Puerto.Value : 993,
            Usuario = cmd.Usuario.Trim(),
            PasswordCifrada = cifrado.Encriptar(LimpiadorPassword.Limpiar(cmd.Password)),
            Habilitada = cmd.Habilitada ?? true,
            IntervaloMinutos = cmd.IntervaloMinutos is > 0 ? cmd.IntervaloMinutos.Value : 5
        };
        repo.Agregar(cuenta);
        await repo.GuardarCambiosAsync();

        return Resultado<CuentaCreadaDto>.Exitoso(new CuentaCreadaDto(cuenta.IdCuenta));
    }
}
