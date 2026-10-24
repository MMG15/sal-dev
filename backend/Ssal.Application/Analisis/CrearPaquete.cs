namespace Ssal.Application.Analisis;

public class CrearPaquete(IAnalisisRepository repo)
{
    public async Task<Resultado<int>> EjecutarAsync(GuardarPaqueteCommand cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd.Nombre))
            return Resultado<int>.Fallido("El nombre del paquete es obligatorio.");
        if (cmd.IdsAnalisis.Count == 0)
            return Resultado<int>.Fallido("El paquete tiene que incluir al menos un análisis.");

        var analisis = await repo.ObtenerAnalisisPorIdsAsync(cmd.IdsAnalisis);
        if (analisis.Count != cmd.IdsAnalisis.Distinct().Count())
            return Resultado<int>.Fallido("Alguno de los análisis seleccionados no existe.");

        var paquete = new Ssal.Domain.Paquete
        {
            Nombre = cmd.Nombre.Trim(),
            Area = cmd.Area,
            TipoMuestra = cmd.TipoMuestra,
            Norma = cmd.Norma,
            CantidadMuestra = cmd.CantidadMuestra,
            PrecioUsd = cmd.PrecioUsd,
            Activo = true,
            Items = analisis.Select(a => new Ssal.Domain.PaqueteAnalisis { IdAnalisis = a.IdAnalisis }).ToList()
        };

        repo.AgregarPaquete(paquete);
        await repo.GuardarCambiosAsync();

        return Resultado<int>.Exitoso(paquete.IdPaquete);
    }
}
