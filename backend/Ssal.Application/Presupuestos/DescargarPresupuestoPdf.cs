namespace Ssal.Application.Presupuestos;

public record PresupuestoPdfDto(byte[] Contenido, string NombreArchivo);

public class DescargarPresupuestoPdf(IPresupuestoRepository repo, IPresupuestoPdfGenerator generador)
{
    public async Task<PresupuestoPdfDto?> EjecutarAsync(int id)
    {
        var p = await repo.ObtenerCompletoAsync(id);
        if (p is null) return null;

        var pdfBytes = generador.Generar(p);
        return new PresupuestoPdfDto(pdfBytes, $"{p.CodigoRpo ?? "presupuesto"}.pdf");
    }
}
