using Ssal.Application.DocumentosInstitucionales;

namespace Ssal.Application.Presupuestos;

// Copia un documento de la biblioteca institucional (certificación ISO 9001, procedimientos,
// disposiciones) como adjunto de este presupuesto, reutilizando la misma lógica de "subir
// archivo" — así no hace falta bajarlo y volver a subirlo a mano cada vez.
public class AdjuntarDocumentoInstitucional(
    IDocumentoInstitucionalRepository documentosRepo, SubirArchivoPresupuesto subirArchivo)
{
    public async Task<Resultado<ArchivoSubidoDto>> EjecutarAsync(int idPresupuesto, int idDocumento, int idUsuario, CancellationToken ct)
    {
        var documento = await documentosRepo.ObtenerAsync(idDocumento);
        if (documento is null) return Resultado<ArchivoSubidoDto>.Fallido("El documento de la biblioteca no existe.");

        var bytes = await documentosRepo.LeerDeDiscoAsync(documento.NombreAlmacenado);
        if (bytes is null) return Resultado<ArchivoSubidoDto>.Fallido("No se encontró el archivo en disco.");

        using var stream = new MemoryStream(bytes);
        return await subirArchivo.EjecutarAsync(
            idPresupuesto, documento.NombreOriginal, documento.TipoMime, bytes.Length, stream, idUsuario, ct);
    }
}
