namespace Ssal.Application.Usuarios;

public class ActivarUsuario(IUsuarioRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, int idUsuarioActual)
    {
        var usuario = await repo.ObtenerAsync(id);
        if (usuario is null) return Resultado.Fallido("No encontrado");

        usuario.Activo = true;
        repo.AgregarAuditoria(new Ssal.Domain.AuditoriaSistema
        {
            IdUsuario = idUsuarioActual,
            Accion = "activar_usuario",
            Objetivo = usuario.Email,
            Detalle = null
        });
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
