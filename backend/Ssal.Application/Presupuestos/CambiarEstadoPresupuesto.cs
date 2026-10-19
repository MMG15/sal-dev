namespace Ssal.Application.Presupuestos;

public class CambiarEstadoPresupuesto(IPresupuestoRepository repo)
{
    public async Task<bool> EjecutarAsync(int id, CambiarEstadoPresupuestoCommand cmd)
    {
        var p = await repo.ObtenerAsync(id);
        if (p is null) return false;

        p.Estado = cmd.Estado;
        await repo.GuardarCambiosAsync();
        return true;
    }
}
