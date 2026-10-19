namespace Ssal.Application.Usuarios;

public class ListarUsuarios(IUsuarioRepository repo)
{
    public async Task<List<UsuarioListadoDto>> EjecutarAsync(string? busqueda, string? estado)
    {
        var usuarios = await repo.BuscarAsync(busqueda, estado);

        return usuarios
            .OrderBy(u => u.Apellido).ThenBy(u => u.Nombre)
            .Select(u => new UsuarioListadoDto(
                u.IdUsuario, u.Nombre, u.Apellido, u.Email, u.IdRol,
                u.Rol.Codigo, u.Rol.Nombre, u.Activo, u.UltimoAcceso, u.ForzarCambioPwd))
            .ToList();
    }
}
