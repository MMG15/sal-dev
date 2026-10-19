namespace Ssal.Application.Resultados;

public record InformePdfDto(byte[] Contenido, string NombreArchivo);

public class DescargarInformePdf(IResultadoRepository repo, IInformePdfGenerator generador)
{
    public async Task<InformePdfDto?> EjecutarAsync(int id)
    {
        var informe = await repo.ObtenerInformeCompletoAsync(id);
        if (informe is null) return null;

        // Estado de firma del informe: el PDF muestra las firmas del ciclo vigente y marca borrador si no está aprobado
        var eventos = await repo.ObtenerEventosFirmaAsync("informe", id);
        var areas = informe.Muestra.Resultados.Select(r => r.Analisis.Area).Distinct().ToList();
        var estado = Ssal.Domain.CicloFirma.Calcular(eventos, areas).Estado;

        var pdfBytes = generador.Generar(informe, estado, Ssal.Domain.CicloFirma.EventosCicloActual(eventos));
        return new InformePdfDto(pdfBytes, $"{informe.Codigo}.pdf");
    }
}
