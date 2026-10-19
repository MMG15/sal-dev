namespace Ssal.Application.CuentasCorreo;

public class EliminarCuentaCorreo(ICuentaCorreoRepository repo)
{
    public async Task<bool> EjecutarAsync(int id)
    {
        var cuenta = await repo.ObtenerAsync(id);
        if (cuenta is null) return false;

        repo.Eliminar(cuenta);
        await repo.GuardarCambiosAsync();
        return true;
    }
}
