namespace Ssal.Application.Usuarios;

public record RolDto(int IdRol, string Codigo, string Nombre, string? Descripcion);

public record UsuarioListadoDto(
    int IdUsuario, string Nombre, string Apellido, string Email, int IdRol,
    string RolCodigo, string RolNombre, bool Activo, DateTime? UltimoAcceso, bool ForzarCambioPwd);

public record GuardarUsuarioCommand(string Nombre, string Apellido, string Email, int IdRol);

public record UsuarioConPasswordDto(string Email, string PasswordTemporal);
