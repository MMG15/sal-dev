using Ssal.Application.Comun;

namespace Ssal.Application.Presupuestos;

public class CrearPresupuesto(IPresupuestoRepository repo)
{
    public async Task<Resultado<CrearPresupuestoResultadoDto>> EjecutarAsync(GuardarPresupuestoCommand cmd, int idUsuario)
    {
        var consulta = await repo.ObtenerConsultaAsync(cmd.IdConsulta);
        if (consulta is null) return Resultado<CrearPresupuestoResultadoDto>.Fallido("Consulta no encontrada");

        // Si ya existe uno para esta consulta, se actualiza en el lugar (mismo Id y mismo código RPO) en
        // vez de borrarlo y crear uno nuevo: si ya se convirtió en SSE, ésta sigue apuntando al mismo
        // presupuesto por Id, y borrarlo violaría esa referencia.
        var existente = await repo.ObtenerExistentePorConsultaAsync(cmd.IdConsulta);

        // Obtener precios actuales de los análisis y paquetes seleccionados
        var ids = cmd.Items.Select(i => i.IdAnalisis).ToList();
        var analisis = await repo.ObtenerAnalisisPorIdsAsync(ids);

        var items = cmd.Items.Select(i => new Ssal.Domain.PresupuestoAnalisis
        {
            IdAnalisis = i.IdAnalisis,
            PrecioUsdSnapshot = analisis.TryGetValue(i.IdAnalisis, out var a) ? a.PrecioUsd : 0
        }).ToList();

        var idsPaquetes = (cmd.Paquetes ?? []).Select(p => p.IdPaquete).ToList();
        var paquetesCatalogo = await repo.ObtenerPaquetesPorIdsAsync(idsPaquetes);

        var paquetes = (cmd.Paquetes ?? []).Select(p => new Ssal.Domain.PresupuestoPaquete
        {
            IdPaquete = p.IdPaquete,
            PrecioUsdSnapshot = paquetesCatalogo.TryGetValue(p.IdPaquete, out var pq) ? pq.PrecioUsd : 0
        }).ToList();

        var totalUsd = items.Sum(i => i.PrecioUsdSnapshot) + paquetes.Sum(p => p.PrecioUsdSnapshot) + cmd.AdicionalAsesoramiento;
        var cotizacion = cmd.Cotizacion > 0 ? cmd.Cotizacion : 1;
        var mostrarMoneda = cmd.MostrarMoneda is "usd" or "ars" or "ambos" ? cmd.MostrarMoneda : "ambos";

        Ssal.Domain.Presupuesto presupuesto;

        if (existente is not null)
        {
            repo.ReemplazarItemsYPaquetes(existente, items, paquetes);
            existente.ImporteDolares = totalUsd;
            existente.ImportePesos = totalUsd * cotizacion;
            existente.Cotizacion = cotizacion;
            existente.AdicionalAsesoramiento = cmd.AdicionalAsesoramiento;
            existente.TipoAsesoramiento = cmd.AdicionalAsesoramiento > 0 ? cmd.TipoAsesoramiento : null;
            existente.MostrarMoneda = mostrarMoneda;
            existente.Estado = "borrador";
            presupuesto = existente;
        }
        else
        {
            var hoy = DateTime.UtcNow;
            var prefijo = $"RPO-{hoy:yyyyMMdd}-";
            var ultimoCodigo = await repo.ObtenerUltimoCodigoAsync(prefijo);
            var codigo = GeneradorCodigoCorrelativo.Siguiente(prefijo, ultimoCodigo);

            presupuesto = new Ssal.Domain.Presupuesto
            {
                IdConsulta = cmd.IdConsulta,
                CreadoPor = idUsuario,
                CodigoRpo = codigo,
                ImporteDolares = totalUsd,
                ImportePesos = totalUsd * cotizacion,
                Cotizacion = cotizacion,
                AdicionalAsesoramiento = cmd.AdicionalAsesoramiento,
                TipoAsesoramiento = cmd.AdicionalAsesoramiento > 0 ? cmd.TipoAsesoramiento : null,
                MostrarMoneda = mostrarMoneda,
                Estado = "borrador",
                Fecha = hoy,
                Items = items,
                Paquetes = paquetes
            };
            repo.Agregar(presupuesto);
        }

        await repo.GuardarCambiosAsync();

        return Resultado<CrearPresupuestoResultadoDto>.Exitoso(new CrearPresupuestoResultadoDto(
            presupuesto.IdPresupuesto, presupuesto.CodigoRpo, presupuesto.ImporteDolares, presupuesto.ImportePesos));
    }
}
