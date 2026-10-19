namespace Ssal.Application.Firmas;

public class ListarFirmas(IFirmaRepository repo)
{
    public async Task<List<ListarFirmasItemDto>> EjecutarAsync(string? tipo, string? estado, string? area)
    {
        var docs = new List<DocInfoDto>();

        if (string.IsNullOrWhiteSpace(tipo) || tipo == "informe")
        {
            var informes = await repo.ListarInformesAsync();
            docs.AddRange(informes.Select(MapeoDocumentos.MapInforme));
        }

        if (string.IsNullOrWhiteSpace(tipo) || tipo == "presupuesto")
        {
            var presupuestos = await repo.ListarPresupuestosAsync();
            docs.AddRange(presupuestos.Select(MapeoDocumentos.MapPresupuesto));
        }

        var firmas = await repo.ListarTodosLosEventosAsync();

        return docs.Select(d =>
            {
                var eventos = firmas.Where(f => f.TipoDocumento == d.Tipo && f.IdDocumento == d.Id);
                var ciclo = Ssal.Domain.CicloFirma.Calcular(eventos, d.Areas);
                return new ListarFirmasItemDto(
                    d.Tipo, d.Id, d.Codigo, d.Destinatario, d.Sse, d.Rotulo, d.Fecha, d.Areas,
                    ciclo.Estado, ciclo.AreasFirmadas.OrderBy(a => a).ToList());
            })
            .Where(x => string.IsNullOrWhiteSpace(estado) || x.Estado == estado)
            .Where(x => string.IsNullOrWhiteSpace(area) || x.Areas.Contains(area))
            .OrderByDescending(x => x.Fecha)
            .ToList();
    }
}
