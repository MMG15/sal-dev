namespace Ssal.Application.Analisis;

public class EliminarPaquete(IAnalisisRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id)
    {
        var paquete = await repo.ObtenerPaquetePorIdAsync(id);
        if (paquete is null) return Resultado.Fallido("No encontrado");

        if (await repo.TienePaqueteUsoEnPresupuestosAsync(id))
            return Resultado.Fallido("No se puede eliminar: el paquete está incluido en uno o más presupuestos. Podés desactivarlo en su lugar.");

        repo.EliminarPaquete(paquete);
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
