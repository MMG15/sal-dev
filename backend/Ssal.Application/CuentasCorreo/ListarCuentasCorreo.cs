namespace Ssal.Application.CuentasCorreo;

public class ListarCuentasCorreo(ICuentaCorreoRepository repo)
{
    public Task<List<CuentaCorreoDto>> EjecutarAsync() => repo.ListarAsync();
}
