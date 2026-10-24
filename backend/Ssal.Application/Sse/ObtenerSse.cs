namespace Ssal.Application.Sse;

public class ObtenerSse(ISseRepository repo)
{
    public async Task<SseDetalleDto?> EjecutarAsync(int id)
    {
        var s = await repo.ObtenerDetalleAsync(id);
        if (s is null) return null;

        return new SseDetalleDto(
            s.IdSse, s.Codigo, s.CodigoRpo, s.Area, s.Estado, s.Fecha, s.FormaPago, s.ImporteDolares, s.ImportePesos,
            new UsuarioNombreDto(s.CreadoPorUsuario.Nombre, s.CreadoPorUsuario.Apellido),
            s.Cliente == null ? null : new ClienteDetalleDto(
                s.Cliente.IdCliente, s.Cliente.Nombre, s.Cliente.Apellido, s.Cliente.Email, s.Cliente.Telefono,
                s.Cliente.Empresa == null ? null : new EmpresaResumenDto(s.Cliente.Empresa.RazonSocial)),
            s.Empresa == null ? null : new EmpresaDetalleDto(s.Empresa.IdEmpresa, s.Empresa.RazonSocial, s.Empresa.Email, s.Empresa.Telefono),
            s.Presupuesto == null ? null : new PresupuestoResumenDto(
                s.Presupuesto.IdPresupuesto, s.Presupuesto.CodigoRpo, s.Presupuesto.ImporteDolares, s.Presupuesto.ImportePesos,
                s.Presupuesto.Items.Select(i => new AnalisisResumenDto(i.Analisis.Nombre, i.Analisis.Area)).ToList()),
            s.RotulosInternos
                .OrderBy(r => r.NumeroUnico)
                .Select(r => new RotuloDetalleDto(
                    r.IdRotulo, r.NumeroUnico, r.Descripcion, r.Estado, r.FechaAsignacion,
                    new UsuarioNombreDto(r.AsignadoPorUsuario.Nombre, r.AsignadoPorUsuario.Apellido),
                    r.Muestra == null ? null : new MuestraDto(
                        r.Muestra.IdMuestra, r.Muestra.Tipo, r.Muestra.Estado,
                        r.Muestra.Observacion, r.Muestra.FechaRecepcion,
                        new UsuarioNombreDto(r.Muestra.RecibidoPorUsuario.Nombre, r.Muestra.RecibidoPorUsuario.Apellido)),
                    r.Auditorias
                        .OrderByDescending(a => a.FechaCambio)
                        .Select(a => new AuditoriaRotuloDto(a.IdAuditoria, a.Campo, a.ValorAnterior, a.ValorNuevo, a.Motivo, a.FechaCambio, new UsuarioNombreDto(a.Usuario.Nombre, a.Usuario.Apellido)))
                        .ToList()))
                .ToList());
    }
}
