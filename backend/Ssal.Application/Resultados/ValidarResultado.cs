namespace Ssal.Application.Resultados;

// Responsable de Área valida o rechaza un resultado ya cargado.
public class ValidarResultado(IResultadoRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, ValidarResultadoCommand cmd, int idUsuario)
    {
        var r = await repo.ObtenerAsync(id);
        if (r is null) return Resultado.Fallido("No encontrado");
        if (r.Estado != "cargado")
            return Resultado.Fallido("Solo se pueden validar resultados que ya fueron cargados.");

        if (cmd.Aprobado)
        {
            r.Estado = "validado";
            r.MotivoRechazo = null;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(cmd.MotivoRechazo))
                return Resultado.Fallido("Indicá el motivo del rechazo.");
            r.Estado = "rechazado";
            r.MotivoRechazo = cmd.MotivoRechazo.Trim();
        }
        r.ValidadoPor = idUsuario;
        r.FechaValidacion = DateTime.UtcNow;

        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
