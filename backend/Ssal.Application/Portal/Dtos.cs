namespace Ssal.Application.Portal;

public record LoginPortalResultadoDto(
    string Token, int IdUsuario, string Nombre, string Apellido, string Email,
    string RolCodigo, string RolNombre, bool ForzarCambioPwd);

public record CambiarPasswordPortalCommand(string PasswordActual, string? PasswordNueva);

public record InformeResumenDto(int IdInforme, string Codigo, DateTime FechaGeneracion);

public record PedidoDto(int IdMuestra, string NumeroRotulo, string? CodigoSse, DateTime FechaRecepcion, string Estado, InformeResumenDto? Informe);

public record ResultadoPortalDto(string Area, string Analisis, string? Valor, string? Unidad, string? Referencia);

public record InformeDetallePortalDto(int IdInforme, string Codigo, DateTime Fecha, string Rotulo, string? CodigoSse, IReadOnlyList<ResultadoPortalDto> Resultados);
