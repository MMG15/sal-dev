namespace Ssal.Application.Auth;

public interface IAuthRepository
{
    Task<Ssal.Domain.Usuario?> ObtenerActivoPorEmailAsync(string email);

    Task<Ssal.Domain.Usuario?> ObtenerAsync(int id);

    void AgregarAuditoria(Ssal.Domain.AuditoriaSistema entrada);

    Task GuardarCambiosAsync();
}

// La implementación real (JWT) vive en Ssal.Infrastructure.
public interface IGeneradorToken
{
    string GenerarPara(Ssal.Domain.Usuario usuario);
}
