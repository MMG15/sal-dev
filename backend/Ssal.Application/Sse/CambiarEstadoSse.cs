namespace Ssal.Application.Sse;

public class CambiarEstadoSse(ISseRepository repo)
{
    public async Task<bool> EjecutarAsync(int id, CambiarEstadoSseCommand cmd)
    {
        var s = await repo.ObtenerAsync(id);
        if (s is null) return false;

        s.Estado = cmd.Estado;
        await repo.GuardarCambiosAsync();
        return true;
    }
}
