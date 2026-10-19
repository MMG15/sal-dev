namespace Ssal.Application.Comun;

// La implementación real (BCrypt) vive en Ssal.Infrastructure.
public interface IHashDeContrasenas
{
    string Hashear(string passwordPlano);

    bool Verificar(string passwordPlano, string hash);
}
