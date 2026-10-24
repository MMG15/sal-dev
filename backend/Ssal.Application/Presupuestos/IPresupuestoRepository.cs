namespace Ssal.Application.Presupuestos;

public interface IPresupuestoRepository
{
    Task<(int Total, List<PresupuestoResumenDto> Items)> BuscarAsync(string? busqueda, string? estado, int pagina, int porPagina);

    Task<Ssal.Domain.Presupuesto?> ObtenerDetalleAsync(int id);

    // Con todos los includes que necesita el PDF (y por lo tanto también el envío por mail).
    Task<Ssal.Domain.Presupuesto?> ObtenerCompletoAsync(int id);

    Task<Ssal.Domain.Presupuesto?> ObtenerAsync(int id);

    Task<Ssal.Domain.Presupuesto?> ObtenerConConsultaAsync(int id);

    Task<Ssal.Domain.Consulta?> ObtenerConsultaAsync(int idConsulta);

    Task<Ssal.Domain.Presupuesto?> ObtenerExistentePorConsultaAsync(int idConsulta);

    // Reemplaza los análisis y paquetes de un presupuesto existente (se usa al editarlo: se actualiza
    // en el lugar, nunca se borra y se recrea, porque una SSE ya podría estar apuntando a este Id).
    void ReemplazarItemsYPaquetes(
        Ssal.Domain.Presupuesto existente,
        List<Ssal.Domain.PresupuestoAnalisis> nuevosItems,
        List<Ssal.Domain.PresupuestoPaquete> nuevosPaquetes);

    Task<Dictionary<int, Ssal.Domain.Analisis>> ObtenerAnalisisPorIdsAsync(List<int> ids);

    Task<Dictionary<int, Ssal.Domain.Paquete>> ObtenerPaquetesPorIdsAsync(List<int> ids);

    // Para el bloque de firmas digitales en el PDF.
    Task<List<Ssal.Domain.FirmaDocumento>> ObtenerEventosFirmaAsync(int idPresupuesto);

    Task<string?> ObtenerUltimoCodigoAsync(string prefijo);

    void Agregar(Ssal.Domain.Presupuesto presupuesto);

    // Para decidir desde qué casilla responder: la que recibió la consulta original, si la hay.
    Task<int?> ObtenerIdCuentaOrigenAsync(int idConsulta);

    Task<Ssal.Domain.CuentaCorreo?> ObtenerCuentaAsync(int idCuenta);

    Task<Ssal.Domain.CuentaCorreo?> ObtenerPrimeraCuentaHabilitadaAsync();

    Task<List<Ssal.Domain.PresupuestoArchivo>> ListarArchivosAsync(int idPresupuesto);

    void AgregarArchivo(Ssal.Domain.PresupuestoArchivo archivo);

    Task<Ssal.Domain.PresupuestoArchivo?> ObtenerArchivoAsync(int id, int idPresupuesto);

    void EliminarArchivoRegistro(Ssal.Domain.PresupuestoArchivo archivo);

    // Almacenamiento en disco: el repositorio decide dónde y cómo guardar, el caso de uso
    // solo sabe que existe un lugar donde guardar/leer/borrar el contenido de un archivo.
    Task<string> GuardarArchivoEnDiscoAsync(int idPresupuesto, string extension, Stream contenido, CancellationToken ct);

    Task<byte[]?> LeerArchivoDeDiscoAsync(int idPresupuesto, string nombreAlmacenado);

    void EliminarArchivoDelDisco(int idPresupuesto, string nombreAlmacenado);

    Task GuardarCambiosAsync();
}

// La implementación real (QuestPDF) vive en Ssal.Infrastructure.
public interface IPresupuestoPdfGenerator
{
    byte[] Generar(Ssal.Domain.Presupuesto presupuesto, IReadOnlyList<Ssal.Domain.FirmaDocumento> firmasCicloActual);
}
