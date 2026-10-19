namespace Ssal.Application.Consultas;

public record ListarConsultasResultado(int Total, int Pagina, int PorPagina, List<ConsultaResumenDto> Items);

public class ListarConsultas(IConsultaRepository repo)
{
    // Días sin respuesta a partir de los cuales una consulta "recibida" se considera pendiente/demorada.
    private const int DiasParaPendiente = 2;

    public async Task<ListarConsultasResultado> EjecutarAsync(string? busqueda, string? estado, int pagina, int porPagina)
    {
        // "pendiente" es un estado virtual: consultas recibidas que llevan varios días sin respuesta.
        DateTime? limitePendiente = estado == "pendiente" ? DateTime.UtcNow.AddDays(-DiasParaPendiente) : null;
        var estadoReal = estado == "pendiente" ? null : estado;

        var (total, consultas) = await repo.BuscarAsync(busqueda, estadoReal, limitePendiente, pagina, porPagina);

        // El repositorio ya devuelve la página ordenada (más reciente primero) y recortada.
        var ahora = DateTime.UtcNow;
        var items = consultas
            .Select(c => new ConsultaResumenDto(
                c.IdConsulta, c.Codigo, c.Canal, c.Estado, c.Descripcion, c.Fecha, c.CreatedAt,
                c.Cliente == null ? null : new ClienteResumenDto(c.Cliente.IdCliente, c.Cliente.Nombre, c.Cliente.Apellido, c.Cliente.Email),
                c.Empresa == null ? null : new EmpresaResumenDto(c.Empresa.IdEmpresa, c.Empresa.RazonSocial),
                c.Responsable == null ? null : new UsuarioResumenDto(c.Responsable.IdUsuario, c.Responsable.Nombre, c.Responsable.Apellido),
                c.Presupuesto != null,
                c.Presupuesto?.Estado,
                (int)(ahora - c.CreatedAt).TotalDays,
                c.Estado == "recibida" && (ahora - c.CreatedAt).TotalDays >= DiasParaPendiente))
            .ToList();

        return new ListarConsultasResultado(total, pagina, porPagina, items);
    }
}
