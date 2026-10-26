namespace Ssal.Application.Presupuestos;

public class ObtenerPresupuesto(IPresupuestoRepository repo)
{
    public async Task<PresupuestoDetalleDto?> EjecutarAsync(int id)
    {
        var p = await repo.ObtenerDetalleAsync(id);
        if (p is null) return null;

        var c = p.Consulta;
        return new PresupuestoDetalleDto(
            p.IdPresupuesto, p.CodigoRpo, p.Estado, p.ImporteDolares, p.ImportePesos, p.Cotizacion,
            p.AdicionalAsesoramiento, p.TipoAsesoramiento, p.MostrarMoneda, p.Fecha,
            new ConsultaResumenDto(
                c.IdConsulta, c.Codigo, c.Canal, c.Descripcion,
                c.Cliente == null ? null : new ClienteDetalleDto(
                    c.Cliente.IdCliente, c.Cliente.Nombre, c.Cliente.Apellido, c.Cliente.Email, c.Cliente.Telefono,
                    c.Cliente.Empresa == null ? null : new EmpresaConEmailDto(c.Cliente.Empresa.IdEmpresa, c.Cliente.Empresa.RazonSocial, c.Cliente.Empresa.Email)),
                c.Empresa == null ? null : new EmpresaConEmailDto(c.Empresa.IdEmpresa, c.Empresa.RazonSocial, c.Empresa.Email)),
            new UsuarioNombreDto(p.CreadoPorUsuario.Nombre, p.CreadoPorUsuario.Apellido),
            p.Items
                .OrderBy(i => i.Analisis.Area)
                .ThenBy(i => i.Analisis.Grupo.CodigoAgrupador)
                .Select(i => new ItemPresupuestoDto(i.Id, i.IdAnalisis, i.PrecioUsdSnapshot, i.Analisis.Codigo, i.Analisis.Nombre, i.Analisis.Area, i.Analisis.Grupo.CodigoAgrupador))
                .ToList(),
            p.Paquetes
                .OrderBy(pp => pp.Paquete.Area)
                .ThenBy(pp => pp.Paquete.Nombre)
                .Select(pp => new ItemPaqueteDto(
                    pp.Id, pp.IdPaquete, pp.PrecioUsdSnapshot, pp.Paquete.Nombre, pp.Paquete.Area,
                    pp.Paquete.Items.Select(i => i.Analisis.Nombre).ToList()))
                .ToList(),
            p.Metodos
                .OrderBy(pm => pm.Metodo.Area)
                .ThenBy(pm => pm.Metodo.Nombre)
                .Select(pm => new ItemMetodoDto(pm.Id, pm.IdMetodo, pm.Metodo.Nombre, pm.Metodo.Descripcion, pm.Metodo.Area))
                .ToList());
    }
}
