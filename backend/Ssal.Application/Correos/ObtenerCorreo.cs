namespace Ssal.Application.Correos;

public class ObtenerCorreo(ICorreoEntranteRepository repo)
{
    public async Task<CorreoDetalleDto?> EjecutarAsync(int id)
    {
        var c = await repo.ObtenerDetalleAsync(id);
        if (c is null) return null;

        return new CorreoDetalleDto(
            c.IdCorreo, c.Remitente, c.RemitenteNombre, c.Asunto, c.Cuerpo, c.FechaRecibido, c.Estado,
            c.UsuarioAccion == null ? null : new UsuarioNombreDto(c.UsuarioAccion.Nombre, c.UsuarioAccion.Apellido),
            c.FechaAccion,
            c.ConsultaGenerada == null ? null : new ConsultaResumenDto(c.ConsultaGenerada.IdConsulta, c.ConsultaGenerada.Codigo),
            c.ConsultaRelacionada == null ? null : new ConsultaRelacionadaDto(
                c.ConsultaRelacionada.IdConsulta, c.ConsultaRelacionada.Codigo,
                c.ConsultaRelacionada.Presupuesto == null ? null : new PresupuestoResumenDto(
                    c.ConsultaRelacionada.Presupuesto.IdPresupuesto, c.ConsultaRelacionada.Presupuesto.CodigoRpo, c.ConsultaRelacionada.Presupuesto.Estado)));
    }
}
