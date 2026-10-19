using Ssal.Application.Comun;

namespace Ssal.Application.Presupuestos;

public class CrearPresupuesto(IPresupuestoRepository repo)
{
    public async Task<Resultado<CrearPresupuestoResultadoDto>> EjecutarAsync(GuardarPresupuestoCommand cmd, int idUsuario)
    {
        var consulta = await repo.ObtenerConsultaAsync(cmd.IdConsulta);
        if (consulta is null) return Resultado<CrearPresupuestoResultadoDto>.Fallido("Consulta no encontrada");

        // Si ya existe uno para esta consulta, eliminarlo
        var existente = await repo.ObtenerExistentePorConsultaAsync(cmd.IdConsulta);
        if (existente is not null)
        {
            repo.EliminarConItems(existente);
            await repo.GuardarCambiosAsync();
        }

        // Obtener precios actuales de los análisis seleccionados
        var ids = cmd.Items.Select(i => i.IdAnalisis).ToList();
        var analisis = await repo.ObtenerAnalisisPorIdsAsync(ids);

        var items = cmd.Items.Select(i => new Ssal.Domain.PresupuestoAnalisis
        {
            IdAnalisis = i.IdAnalisis,
            PrecioUsdSnapshot = analisis.TryGetValue(i.IdAnalisis, out var a) ? a.PrecioUsd : 0
        }).ToList();

        var totalUsd = items.Sum(i => i.PrecioUsdSnapshot) + cmd.AdicionalAsesoramiento;
        var cotizacion = cmd.Cotizacion > 0 ? cmd.Cotizacion : 1;

        var hoy = DateTime.UtcNow;
        var prefijo = $"RPO-{hoy:yyyyMMdd}-";
        var ultimoCodigo = await repo.ObtenerUltimoCodigoAsync(prefijo);
        var codigo = GeneradorCodigoCorrelativo.Siguiente(prefijo, ultimoCodigo);

        var presupuesto = new Ssal.Domain.Presupuesto
        {
            IdConsulta = cmd.IdConsulta,
            CreadoPor = idUsuario,
            CodigoRpo = codigo,
            ImporteDolares = totalUsd,
            ImportePesos = totalUsd * cotizacion,
            Cotizacion = cotizacion,
            AdicionalAsesoramiento = cmd.AdicionalAsesoramiento,
            TipoAsesoramiento = cmd.AdicionalAsesoramiento > 0 ? cmd.TipoAsesoramiento : null,
            Estado = "borrador",
            Fecha = hoy,
            Items = items
        };

        repo.Agregar(presupuesto);
        await repo.GuardarCambiosAsync();

        return Resultado<CrearPresupuestoResultadoDto>.Exitoso(new CrearPresupuestoResultadoDto(
            presupuesto.IdPresupuesto, presupuesto.CodigoRpo, presupuesto.ImporteDolares, presupuesto.ImportePesos));
    }
}
