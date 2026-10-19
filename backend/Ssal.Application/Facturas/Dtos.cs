namespace Ssal.Application.Facturas;

public record SseResumenDto(int IdSse, string? Codigo, string Area);
public record ClienteNombreDto(string Nombre, string Apellido);
public record EmpresaNombreDto(string RazonSocial);

public record FacturaResumenDto(
    int IdFactura, string? Numero, string Estado, DateTime Fecha, DateTime? FechaPago,
    decimal ImportePesos, decimal ImporteDolares,
    SseResumenDto Sse, ClienteNombreDto? Cliente, EmpresaNombreDto? Empresa, string? Rotulo);

public record SseDisponibleDto(
    int IdSse, string? Codigo, string Area, DateTime Fecha, decimal ImportePesos, decimal ImporteDolares,
    ClienteNombreDto? Cliente, EmpresaNombreDto? Empresa, string? Rotulo);

public record RegistrarFacturaCommand(int IdSse, string Numero, DateTime? Fecha);

public record MarcarPagadaCommand(DateTime? FechaPago);
