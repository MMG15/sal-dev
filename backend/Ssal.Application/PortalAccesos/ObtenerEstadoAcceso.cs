namespace Ssal.Application.PortalAccesos;

public class ObtenerEstadoAcceso(IPortalAccesoRepository repo)
{
    public async Task<EstadoAccesoDto> EjecutarAsync(int idCliente)
    {
        var acceso = await repo.ObtenerPorClienteAsync(idCliente);
        if (acceso is null) return EstadoAccesoDto.NoExiste();

        return new EstadoAccesoDto(true, acceso.Email, acceso.Activo, acceso.ForzarCambioPwd, acceso.UltimoAcceso);
    }
}
