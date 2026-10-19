namespace Ssal.Application.Usuarios;

public interface IUsuarioRepository
{
    Task<List<Ssal.Domain.Rol>> ListarRolesAsignablesAsync(string rolNoAsignable);

    Task<List<Ssal.Domain.Usuario>> BuscarAsync(string? busqueda, string? estado);

    Task<Ssal.Domain.Usuario?> ObtenerAsync(int id);

    Task<Ssal.Domain.Rol?> ObtenerRolAsync(int idRol);

    Task<bool> ExisteEmailAsync(string email, int? excluirId);

    void Agregar(Ssal.Domain.Usuario usuario);

    void AgregarAuditoria(Ssal.Domain.AuditoriaSistema entrada);

    Task GuardarCambiosAsync();
}
