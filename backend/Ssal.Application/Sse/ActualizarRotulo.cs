namespace Ssal.Application.Sse;

// Corrige número/descripción/estado del rótulo. Operación crítica (RF-13/RF-14):
// exige motivo y audita cada campo modificado por separado.
public class ActualizarRotulo(ISseRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int idSse, ActualizarRotuloCommand cmd, int idUsuario)
    {
        var rotulo = await repo.ObtenerRotuloPorSseAsync(idSse);
        if (rotulo is null) return Resultado.Fallido("No encontrado");

        var cambios = new List<Ssal.Domain.AuditoriaRotulo>();

        if (cmd.NumeroUnico is not null && cmd.NumeroUnico != rotulo.NumeroUnico)
        {
            if (await repo.ExisteOtroRotuloConNumeroAsync(rotulo.IdRotulo, cmd.NumeroUnico))
                return Resultado.Fallido($"El número de rótulo '{cmd.NumeroUnico}' ya existe.");

            cambios.Add(new Ssal.Domain.AuditoriaRotulo { Campo = "numero_unico", ValorAnterior = rotulo.NumeroUnico, ValorNuevo = cmd.NumeroUnico });
            rotulo.NumeroUnico = cmd.NumeroUnico;
        }

        if (cmd.Descripcion is not null && cmd.Descripcion != rotulo.Descripcion)
        {
            cambios.Add(new Ssal.Domain.AuditoriaRotulo { Campo = "descripcion", ValorAnterior = rotulo.Descripcion, ValorNuevo = cmd.Descripcion });
            rotulo.Descripcion = cmd.Descripcion;
        }

        if (cmd.Estado is not null && cmd.Estado != rotulo.Estado)
        {
            cambios.Add(new Ssal.Domain.AuditoriaRotulo { Campo = "estado", ValorAnterior = rotulo.Estado, ValorNuevo = cmd.Estado });
            rotulo.Estado = cmd.Estado;
        }

        if (cambios.Count == 0) return Resultado.Exitoso();

        if (string.IsNullOrWhiteSpace(cmd.Motivo))
            return Resultado.Fallido("Indicá el motivo de la corrección.");

        foreach (var cambio in cambios)
        {
            cambio.IdRotulo = rotulo.IdRotulo;
            cambio.IdUsuario = idUsuario;
            cambio.Motivo = cmd.Motivo;
            cambio.FechaCambio = DateTime.UtcNow;
        }
        repo.AgregarAuditoriasRotulo(cambios);

        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
