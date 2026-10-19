namespace Ssal.Application.CuentasCorreo;

public record CuentaCorreoDto(
    int IdCuenta, string Nombre, string Servidor, int Puerto, string Usuario,
    bool Habilitada, int IntervaloMinutos, DateTime? UltimaRevision, string? UltimoError);

public record GuardarCuentaCorreoCommand(
    string Nombre, string? Servidor, int? Puerto, string Usuario, string? Password,
    bool? Habilitada, int? IntervaloMinutos);

public record CuentaCreadaDto(int IdCuenta);

public record RevisionResultadoDto(int Importados);
