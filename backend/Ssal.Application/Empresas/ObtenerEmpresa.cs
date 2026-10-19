namespace Ssal.Application.Empresas;

// Nota: a diferencia de ListarEmpresas, acá se devuelve la entidad de dominio tal cual
// (con sus Clientes), igual que hacía el controller original. Se preserva así a propósito
// en esta primera pasada (comportamiento idéntico); se puede resolver con un DTO de detalle
// más adelante sin que eso sea parte de esta migración mecánica.
public class ObtenerEmpresa(IEmpresaRepository repo)
{
    public Task<Ssal.Domain.Empresa?> EjecutarAsync(int id) => repo.ObtenerConClientesAsync(id);
}
