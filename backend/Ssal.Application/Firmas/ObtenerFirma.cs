namespace Ssal.Application.Firmas;

public class ObtenerFirma(CargadorEstadoDocumento cargador)
{
    public async Task<DetalleFirmaDto?> EjecutarAsync(string tipo, int id, string rolActual)
    {
        var estado = await cargador.CargarAsync(tipo, id);
        if (estado is null) return null;
        var (doc, eventos, ciclo) = estado.Value;

        return new DetalleFirmaDto(
            doc, ciclo.Estado, ciclo.AreasFirmadas.OrderBy(a => a).ToList(),
            MapeoDocumentos.CalcularPermisos(doc, ciclo, rolActual),
            eventos.Select(e => new EventoFirmaDto(
                e.IdFirma, e.TipoFirma, e.Area, e.Estado, e.MotivoRechazo, e.FechaFirma,
                $"{e.Firmante.Nombre} {e.Firmante.Apellido}"))
                .ToList());
    }
}
