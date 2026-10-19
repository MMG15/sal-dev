using System.Security.Cryptography;

namespace Ssal.Application.Comun;

public static class Contrasenas
{
    // Sin caracteres ambiguos (0/O, 1/l/I) para que sea fácil de dictar
    private const string Alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";

    public static string Temporal() => RandomNumberGenerator.GetString(Alfabeto, 10);
}
