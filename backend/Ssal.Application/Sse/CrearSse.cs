using Ssal.Application.Comun;

namespace Ssal.Application.Sse;

public class CrearSse(ISseRepository repo)
{
    public async Task<CrearSseResultadoDto> EjecutarAsync(CrearSseCommand cmd, int idUsuario)
    {
        var hoy = DateTime.UtcNow;
        var prefijo = $"SSE-{hoy:yyyyMMdd}-";
        var ultimo = await repo.ObtenerUltimoCodigoAsync(prefijo);
        var codigo = GeneradorCodigoCorrelativo.Siguiente(prefijo, ultimo);

        string? codigoRpo = cmd.IdPresupuesto.HasValue
            ? await repo.ObtenerCodigoRpoAsync(cmd.IdPresupuesto.Value)
            : null;

        var sse = new Ssal.Domain.Sse
        {
            Codigo = codigo,
            CodigoRpo = codigoRpo,
            IdPresupuesto = cmd.IdPresupuesto,
            IdCliente = cmd.IdCliente,
            CreadoPor = idUsuario,
            Area = cmd.Area,
            FormaPago = cmd.FormaPago,
            ImporteDolares = cmd.ImporteDolares,
            ImportePesos = cmd.ImportePesos,
            Estado = "activa",
            Fecha = hoy
        };

        repo.Agregar(sse);
        await repo.GuardarCambiosAsync();

        return new CrearSseResultadoDto(sse.IdSse, sse.Codigo);
    }
}
