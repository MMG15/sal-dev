namespace Ssal.Application.Portal;

// Rótulos del cliente con su estado: en análisis, disponible o rechazada.
public class ListarPedidos(IPortalRepository repo)
{
    public async Task<List<PedidoDto>?> EjecutarAsync(int idCuenta)
    {
        var cuenta = await repo.ObtenerCuentaAsync(idCuenta);
        if (cuenta is null) return null;

        var muestras = await repo.ListarMuestrasDelClienteAsync(cuenta.IdCliente);
        var firmas = await repo.ObtenerFirmasDeInformesAsync(muestras.Where(m => m.Informe is not null).Select(m => m.Informe!.IdInforme));

        return muestras.Select(m =>
        {
            var areas = m.Resultados.Select(r => r.Analisis.Area).Distinct().ToList();
            var aprobado = m.Informe is not null && EstadoInforme.EstaAprobado(m.Informe.IdInforme, areas, firmas);

            var estado = m.Estado == "rechazada" ? "rechazada"
                : aprobado ? "disponible"
                : "en_analisis";

            return new PedidoDto(
                m.IdMuestra, m.RotuloInterno.NumeroUnico, m.RotuloInterno.Sse.Codigo, m.FechaRecepcion, estado,
                aprobado ? new InformeResumenDto(m.Informe!.IdInforme, m.Informe.Codigo, m.Informe.FechaGeneracion) : null);
        }).ToList();
    }
}
