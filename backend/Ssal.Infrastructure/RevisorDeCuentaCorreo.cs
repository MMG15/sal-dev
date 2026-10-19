using Microsoft.Extensions.Configuration;
using Ssal.Application.CuentasCorreo;

namespace Ssal.Infrastructure;

public class RevisorDeCuentaCorreo(IServiceProvider servicios, IConfiguration config) : IRevisorDeCuentaCorreo
{
    private string ClaveCifrado => config["MailImport:ClaveCifrado"]
        ?? throw new InvalidOperationException("MailImport:ClaveCifrado no está configurada.");

    public Task<(int Importados, string? Error)> RevisarAsync(int idCuenta, CancellationToken ct) =>
        ImportadorCorreosBackgroundService.RevisarUnaCuentaAsync(servicios, idCuenta, ClaveCifrado, ct);
}
