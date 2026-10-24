namespace Ssal.Application.Consultas;

public record ClienteResumenDto(int IdCliente, string Nombre, string Apellido, string? Email);
public record EmpresaResumenDto(int IdEmpresa, string RazonSocial);
public record UsuarioResumenDto(int IdUsuario, string Nombre, string Apellido);

public record ConsultaResumenDto(
    int IdConsulta, string? Codigo, string Canal, string Estado, string? Descripcion,
    DateTime Fecha, DateTime CreatedAt,
    ClienteResumenDto? Cliente, EmpresaResumenDto? Empresa, UsuarioResumenDto? Responsable,
    bool TienePresupuesto, string? EstadoPresupuesto,
    int DiasSinResponder, bool Pendiente);

public record ItemAnalisisConsultaDto(int Id, int IdAnalisis, decimal PrecioUsdSnapshot, string Codigo, string Nombre, string Area, string GrupoNombre);

public record ItemPaqueteConsultaDto(int Id, int IdPaquete, decimal PrecioUsdSnapshot, string Nombre, string Area, IReadOnlyList<string> AnalisisIncluidos);

public record PresupuestoResumenDto(
    int IdPresupuesto, string? CodigoRpo, string Estado, decimal ImporteDolares, decimal ImportePesos,
    decimal Cotizacion, decimal AdicionalAsesoramiento, string? TipoAsesoramiento, DateTime Fecha,
    IReadOnlyList<ItemAnalisisConsultaDto> Items, IReadOnlyList<ItemPaqueteConsultaDto> Paquetes);

public record ClienteDetalleDto(int IdCliente, string Nombre, string Apellido, string? Email, string? Telefono, EmpresaResumenDto? Empresa);

public record ConsultaDetalleDto(
    int IdConsulta, string? Codigo, string Canal, string Estado, string? Descripcion,
    DateTime Fecha, DateTime CreatedAt,
    ClienteDetalleDto? Cliente, EmpresaResumenDto? Empresa, UsuarioResumenDto? Responsable,
    PresupuestoResumenDto? Presupuesto);

public record CrearConsultaCommand(string? Canal, string? Descripcion, int? IdCliente, int? IdEmpresa, int? IdResponsable);

public record ActualizarConsultaCommand(string? Canal, string? Estado, string? Descripcion, int? IdCliente, int? IdEmpresa, int? IdResponsable);
