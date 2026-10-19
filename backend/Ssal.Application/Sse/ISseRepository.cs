namespace Ssal.Application.Sse;

public interface ISseRepository
{
    Task<(int Total, List<SseResumenDto> Items)> BuscarAsync(string? busqueda, string? estado, string? area, int pagina, int porPagina);

    Task<Ssal.Domain.Sse?> ObtenerDetalleAsync(int id);

    Task<Ssal.Domain.Sse?> ObtenerCompletoParaPdfAsync(int id);

    Task<Ssal.Domain.Sse?> ObtenerAsync(int id);

    Task<Ssal.Domain.Sse?> ObtenerConRotuloAsync(int id);

    Task<bool> ExisteClienteAsync(int id);

    Task<bool> ExisteEmpresaAsync(int id);

    Task<string?> ObtenerCodigoRpoAsync(int idPresupuesto);

    Task<string?> ObtenerUltimoCodigoAsync(string prefijo);

    void Agregar(Ssal.Domain.Sse sse);

    Task<bool> ExisteNumeroRotuloAsync(string numeroUnico);

    Task<string?> ObtenerUltimoNumeroRotuloAsync(string prefijo);

    void AgregarRotulo(Ssal.Domain.RotuloInterno rotulo);

    Task<Ssal.Domain.RotuloInterno?> ObtenerRotuloConMuestraAsync(int idSse);

    void AgregarMuestra(Ssal.Domain.Muestra muestra);

    Task<Ssal.Domain.Sse?> ObtenerConPresupuestoItemsAsync(int id);

    void AgregarResultado(Ssal.Domain.Resultado resultado);

    Task<Ssal.Domain.RotuloInterno?> ObtenerRotuloPorSseAsync(int idSse);

    Task<bool> ExisteOtroRotuloConNumeroAsync(int idRotulo, string numeroUnico);

    void AgregarAuditoriasRotulo(IEnumerable<Ssal.Domain.AuditoriaRotulo> cambios);

    Task GuardarCambiosAsync();
}

// La implementación real (QuestPDF) vive en Ssal.Infrastructure.
public interface ISsePdfGenerator
{
    byte[] Generar(Ssal.Domain.Sse sse);
}
