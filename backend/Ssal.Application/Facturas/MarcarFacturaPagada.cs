namespace Ssal.Application.Facturas;

public class MarcarFacturaPagada(IFacturaRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, MarcarPagadaCommand cmd)
    {
        var factura = await repo.ObtenerAsync(id);
        if (factura is null) return Resultado.Fallido("No encontrada");
        if (factura.Estado != "emitida")
            return Resultado.Fallido("Solo se pueden marcar como pagadas las facturas emitidas.");

        factura.Estado = "pagada";
        factura.FechaPago = cmd.FechaPago?.ToUniversalTime() ?? DateTime.UtcNow;
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
