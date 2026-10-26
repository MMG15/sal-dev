namespace Ssal.Application.DocumentosInstitucionales;

public interface IDocumentoInstitucionalRepository
{
    Task<List<Ssal.Domain.DocumentoInstitucional>> ListarAsync();

    Task<Ssal.Domain.DocumentoInstitucional?> ObtenerAsync(int id);

    void Agregar(Ssal.Domain.DocumentoInstitucional documento);

    void Eliminar(Ssal.Domain.DocumentoInstitucional documento);

    Task<string> GuardarEnDiscoAsync(string extension, Stream contenido, CancellationToken ct);

    Task<byte[]?> LeerDeDiscoAsync(string nombreAlmacenado);

    void EliminarDelDisco(string nombreAlmacenado);

    Task GuardarCambiosAsync();
}
