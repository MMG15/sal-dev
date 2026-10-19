namespace Ssal.Application.CuentasCorreo;

public interface ICuentaCorreoRepository
{
    Task<List<CuentaCorreoDto>> ListarAsync();

    void Agregar(Ssal.Domain.CuentaCorreo cuenta);

    Task<Ssal.Domain.CuentaCorreo?> ObtenerAsync(int id);

    Task<bool> ExisteAsync(int id);

    void Eliminar(Ssal.Domain.CuentaCorreo cuenta);

    Task GuardarCambiosAsync();
}

// Dispara una revisión inmediata de la casilla (reutiliza el mismo código que usa el
// servicio en segundo plano). La implementación real vive en Ssal.Infrastructure.
public interface IRevisorDeCuentaCorreo
{
    Task<(int Importados, string? Error)> RevisarAsync(int idCuenta, CancellationToken ct);
}
