namespace Ssal.Application.Portal;

public class ObtenerInformeDetalle(IPortalRepository repo)
{
    // Null = no existe o no es del cliente (se devuelve igual para no filtrar cuáles existen).
    // Resultado.Fallido = existe y es del cliente, pero todavía no está aprobado (403, no 404).
    public async Task<(InformeDetallePortalDto? Dto, bool Existe, bool Aprobado)> EjecutarAsync(int idInforme, int idCuenta)
    {
        var informe = await repo.ObtenerInformeParaDetalleAsync(idInforme);
        if (informe is null) return (null, false, false);

        var cuenta = await repo.ObtenerCuentaAsync(idCuenta);
        if (cuenta is null || !await repo.PerteneceAlClienteAsync(informe.Muestra.RotuloInterno.IdSse, cuenta.IdCliente))
            return (null, false, false);

        var firmas = await repo.ObtenerFirmasDeInformesAsync([idInforme]);
        var areas = informe.Muestra.Resultados.Select(r => r.Analisis.Area).Distinct().ToList();
        if (!EstadoInforme.EstaAprobado(idInforme, areas, firmas))
            return (null, true, false);

        var dto = new InformeDetallePortalDto(
            informe.IdInforme, informe.Codigo, informe.FechaGeneracion,
            informe.Muestra.RotuloInterno.NumeroUnico, informe.Muestra.RotuloInterno.Sse.Codigo,
            informe.Muestra.Resultados
                .OrderBy(r => r.Analisis.Area).ThenBy(r => r.Analisis.Nombre)
                .Select(r => new ResultadoPortalDto(r.Analisis.Area, r.Analisis.Nombre, r.Valor, r.Analisis.Unidad, EstadoInforme.ReferenciaTexto(r.Analisis)))
                .ToList());

        return (dto, true, true);
    }
}
