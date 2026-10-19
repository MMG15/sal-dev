namespace Ssal.Application.Correos;

public record CorreoResumenDto(
    int IdCorreo, string Remitente, string? RemitenteNombre, string? Asunto, string? Resumen,
    DateTime FechaRecibido, string Estado, int? IdConsultaGenerada, int? IdConsultaRelacionada);

public record UsuarioNombreDto(string Nombre, string Apellido);
public record ConsultaResumenDto(int IdConsulta, string? Codigo);
public record PresupuestoResumenDto(int IdPresupuesto, string? CodigoRpo, string Estado);
public record ConsultaRelacionadaDto(int IdConsulta, string? Codigo, PresupuestoResumenDto? Presupuesto);

public record CorreoDetalleDto(
    int IdCorreo, string Remitente, string? RemitenteNombre, string? Asunto, string? Cuerpo,
    DateTime FechaRecibido, string Estado, UsuarioNombreDto? UsuarioAccion, DateTime? FechaAccion,
    ConsultaResumenDto? ConsultaGenerada, ConsultaRelacionadaDto? ConsultaRelacionada);

public record ResponderCorreoCommand(string Cuerpo);

public record ConvertirCorreoCommand(int? IdCliente, int? IdEmpresa);

public record ConsultaCreadaDto(int IdConsulta, string Codigo);
