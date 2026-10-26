namespace Ssal.Application.Presupuestos;

public record ClienteNombreEmailDto(string Nombre, string Apellido, string? Email);
public record EmpresaNombreDto(string RazonSocial);
public record UsuarioNombreDto(string Nombre, string Apellido);

public record PresupuestoResumenDto(
    int IdPresupuesto, string? CodigoRpo, string Estado, decimal ImporteDolares, decimal ImportePesos,
    decimal Cotizacion, DateTime Fecha, string? ConsultaCodigo, int IdConsulta,
    ClienteNombreEmailDto? Cliente, EmpresaNombreDto? Empresa, UsuarioNombreDto CreadoPor);

public record ItemPresupuestoDto(int Id, int IdAnalisis, decimal PrecioUsdSnapshot, string Codigo, string Nombre, string Area, string GrupoNombre);

public record ItemPaqueteDto(int Id, int IdPaquete, decimal PrecioUsdSnapshot, string Nombre, string Area, IReadOnlyList<string> AnalisisIncluidos);

public record ItemMetodoDto(int Id, int IdMetodo, string Nombre, string? Descripcion, string Area);

public record EmpresaConEmailDto(int IdEmpresa, string RazonSocial, string? Email);

public record ClienteDetalleDto(int IdCliente, string Nombre, string Apellido, string? Email, string? Telefono, EmpresaConEmailDto? Empresa);

public record ConsultaResumenDto(int IdConsulta, string? Codigo, string Canal, string? Descripcion, ClienteDetalleDto? Cliente, EmpresaConEmailDto? Empresa);

public record PresupuestoDetalleDto(
    int IdPresupuesto, string? CodigoRpo, string Estado, decimal ImporteDolares, decimal ImportePesos,
    decimal Cotizacion, decimal AdicionalAsesoramiento, string? TipoAsesoramiento, string MostrarMoneda, DateTime Fecha,
    ConsultaResumenDto Consulta, UsuarioNombreDto CreadoPor,
    IReadOnlyList<ItemPresupuestoDto> Items, IReadOnlyList<ItemPaqueteDto> Paquetes, IReadOnlyList<ItemMetodoDto> Metodos);

public record ItemPresupuestoCommand(int IdAnalisis);

public record ItemPaqueteCommand(int IdPaquete);

public record ItemMetodoCommand(int IdMetodo);

public record GuardarPresupuestoCommand(
    int IdConsulta, List<ItemPresupuestoCommand> Items, List<ItemPaqueteCommand> Paquetes, decimal Cotizacion,
    decimal AdicionalAsesoramiento, string? TipoAsesoramiento, string? MostrarMoneda, List<ItemMetodoCommand>? Metodos);

public record CrearPresupuestoResultadoDto(int IdPresupuesto, string? CodigoRpo, decimal ImporteDolares, decimal ImportePesos);

public record CambiarEstadoPresupuestoCommand(string Estado);

public record EnviarPresupuestoCommand(string Destinatario, string Asunto, string Cuerpo);

public record ArchivoPresupuestoDto(int Id, string NombreOriginal, string TipoMime, long TamañoBytes, DateTime SubidoEn, string SubidoPor);

public record ArchivoSubidoDto(int Id, string NombreOriginal, long TamañoBytes);

public record AdjuntarDesdeBibliotecaCommand(int IdDocumento);

public record ArchivoContenidoDto(byte[] Contenido, string TipoMime, string NombreOriginal);
