namespace Ssal.Application.Usuarios;

public class DesactivarUsuario(IUsuarioRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, int idUsuarioActual)
    {
        var usuario = await repo.ObtenerAsync(id);
        if (usuario is null) return Resultado.Fallido("No encontrado");
        if (id == idUsuarioActual) return Resultado.Fallido("No podés desactivar tu propia cuenta.");

        usuario.Activo = false;
        repo.AgregarAuditoria(new Ssal.Domain.AuditoriaSistema
        {
            IdUsuario = idUsuarioActual,
            Accion = "desactivar_usuario",
            Objetivo = usuario.Email,
            Detalle = null
        });
        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
