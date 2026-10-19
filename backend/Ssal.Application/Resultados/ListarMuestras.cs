namespace Ssal.Application.Resultados;

public record ListarMuestrasResultado(int Total, int Pagina, int PorPagina, List<MuestraResumenDto> Items);

// Lista de muestras con resultados, con resumen de avance de carga/validación.
public class ListarMuestras(IResultadoRepository repo)
{
    public async Task<ListarMuestrasResultado> EjecutarAsync(string? busqueda, string? estado, string? area, int pagina, int porPagina)
    {
        var todas = await repo.BuscarMuestrasAsync(busqueda, area);

        var proyectadas = todas.Select(m => new MuestraResumenDto(
            m.IdMuestra,
            m.RotuloInterno.NumeroUnico,
            m.RotuloInterno.Sse.Codigo,
            m.RotuloInterno.Sse.Cliente == null ? null : new ClienteNombreDto(m.RotuloInterno.Sse.Cliente.Nombre, m.RotuloInterno.Sse.Cliente.Apellido),
            m.RotuloInterno.Sse.Cliente == null || m.RotuloInterno.Sse.Cliente.Empresa == null ? null : new EmpresaNombreDto(m.RotuloInterno.Sse.Cliente.Empresa.RazonSocial),
            m.Resultados.Select(r => r.Analisis.Area).Distinct().OrderBy(a => a).ToList(),
            m.Resultados.Count,
            m.Resultados.Count(r => r.Estado == "pendiente" || r.Estado == "rechazado"),
            m.Resultados.Count(r => r.Estado == "cargado"),
            m.Resultados.Count(r => r.Estado == "validado"),
            m.FechaRecepcion,
            m.Resultados.All(r => r.Estado == "validado") ? "completo"
                : m.Resultados.Any(r => r.Estado == "cargado") ? "por_validar"
                : "pendiente"))
            .ToList();

        var filtradas = estado switch
        {
            "pendientes" => proyectadas.Where(p => p.EstadoGeneral == "pendiente"),
            "por_validar" => proyectadas.Where(p => p.EstadoGeneral == "por_validar"),
            "completos" => proyectadas.Where(p => p.EstadoGeneral == "completo"),
            _ => proyectadas.AsEnumerable()
        };

        var ordenadas = filtradas.OrderByDescending(p => p.FechaRecepcion).ToList();
        var total = ordenadas.Count;
        var items = ordenadas.Skip((pagina - 1) * porPagina).Take(porPagina).ToList();

        return new ListarMuestrasResultado(total, pagina, porPagina, items);
    }
}
