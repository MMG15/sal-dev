using Ssal.Application.Comun;

namespace Ssal.Application.Correos;

// Crea una Consulta formal (mismo correlativo CF-AAAAMMDD-NNN que la carga manual).
public class ConvertirCorreo(ICorreoEntranteRepository repo)
{
    public async Task<Resultado<ConsultaCreadaDto>> EjecutarAsync(int id, ConvertirCorreoCommand cmd, int idUsuario)
    {
        var correo = await repo.ObtenerAsync(id);
        if (correo is null) return Resultado<ConsultaCreadaDto>.Fallido("No encontrado");
        if (correo.Estado != "pendiente")
            return Resultado<ConsultaCreadaDto>.Fallido("Este correo ya fue procesado.");

        var hoy = DateTime.UtcNow;
        var prefijo = $"CF-{hoy:yyyyMMdd}-";
        var ultimoCodigo = await repo.ObtenerUltimoCodigoConsultaAsync(prefijo);
        var codigo = GeneradorCodigoCorrelativo.Siguiente(prefijo, ultimoCodigo);

        var descripcion = string.IsNullOrWhiteSpace(correo.Asunto)
            ? correo.Cuerpo
            : $"{correo.Asunto}\n\n{correo.Cuerpo}";

        var consulta = new Ssal.Domain.Consulta
        {
            Codigo = codigo,
            Canal = "mail",
            Estado = "recibida",
            Descripcion = descripcion,
            Fecha = correo.FechaRecibido,
            CreatedAt = hoy,
            IdCliente = cmd.IdCliente,
            IdEmpresa = cmd.IdEmpresa,
            Origen = "automatico",
            ExternoId = correo.ExternoId
        };
        repo.AgregarConsulta(consulta);

        correo.Estado = "convertido";
        correo.IdUsuarioAccion = idUsuario;
        correo.FechaAccion = hoy;
        correo.ConsultaGenerada = consulta;

        await repo.GuardarCambiosAsync();
        return Resultado<ConsultaCreadaDto>.Exitoso(new ConsultaCreadaDto(consulta.IdConsulta, consulta.Codigo!));
    }
}
