namespace Ssal.Application.Consultas;

public class ObtenerConsulta(IConsultaRepository repo)
{
    public async Task<ConsultaDetalleDto?> EjecutarAsync(int id)
    {
        var c = await repo.ObtenerDetalleAsync(id);
        if (c is null) return null;

        return new ConsultaDetalleDto(
            c.IdConsulta, c.Codigo, c.Canal, c.Estado, c.Descripcion, c.Fecha, c.CreatedAt,
            c.Cliente == null ? null : new ClienteDetalleDto(
                c.Cliente.IdCliente, c.Cliente.Nombre, c.Cliente.Apellido, c.Cliente.Email, c.Cliente.Telefono,
                c.Cliente.Empresa == null ? null : new EmpresaResumenDto(c.Cliente.Empresa.IdEmpresa, c.Cliente.Empresa.RazonSocial)),
            c.Empresa == null ? null : new EmpresaResumenDto(c.Empresa.IdEmpresa, c.Empresa.RazonSocial),
            c.Responsable == null ? null : new UsuarioResumenDto(c.Responsable.IdUsuario, c.Responsable.Nombre, c.Responsable.Apellido),
            c.Presupuesto == null ? null : new PresupuestoResumenDto(
                c.Presupuesto.IdPresupuesto, c.Presupuesto.CodigoRpo, c.Presupuesto.Estado,
                c.Presupuesto.ImporteDolares, c.Presupuesto.ImportePesos, c.Presupuesto.Cotizacion,
                c.Presupuesto.AdicionalAsesoramiento, c.Presupuesto.TipoAsesoramiento, c.Presupuesto.Fecha,
                c.Presupuesto.Items.Select(i => new ItemAnalisisConsultaDto(
                    i.Id, i.IdAnalisis, i.PrecioUsdSnapshot, i.Analisis.Codigo, i.Analisis.Nombre,
                    i.Analisis.Area, i.Analisis.Grupo.CodigoAgrupador)).ToList()));
    }
}
