namespace Ssal.Application.Facturas;

public class RegistrarFactura(IFacturaRepository repo)
{
    public async Task<Resultado<(int IdFactura, string Numero)>> EjecutarAsync(RegistrarFacturaCommand cmd, int idUsuario)
    {
        var numero = cmd.Numero?.Trim();
        if (string.IsNullOrWhiteSpace(numero))
            return Resultado<(int, string)>.Fallido("El número de factura es obligatorio.");
        if (numero.Length > 30)
            return Resultado<(int, string)>.Fallido("El número de factura no puede superar los 30 caracteres.");

        var sse = await repo.ObtenerSseAsync(cmd.IdSse);
        if (sse is null) return Resultado<(int, string)>.Fallido("SSE no encontrada.");

        if (await repo.SseTieneFacturaActivaAsync(sse.IdSse))
            return Resultado<(int, string)>.Fallido($"La SSE {sse.Codigo} ya tiene una factura registrada.");

        if (await repo.ExisteNumeroAsync(numero))
            return Resultado<(int, string)>.Fallido($"Ya existe una factura con el número {numero}.");

        var factura = new Ssal.Domain.Factura
        {
            IdSse = sse.IdSse,
            CreadoPor = idUsuario,
            Numero = numero,
            ImportePesos = sse.ImportePesos,
            ImporteDolares = sse.ImporteDolares,
            Estado = "emitida",
            Fecha = cmd.Fecha?.ToUniversalTime() ?? DateTime.UtcNow
        };
        repo.Agregar(factura);
        await repo.GuardarCambiosAsync();

        return Resultado<(int, string)>.Exitoso((factura.IdFactura, factura.Numero));
    }
}
