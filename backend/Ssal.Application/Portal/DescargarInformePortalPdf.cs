using Ssal.Application.Resultados;

namespace Ssal.Application.Portal;

public record InformePortalPdfDto(byte[] Contenido, string NombreArchivo);

public class DescargarInformePortalPdf(IPortalRepository repo, IInformePdfGenerator generador)
{
    // (null, false) = no existe o no es del cliente (404). (null, true) = existe pero no aprobado (403).
    public async Task<(InformePortalPdfDto? Pdf, bool PerteneceAlCliente)> EjecutarAsync(int idInforme, int idCuenta)
    {
        var informe = await repo.ObtenerInformeCompletoAsync(idInforme);
        if (informe is null) return (null, false);

        var cuenta = await repo.ObtenerCuentaAsync(idCuenta);
        if (cuenta is null || !await repo.PerteneceAlClienteAsync(informe.Muestra.RotuloInterno.IdSse, cuenta.IdCliente))
            return (null, false);

        var eventos = await repo.ObtenerFirmasDeInformesAsync([idInforme]);
        var areas = informe.Muestra.Resultados.Select(r => r.Analisis.Area).Distinct().ToList();
        var ciclo = Ssal.Domain.CicloFirma.Calcular(eventos, areas);
        if (ciclo.Estado != Ssal.Domain.CicloFirma.Aprobado)
            return (null, true);

        var pdf = generador.Generar(informe, ciclo.Estado, Ssal.Domain.CicloFirma.EventosCicloActual(eventos));
        return (new InformePortalPdfDto(pdf, $"{informe.Codigo}.pdf"), true);
    }
}
