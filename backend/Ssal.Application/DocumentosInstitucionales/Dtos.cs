namespace Ssal.Application.DocumentosInstitucionales;

public record DocumentoInstitucionalDto(
    int Id, string Nombre, string NombreOriginal, string TipoMime, long TamañoBytes,
    DateTime SubidoEn, string SubidoPor);

public record DocumentoSubidoDto(int Id, string Nombre);

public record DocumentoContenidoDto(byte[] Contenido, string TipoMime, string NombreOriginal);
