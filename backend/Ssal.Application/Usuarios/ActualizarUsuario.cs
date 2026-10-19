namespace Ssal.Application.Usuarios;

public class ActualizarUsuario(IUsuarioRepository repo)
{
    public async Task<Resultado> EjecutarAsync(int id, GuardarUsuarioCommand cmd, int idUsuarioActual)
    {
        var usuario = await repo.ObtenerAsync(id);
        if (usuario is null) return Resultado.Fallido("No encontrado");

        var error = await new ValidadorUsuario(repo).ValidarAsync(cmd, idExcluido: id);
        if (error is not null) return Resultado.Fallido(error);

        // Evita que un administrador se quede sin acceso por error
        if (id == idUsuarioActual && cmd.IdRol != usuario.IdRol)
            return Resultado.Fallido("No podés cambiar tu propio rol.");

        var cambios = new List<string>();
        if (usuario.Nombre != cmd.Nombre.Trim() || usuario.Apellido != cmd.Apellido.Trim())
            cambios.Add("datos personales");
        if (usuario.Email != cmd.Email.Trim().ToLowerInvariant())
            cambios.Add("correo");

        var rolAnterior = usuario.IdRol;
        if (rolAnterior != cmd.IdRol)
        {
            var codigoAnterior = (await repo.ObtenerRolAsync(rolAnterior))?.Codigo ?? "?";
            var codigoNuevo = (await repo.ObtenerRolAsync(cmd.IdRol))?.Codigo ?? "?";
            cambios.Add($"rol {codigoAnterior} → {codigoNuevo}");
        }

        usuario.Nombre = cmd.Nombre.Trim();
        usuario.Apellido = cmd.Apellido.Trim();
        usuario.Email = cmd.Email.Trim().ToLowerInvariant();
        usuario.IdRol = cmd.IdRol;

        if (cambios.Count > 0)
        {
            var accion = rolAnterior != cmd.IdRol ? "cambio_rol" : "modificar_usuario";
            repo.AgregarAuditoria(new Ssal.Domain.AuditoriaSistema
            {
                IdUsuario = idUsuarioActual,
                Accion = accion,
                Objetivo = usuario.Email,
                Detalle = string.Join("; ", cambios)
            });
        }

        await repo.GuardarCambiosAsync();
        return Resultado.Exitoso();
    }
}
