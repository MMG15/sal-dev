namespace Ssal.Application.Resultados;

public interface IResultadoRepository
{
    // Trae las muestras candidatas (con área filtrada en SQL si se pidió); el resto del
    // filtrado/orden/paginado depende de un estado calculado y se hace en el caso de uso,
    // igual que hacía el controller original.
    Task<List<Ssal.Domain.Muestra>> BuscarMuestrasAsync(string? busqueda, string? area);

    Task<Ssal.Domain.Muestra?> ObtenerDetalleAsync(int id);

    Task<Ssal.Domain.Muestra?> ObtenerParaInformeAsync(int id);

    Task<string?> ObtenerUltimoCodigoInformeAsync(string prefijo);

    void AgregarInforme(Ssal.Domain.Informe informe);

    Task<Ssal.Domain.Informe?> ObtenerInformeCompletoAsync(int id);

    Task<List<Ssal.Domain.FirmaDocumento>> ObtenerEventosFirmaAsync(string tipoDocumento, int idDocumento);

    Task<Ssal.Domain.Resultado?> ObtenerConAnalisisAsync(int id);

    Task<Ssal.Domain.Resultado?> ObtenerAsync(int id);

    Task GuardarCambiosAsync();
}

// La implementación real (QuestPDF) vive en Ssal.Infrastructure.
public interface IInformePdfGenerator
{
    byte[] Generar(Ssal.Domain.Informe informe, string estadoFirma, IReadOnlyList<Ssal.Domain.FirmaDocumento> firmas);
}
