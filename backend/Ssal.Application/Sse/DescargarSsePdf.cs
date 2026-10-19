namespace Ssal.Application.Sse;

public record SsePdfDto(byte[] Contenido, string NombreArchivo);

public class DescargarSsePdf(ISseRepository repo, ISsePdfGenerator generador)
{
    public async Task<SsePdfDto?> EjecutarAsync(int id)
    {
        var s = await repo.ObtenerCompletoParaPdfAsync(id);
        if (s is null) return null;

        var pdfBytes = generador.Generar(s);
        return new SsePdfDto(pdfBytes, $"{s.Codigo ?? "sse"}.pdf");
    }
}
