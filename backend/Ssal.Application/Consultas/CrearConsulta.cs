using Ssal.Application.Comun;

namespace Ssal.Application.Consultas;

public class CrearConsulta(IConsultaRepository repo)
{
    public async Task<(int IdConsulta, string Codigo)> EjecutarAsync(CrearConsultaCommand cmd)
    {
        var hoy = DateTime.UtcNow;
        var prefijo = $"CF-{hoy:yyyyMMdd}-";
        var ultimoCodigo = await repo.ObtenerUltimoCodigoAsync(prefijo);
        var codigo = GeneradorCodigoCorrelativo.Siguiente(prefijo, ultimoCodigo);

        var consulta = new Ssal.Domain.Consulta
        {
            Codigo = codigo,
            Canal = cmd.Canal ?? "mail",
            Estado = "recibida",
            Descripcion = cmd.Descripcion,
            Fecha = hoy,
            CreatedAt = hoy,
            IdCliente = cmd.IdCliente,
            IdEmpresa = cmd.IdEmpresa,
            IdResponsable = cmd.IdResponsable
        };

        repo.Agregar(consulta);
        await repo.GuardarCambiosAsync();

        return (consulta.IdConsulta, consulta.Codigo);
    }
}
