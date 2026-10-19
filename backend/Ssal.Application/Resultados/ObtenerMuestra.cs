namespace Ssal.Application.Resultados;

public class ObtenerMuestra(IResultadoRepository repo)
{
    public async Task<MuestraDetalleDto?> EjecutarAsync(int id)
    {
        var m = await repo.ObtenerDetalleAsync(id);
        if (m is null) return null;

        var sse = m.RotuloInterno.Sse;
        return new MuestraDetalleDto(
            m.IdMuestra, m.Tipo, m.Estado, m.FechaRecepcion, m.RotuloInterno.NumeroUnico, sse.Codigo,
            sse.Cliente == null ? null : new ClienteNombreDto(sse.Cliente.Nombre, sse.Cliente.Apellido),
            sse.Cliente == null || sse.Cliente.Empresa == null ? null : new EmpresaNombreDto(sse.Cliente.Empresa.RazonSocial),
            m.Informe == null ? null : new InformeResumenDto(m.Informe.IdInforme, m.Informe.Codigo, m.Informe.FechaGeneracion),
            m.Resultados.Count > 0 && m.Resultados.All(r => r.Estado == "validado"),
            m.Resultados
                .OrderBy(r => r.Analisis.Area).ThenBy(r => r.Analisis.Nombre)
                .Select(r => new ResultadoDto(
                    r.IdResultado, r.Valor, r.Estado, r.FechaCarga, r.MotivoRechazo,
                    r.CargadoPorUsuario == null ? null : new UsuarioNombreDto(r.CargadoPorUsuario.Nombre, r.CargadoPorUsuario.Apellido),
                    r.ValidadoPorUsuario == null ? null : new UsuarioNombreDto(r.ValidadoPorUsuario.Nombre, r.ValidadoPorUsuario.Apellido),
                    r.FechaValidacion,
                    new AnalisisDetalleDto(r.Analisis.IdAnalisis, r.Analisis.Codigo, r.Analisis.Nombre, r.Analisis.Area, r.Analisis.Unidad,
                        r.Analisis.RangoMin, r.Analisis.RangoMax, r.Analisis.ValorEsperado, r.Analisis.Grupo.CodigoAgrupador),
                    Ssal.Domain.EvaluadorDeRango.FueraDeRango(r.Valor, r.Analisis)))
                .ToList());
    }
}
