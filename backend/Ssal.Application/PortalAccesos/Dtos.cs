namespace Ssal.Application.PortalAccesos;

public record EstadoAccesoDto(bool Existe, string? Email, bool? Activo, bool? ForzarCambioPwd, DateTime? UltimoAcceso)
{
    public static EstadoAccesoDto NoExiste() => new(false, null, null, null, null);
}

public record GenerarAccesoCommand(string? Email);

public record AccesoConPasswordDto(string Email, string PasswordTemporal);
