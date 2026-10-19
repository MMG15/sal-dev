namespace Ssal.Application.Auth;

public record LoginResultadoDto(
    string Token, int IdUsuario, string Nombre, string Apellido, string Email,
    string RolCodigo, string RolNombre, bool ForzarCambioPwd);
