namespace Ssal.Application.Resultados;

public record ClienteNombreDto(string Nombre, string Apellido);
public record EmpresaNombreDto(string RazonSocial);
public record UsuarioNombreDto(string Nombre, string Apellido);

public record MuestraResumenDto(
    int IdMuestra, string NumeroRotulo, string? CodigoSse, ClienteNombreDto? Cliente, EmpresaNombreDto? Empresa,
    List<string> Areas, int Total, int Pendientes, int PorValidar, int Validados,
    DateTime FechaRecepcion, string EstadoGeneral);

public record AnalisisDetalleDto(
    int IdAnalisis, string Codigo, string Nombre, string Area, string? Unidad,
    decimal? RangoMin, decimal? RangoMax, string? ValorEsperado, string GrupoNombre);

public record ResultadoDto(
    int IdResultado, string? Valor, string Estado, DateTime? FechaCarga, string? MotivoRechazo,
    UsuarioNombreDto? CargadoPor, UsuarioNombreDto? ValidadoPor, DateTime? FechaValidacion,
    AnalisisDetalleDto Analisis, bool? FueraDeRango);

public record InformeResumenDto(int IdInforme, string Codigo, DateTime FechaGeneracion);

public record MuestraDetalleDto(
    int IdMuestra, string? Tipo, string Estado, DateTime FechaRecepcion, string NumeroRotulo, string? CodigoSse,
    ClienteNombreDto? Cliente, EmpresaNombreDto? Empresa, InformeResumenDto? Informe, bool TodoValidado,
    IReadOnlyList<ResultadoDto> Resultados);

public record InformeCreadoDto(int IdInforme, string Codigo);

public record CargarValorCommand(string Valor);

public record ValorCargadoDto(int IdResultado, string Estado, bool? FueraDeRango);

public record ValidarResultadoCommand(bool Aprobado, string? MotivoRechazo);
