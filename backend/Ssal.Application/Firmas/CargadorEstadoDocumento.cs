namespace Ssal.Application.Firmas;

public class CargadorEstadoDocumento(IFirmaRepository repo)
{
    private static readonly string[] Tipos = ["informe", "presupuesto"];

    public async Task<(DocInfoDto Doc, List<Ssal.Domain.FirmaDocumento> Eventos, Ssal.Domain.CicloFirma.Resultado Ciclo)?> CargarAsync(string tipo, int id)
    {
        if (!Tipos.Contains(tipo)) return null;

        DocInfoDto? doc = tipo == "informe" ? await CargarInformeAsync(id) : await CargarPresupuestoAsync(id);
        if (doc is null) return null;

        var eventos = await repo.ObtenerEventosAsync(tipo, id);

        return (doc, eventos, Ssal.Domain.CicloFirma.Calcular(eventos, doc.Areas));
    }

    private async Task<DocInfoDto?> CargarInformeAsync(int id)
    {
        var informe = await repo.ObtenerInformeAsync(id);
        return informe is null ? null : MapeoDocumentos.MapInforme(informe);
    }

    private async Task<DocInfoDto?> CargarPresupuestoAsync(int id)
    {
        var p = await repo.ObtenerPresupuestoAsync(id);
        return p is null ? null : MapeoDocumentos.MapPresupuesto(p);
    }
}
