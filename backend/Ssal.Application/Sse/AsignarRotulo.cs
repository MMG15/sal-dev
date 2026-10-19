using Ssal.Application.Comun;

namespace Ssal.Application.Sse;

public class AsignarRotulo(ISseRepository repo)
{
    public async Task<Resultado<RotuloCreadoDto>> EjecutarAsync(int idSse, AsignarRotuloCommand cmd, int idUsuario)
    {
        var sse = await repo.ObtenerConRotuloAsync(idSse);
        if (sse is null) return Resultado<RotuloCreadoDto>.Fallido("No encontrado");
        if (sse.RotuloInterno is not null)
            return Resultado<RotuloCreadoDto>.Fallido("Esta SSE ya tiene un rótulo asignado.");

        var numeroUnico = cmd.NumeroUnico?.Trim() ?? await GenerarNumeroUnicoAsync();

        if (await repo.ExisteNumeroRotuloAsync(numeroUnico))
            return Resultado<RotuloCreadoDto>.Fallido($"El número de rótulo '{numeroUnico}' ya existe.");

        var rotulo = new Ssal.Domain.RotuloInterno
        {
            IdSse = idSse,
            NumeroUnico = numeroUnico,
            Descripcion = cmd.Descripcion,
            Estado = "activo",
            FechaAsignacion = DateTime.UtcNow,
            AsignadoPor = idUsuario
        };

        repo.AgregarRotulo(rotulo);
        await repo.GuardarCambiosAsync();

        return Resultado<RotuloCreadoDto>.Exitoso(new RotuloCreadoDto(rotulo.IdRotulo, rotulo.NumeroUnico));
    }

    private async Task<string> GenerarNumeroUnicoAsync()
    {
        var prefijo = $"ROT-{DateTime.UtcNow:yyyyMM}-";
        var ultimo = await repo.ObtenerUltimoNumeroRotuloAsync(prefijo);
        return GeneradorCodigoCorrelativo.Siguiente(prefijo, ultimo, digitos: 4);
    }
}
