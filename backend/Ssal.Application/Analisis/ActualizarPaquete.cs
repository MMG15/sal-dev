namespace Ssal.Application.Analisis;

public class ActualizarPaquete(IAnalisisRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, GuardarPaqueteCommand cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd.Nombre))
            return Resultado.Fallido("El nombre del paquete es obligatorio.");
        if (cmd.IdsAnalisis.Count == 0)
            return Resultado.Fallido("El paquete tiene que incluir al menos un análisis.");

        var paquete = await repo.ObtenerPaquetePorIdAsync(id);
        if (paquete is null) return Resultado.Fallido("No encontrado");

        var analisis = await repo.ObtenerAnalisisPorIdsAsync(cmd.IdsAnalisis);
        if (analisis.Count != cmd.IdsAnalisis.Distinct().Count())
            return Resultado.Fallido("Alguno de los análisis seleccionados no existe.");

        paquete.Nombre = cmd.Nombre.Trim();
        paquete.Area = cmd.Area;
        paquete.TipoMuestra = cmd.TipoMuestra;
        paquete.Norma = cmd.Norma;
        paquete.CantidadMuestra = cmd.CantidadMuestra;
        paquete.PrecioUsd = cmd.PrecioUsd;
        repo.ReemplazarItemsDePaquete(paquete, analisis);

        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
