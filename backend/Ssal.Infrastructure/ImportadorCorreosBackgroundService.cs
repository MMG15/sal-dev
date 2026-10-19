using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ssal.Infrastructure.Data;

namespace Ssal.Infrastructure;

// Corre en segundo plano junto con el backend. Cada minuto revisa qué cuentas de correo
// (tabla cuentas_correo, administrada por ROL-01/ROL-06 desde Usuarios) están habilitadas
// y les toca revisión según su propio intervalo, y las importa. Si no hay ninguna cuenta
// cargada todavía, simplemente no hace nada.
public class ImportadorCorreosBackgroundService(
    IServiceScopeFactory scopeFactory,
    IConfiguration config,
    ILogger<ImportadorCorreosBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan IntervaloDeChequeo = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var claveCifrado = config["MailImport:ClaveCifrado"];

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(claveCifrado))
                {
                    logger.LogWarning("MailImport:ClaveCifrado no está configurada; no se revisa ningún correo.");
                }
                else
                {
                    await RevisarCuentasPendientesAsync(claveCifrado, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // El backend se está apagando: no es un error.
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al revisar las cuentas de correo.");
            }

            try
            {
                await Task.Delay(IntervaloDeChequeo, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Idem: apagado normal.
            }
        }
    }

    private async Task RevisarCuentasPendientesAsync(string claveCifrado, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SsalDbContext>();

        var ahora = DateTime.UtcNow;
        var cuentas = await db.CuentasCorreo.Where(c => c.Habilitada).ToListAsync(ct);

        foreach (var cuenta in cuentas)
        {
            var corresponde = cuenta.UltimaRevision is null
                || (ahora - cuenta.UltimaRevision.Value).TotalMinutes >= Math.Max(1, cuenta.IntervaloMinutos);
            if (!corresponde) continue;

            await RevisarUnaCuentaAsync(scope.ServiceProvider, cuenta.IdCuenta, claveCifrado, ct);
        }
    }

    // También la usa el endpoint "Revisar ahora" del controlador, para no duplicar la lógica.
    public static async Task<(int importados, string? error)> RevisarUnaCuentaAsync(
        IServiceProvider servicios, int idCuenta, string claveCifrado, CancellationToken ct)
    {
        var db = servicios.GetRequiredService<SsalDbContext>();
        var logger = servicios.GetRequiredService<ILogger<ImportadorCorreosBackgroundService>>();
        var cuenta = await db.CuentasCorreo.FindAsync([idCuenta], ct);
        if (cuenta is null) return (0, "La cuenta no existe.");

        try
        {
            var passwordPlano = Cifrado.Desencriptar(cuenta.PasswordCifrada, claveCifrado);
            var importador = servicios.GetRequiredService<ImportadorCorreos>();
            var nuevos = await importador.ImportarAsync(cuenta, passwordPlano, ct);

            cuenta.UltimaRevision = DateTime.UtcNow;
            cuenta.UltimoError = null;
            await db.SaveChangesAsync(ct);
            return (nuevos, null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo revisar la cuenta de correo {Cuenta}.", cuenta.Usuario);
            cuenta.UltimaRevision = DateTime.UtcNow;
            cuenta.UltimoError = ex.Message.Length > 490 ? ex.Message[..490] : ex.Message;
            await db.SaveChangesAsync(ct);
            return (0, cuenta.UltimoError);
        }
    }
}
