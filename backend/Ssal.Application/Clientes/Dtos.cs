namespace Ssal.Application.Clientes;

public record ClienteResponseDto(
    int IdCliente,
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Cuit,
    string? CondicionIva,
    string? UsuarioWeb,
    string Estado,
    int? IdEmpresa,
    string? NombreEmpresa,
    DateTime CreatedAt);

public record GuardarClienteCommand(
    int? IdEmpresa,
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Cuit,
    string? CondicionIva,
    string? UsuarioWeb);
