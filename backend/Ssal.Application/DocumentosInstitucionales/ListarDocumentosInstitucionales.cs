namespace Ssal.Application.DocumentosInstitucionales;

public class ListarDocumentosInstitucionales(IDocumentoInstitucionalRepository repo)
{
    public async Task<List<DocumentoInstitucionalDto>> EjecutarAsync()
    {
        var documentos = await repo.ListarAsync();
        return documentos
            .OrderBy(d => d.Nombre)
            .Select(d => new DocumentoInstitucionalDto(
                d.Id, d.Nombre, d.NombreOriginal, d.TipoMime, d.TamañoBytes, d.SubidoEn,
                $"{d.SubidoPorUsuario.Nombre} {d.SubidoPorUsuario.Apellido}"))
            .ToList();
    }
}
