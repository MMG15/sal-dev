namespace Ssal.Application.Resultados;

// Cargar o corregir el valor de un resultado (Analista / Resp. Área).
public class CargarValorResultado(IResultadoRepository repo)
{
    public async Task<Resultado<ValorCargadoDto>> EjecutarAsync(int id, CargarValorCommand cmd, int idUsuario)
    {
        var r = await repo.ObtenerConAnalisisAsync(id);
        if (r is null) return Resultado<ValorCargadoDto>.Fallido("No encontrado");

        if (string.IsNullOrWhiteSpace(cmd.Valor))
            return Resultado<ValorCargadoDto>.Fallido("El valor no puede estar vacío.");

        r.Valor = cmd.Valor.Trim();
        r.Estado = "cargado";
        r.CargadoPor = idUsuario;
        r.FechaCarga = DateTime.UtcNow;
        // Al recargar un resultado (ej. tras rechazo), se limpia la validación anterior.
        r.ValidadoPor = null;
        r.FechaValidacion = null;
        r.MotivoRechazo = null;

        await repo.GuardarCambiosAsync();

        return Resultado<ValorCargadoDto>.Exitoso(new ValorCargadoDto(r.IdResultado, r.Estado, Ssal.Domain.EvaluadorDeRango.FueraDeRango(r.Valor, r.Analisis)));
    }
}
