namespace Ssal.Application.Firmas;

public interface IFirmaRepository
{
    Task<List<Ssal.Domain.Informe>> ListarInformesAsync();

    Task<List<Ssal.Domain.Presupuesto>> ListarPresupuestosAsync();

    Task<List<Ssal.Domain.FirmaDocumento>> ListarTodosLosEventosAsync();

    Task<Ssal.Domain.Informe?> ObtenerInformeAsync(int id);

    Task<Ssal.Domain.Presupuesto?> ObtenerPresupuestoAsync(int id);

    // Con todos los includes que necesita el PDF (y por lo tanto también el envío por mail).
    Task<Ssal.Domain.Informe?> ObtenerInformeCompletoAsync(int id);

    Task<List<Ssal.Domain.FirmaDocumento>> ObtenerEventosAsync(string tipoDocumento, int idDocumento);

    void AgregarFirma(Ssal.Domain.FirmaDocumento firma);

    Task<Ssal.Domain.Usuario?> ObtenerUsuarioAsync(int id);

    Task<Ssal.Domain.CuentaCorreo?> ObtenerPrimeraCuentaHabilitadaAsync();

    Task GuardarCambiosAsync();
}
