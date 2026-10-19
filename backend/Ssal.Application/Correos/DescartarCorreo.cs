namespace Ssal.Application.Correos;

public class DescartarCorreo(ICorreoEntranteRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, int idUsuario)
    {
        var c = await repo.ObtenerAsync(id);
        if (c is null) return Resultado.Fallido("No encontrado");
        if (c.Estado != "pendiente")
            return Resultado.Fallido("Este correo ya fue procesado.");

        c.Estado = "descartado";
        c.IdUsuarioAccion = idUsuario;
        c.FechaAccion = DateTime.UtcNow;
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
