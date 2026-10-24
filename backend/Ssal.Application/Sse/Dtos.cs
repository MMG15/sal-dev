namespace Ssal.Application.Sse;

public record ClienteResumenDto(string Nombre, string Apellido, string? Email);
public record EmpresaResumenDto(string RazonSocial);
public record RotuloResumenDto(int IdRotulo, string NumeroUnico, string Estado);
public record UsuarioNombreDto(string Nombre, string Apellido);

public record SseResumenDto(
    int IdSse, string? Codigo, string? CodigoRpo, string Area, string Estado, DateTime Fecha,
    string? FormaPago, decimal ImporteDolares, decimal ImportePesos,
    ClienteResumenDto? Cliente, EmpresaResumenDto? Empresa, IReadOnlyList<RotuloResumenDto> Rotulos);

public record ClienteDetalleDto(int IdCliente, string Nombre, string Apellido, string? Email, string? Telefono, EmpresaResumenDto? Empresa);
public record EmpresaDetalleDto(int IdEmpresa, string RazonSocial, string? Email, string? Telefono);
public record AnalisisResumenDto(string Nombre, string Area);
public record PresupuestoResumenDto(int IdPresupuesto, string? CodigoRpo, decimal ImporteDolares, decimal ImportePesos, IReadOnlyList<AnalisisResumenDto> Analisis);

public record MuestraDto(int IdMuestra, string? Tipo, string Estado, string? Observacion, DateTime FechaRecepcion, UsuarioNombreDto RecibidoPor);

public record AuditoriaRotuloDto(int IdAuditoria, string Campo, string? ValorAnterior, string? ValorNuevo, string? Motivo, DateTime FechaCambio, UsuarioNombreDto Usuario);

public record RotuloDetalleDto(
    int IdRotulo, string NumeroUnico, string? Descripcion, string Estado, DateTime FechaAsignacion,
    UsuarioNombreDto AsignadoPor, MuestraDto? Muestra, IReadOnlyList<AuditoriaRotuloDto> Auditorias);

public record SseDetalleDto(
    int IdSse, string? Codigo, string? CodigoRpo, string Area, string Estado, DateTime Fecha,
    string? FormaPago, decimal ImporteDolares, decimal ImportePesos, UsuarioNombreDto CreadoPor,
    ClienteDetalleDto? Cliente, EmpresaDetalleDto? Empresa, PresupuestoResumenDto? Presupuesto, IReadOnlyList<RotuloDetalleDto> Rotulos);

public record CrearSseCommand(int? IdPresupuesto, int? IdCliente, string Area, string? FormaPago, decimal ImporteDolares, decimal ImportePesos);

public record CrearSseResultadoDto(int IdSse, string? Codigo);

public record AsignarClienteSseCommand(int? IdCliente, int? IdEmpresa);

public record CambiarEstadoSseCommand(string Estado);

// Cantidad: cuántas muestras repetidas trae este pedido (default 1). Si es más de 1, se crean
// varios rótulos de una vez, todos con el mismo número base y sufijo de letra (-A, -B, -C...);
// se ignora si se especifica un NumeroUnico manual (ese modo siempre crea un solo rótulo).
public record AsignarRotuloCommand(string? NumeroUnico, string? Descripcion, int? Cantidad);

public record RotuloCreadoDto(int IdRotulo, string NumeroUnico);

public record RegistrarMuestraCommand(string? Tipo, string? Observacion, string Condicion);

public record ActualizarRotuloCommand(string? NumeroUnico, string? Estado, string? Descripcion, string? Motivo);
