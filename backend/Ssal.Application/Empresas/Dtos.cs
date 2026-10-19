namespace Ssal.Application.Empresas;

public record EmpresaResponseDto(
    int IdEmpresa,
    string RazonSocial,
    string? Cuit,
    string? CondicionIva,
    string? Email,
    string? Telefono,
    string? Direccion,
    string Estado,
    int CantidadClientes);

public record GuardarEmpresaCommand(
    string RazonSocial,
    string? Cuit,
    string? CondicionIva,
    string? Email,
    string? Telefono,
    string? Direccion);
