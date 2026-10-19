namespace Ssal.Application.Usuarios;

public class ListarRoles(IUsuarioRepository repo)
{
    public async Task<List<RolDto>> EjecutarAsync()
    {
        var roles = await repo.ListarRolesAsignablesAsync(ValidadorUsuario.RolNoAsignable);
        return roles
            .OrderBy(r => r.IdRol)
            .Select(r => new RolDto(r.IdRol, r.Codigo, r.Nombre, r.Descripcion))
            .ToList();
    }
}
