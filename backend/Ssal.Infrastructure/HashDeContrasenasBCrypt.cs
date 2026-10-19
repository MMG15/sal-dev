using Ssal.Application.Comun;

namespace Ssal.Infrastructure;

public class HashDeContrasenasBCrypt : IHashDeContrasenas
{
    public string Hashear(string passwordPlano) => BCrypt.Net.BCrypt.HashPassword(passwordPlano);

    public bool Verificar(string passwordPlano, string hash) => BCrypt.Net.BCrypt.Verify(passwordPlano, hash);
}
