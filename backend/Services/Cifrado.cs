using System.Security.Cryptography;
using System.Text;

namespace Ssal.Api.Services;

// Cifrado simétrico (AES) para datos que el sistema necesita poder leer de vuelta,
// como la contraseña de una casilla de correo. A diferencia de BCrypt (que usamos
// para contraseñas de login), esto es reversible a propósito: el servicio de
// importación necesita la contraseña real para conectarse por IMAP.
//
// La clave sale de configuración (MailImport:ClaveCifrado, cargada por user-secrets,
// nunca en el repositorio) y se deriva a 256 bits con SHA-256, así acepta cualquier
// frase como clave.
public static class Cifrado
{
    public static string Encriptar(string textoPlano, string claveSecreta)
    {
        using var aes = Aes.Create();
        aes.Key = DerivarClave(claveSecreta);
        aes.GenerateIV();

        using var encriptador = aes.CreateEncryptor();
        var bytesPlano = Encoding.UTF8.GetBytes(textoPlano);
        var bytesCifrados = encriptador.TransformFinalBlock(bytesPlano, 0, bytesPlano.Length);

        // Guardamos el IV junto con el texto cifrado (hace falta para desencriptar)
        var resultado = new byte[aes.IV.Length + bytesCifrados.Length];
        Buffer.BlockCopy(aes.IV, 0, resultado, 0, aes.IV.Length);
        Buffer.BlockCopy(bytesCifrados, 0, resultado, aes.IV.Length, bytesCifrados.Length);
        return Convert.ToBase64String(resultado);
    }

    public static string Desencriptar(string textoCifradoBase64, string claveSecreta)
    {
        var datos = Convert.FromBase64String(textoCifradoBase64);

        using var aes = Aes.Create();
        aes.Key = DerivarClave(claveSecreta);

        var iv = new byte[16];
        Buffer.BlockCopy(datos, 0, iv, 0, iv.Length);
        aes.IV = iv;

        using var desencriptador = aes.CreateDecryptor();
        var bytesCifrados = new byte[datos.Length - iv.Length];
        Buffer.BlockCopy(datos, iv.Length, bytesCifrados, 0, bytesCifrados.Length);
        var bytesPlano = desencriptador.TransformFinalBlock(bytesCifrados, 0, bytesCifrados.Length);
        return Encoding.UTF8.GetString(bytesPlano);
    }

    private static byte[] DerivarClave(string claveSecreta) =>
        SHA256.HashData(Encoding.UTF8.GetBytes(claveSecreta));
}
