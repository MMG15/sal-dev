using Ssal.Application.Comun;

namespace Ssal.Application.Resultados;

// Guarda (finaliza) el informe de resultados de una muestra. Requiere que
// todos los análisis estén validados. Genera el código INF-AAAAMMDD-NNN.
public class GuardarInforme(IResultadoRepository repo)
{
    public async Task<Resultado<InformeCreadoDto>> EjecutarAsync(int idMuestra, int idUsuario)
    {
        var m = await repo.ObtenerParaInformeAsync(idMuestra);
        if (m is null) return Resultado<InformeCreadoDto>.Fallido("No encontrado");
        if (m.Informe is not null)
            return Resultado<InformeCreadoDto>.Fallido("Esta muestra ya tiene un informe guardado.");
        if (m.Resultados.Count == 0 || !m.Resultados.All(r => r.Estado == "validado"))
            return Resultado<InformeCreadoDto>.Fallido("Todos los análisis deben estar validados antes de guardar el informe.");

        var hoy = DateTime.UtcNow;
        var prefijo = $"INF-{hoy:yyyyMMdd}-";
        var ultimo = await repo.ObtenerUltimoCodigoInformeAsync(prefijo);
        var codigo = GeneradorCodigoCorrelativo.Siguiente(prefijo, ultimo);

        var informe = new Ssal.Domain.Informe
        {
            IdMuestra = idMuestra,
            GeneradoPor = idUsuario,
            Codigo = codigo,
            FechaGeneracion = hoy
        };

        repo.AgregarInforme(informe);
        m.Estado = "completada";

        await repo.GuardarCambiosAsync();
        return Resultado<InformeCreadoDto>.Exitoso(new InformeCreadoDto(informe.IdInforme, informe.Codigo));
    }
}
