namespace Ssal.Application.DocumentosInstitucionales;

public class DescargarDocumentoInstitucional(IDocumentoInstitucionalRepository repo)
{
    public async Task<Resultado<DocumentoContenidoDto>> EjecutarAsync(int id)
    {
        var documento = await repo.ObtenerAsync(id);
        if (documento is null) return Resultado<DocumentoContenidoDto>.Fallido("No encontrado");

        var bytes = await repo.LeerDeDiscoAsync(documento.NombreAlmacenado);
        if (bytes is null) return Resultado<DocumentoContenidoDto>.Fallido("Archivo no encontrado en disco.");

        return Resultado<DocumentoContenidoDto>.Exitoso(new DocumentoContenidoDto(bytes, documento.TipoMime, documento.NombreOriginal));
    }
}
