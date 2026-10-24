using Ssal.Application.Comun;

namespace Ssal.Application.Sse;

public class AsignarRotulo(ISseRepository repo)
{
    const int CantidadMaxima = 20;

    public async Task<Resultado<IReadOnlyList<RotuloCreadoDto>>> EjecutarAsync(int idSse, AsignarRotuloCommand cmd, int idUsuario)
    {
        var sse = await repo.ObtenerConRotulosAsync(idSse);
        if (sse is null) return Resultado<IReadOnlyList<RotuloCreadoDto>>.Fallido("No encontrado");
        if (sse.RotulosInternos.Count > 0)
            return Resultado<IReadOnlyList<RotuloCreadoDto>>.Fallido("Esta SSE ya tiene rótulo(s) asignado(s).");

        var numeros = new List<string>();

        if (!string.IsNullOrWhiteSpace(cmd.NumeroUnico))
        {
            numeros.Add(cmd.NumeroUnico.Trim());
        }
        else
        {
            var cantidad = cmd.Cantidad is > 0 ? cmd.Cantidad.Value : 1;
            if (cantidad > CantidadMaxima)
                return Resultado<IReadOnlyList<RotuloCreadoDto>>.Fallido($"No se pueden generar más de {CantidadMaxima} muestras repetidas a la vez.");

            var baseNumero = await GenerarNumeroBaseAsync();
            // Si hay una sola muestra, el número queda sin sufijo (igual que siempre). Si hay
            // varias, cada repetición del mismo pedido comparte el número base y se diferencia
            // con una letra: 26-296-001-A, -B, -C...
            for (var i = 0; i < cantidad; i++)
                numeros.Add(cantidad == 1 ? baseNumero : $"{baseNumero}-{(char)('A' + i)}");
        }

        foreach (var numero in numeros)
        {
            if (await repo.ExisteNumeroRotuloAsync(numero))
                return Resultado<IReadOnlyList<RotuloCreadoDto>>.Fallido($"El número de rótulo '{numero}' ya existe.");
        }

        var rotulos = numeros.Select(numero => new Ssal.Domain.RotuloInterno
        {
            IdSse = idSse,
            NumeroUnico = numero,
            Descripcion = cmd.Descripcion,
            Estado = "activo",
            FechaAsignacion = DateTime.UtcNow,
            AsignadoPor = idUsuario
        }).ToList();

        foreach (var rotulo in rotulos) repo.AgregarRotulo(rotulo);
        await repo.GuardarCambiosAsync();

        return Resultado<IReadOnlyList<RotuloCreadoDto>>.Exitoso(
            rotulos.Select(r => new RotuloCreadoDto(r.IdRotulo, r.NumeroUnico)).ToList());
    }

    // Numeración por día juliano: AA-DDD-NNN (año de 2 dígitos, día del año 001-366 —
    // DateTime.DayOfYear ya contempla bisiestos solo—, y un correlativo que arranca de nuevo
    // cada día). Se usa la hora local porque lo que importa es qué día es en el laboratorio,
    // no en UTC.
    private async Task<string> GenerarNumeroBaseAsync()
    {
        var hoy = DateTime.Now;
        var prefijo = $"{hoy:yy}-{hoy.DayOfYear:000}-";
        var ultimo = await repo.ObtenerUltimoNumeroRotuloAsync(prefijo);
        // El último rótulo puede tener sufijo de letra (de un pedido con varias muestras
        // repetidas); hay que recortarlo a sus primeros 3 segmentos antes de pedir el
        // siguiente correlativo, porque GeneradorCodigoCorrelativo solo entiende ese formato.
        var ultimoBase = ultimo is null ? null : string.Join("-", ultimo.Split('-').Take(3));
        return GeneradorCodigoCorrelativo.Siguiente(prefijo, ultimoBase, digitos: 3);
    }
}
