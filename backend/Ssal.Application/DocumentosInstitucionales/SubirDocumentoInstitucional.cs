namespace Ssal.Application.DocumentosInstitucionales;

public class SubirDocumentoInstitucional(IDocumentoInstitucionalRepository repo)
{
    private static readonly string[] ExtensionesPermitidas = [".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg"];

    public async Task<Resultado<DocumentoSubidoDto>> EjecutarAsync(
        string nombre, string nombreOriginal, string tipoMime, long longitud, Stream contenido, int idUsuario, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            return Resultado<DocumentoSubidoDto>.Fallido("El nombre del documento es obligatorio.");
        if (longitud == 0)
            return Resultado<DocumentoSubidoDto>.Fallido("No se recibió ningún archivo.");

        var ext = Path.GetExtension(nombreOriginal).ToLowerInvariant();
        if (!ExtensionesPermitidas.Contains(ext))
            return Resultado<DocumentoSubidoDto>.Fallido($"Tipo de archivo no permitido ({ext}).");

        var nombreAlmacenado = await repo.GuardarEnDiscoAsync(ext, contenido, ct);

        var documento = new Ssal.Domain.DocumentoInstitucional
        {
            Nombre = nombre.Trim(),
            NombreOriginal = nombreOriginal,
            NombreAlmacenado = nombreAlmacenado,
            TipoMime = tipoMime,
            TamañoBytes = longitud,
            SubidoPor = idUsuario,
            SubidoEn = DateTime.UtcNow
        };
        repo.Agregar(documento);
        await repo.GuardarCambiosAsync();

        return Resultado<DocumentoSubidoDto>.Exitoso(new DocumentoSubidoDto(documento.Id, documento.Nombre));
    }
}
