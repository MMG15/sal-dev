namespace Ssal.Application.Comun;

// Cifrado reversible (AES) para contraseñas que el sistema necesita poder leer de vuelta,
// como la de una casilla de correo. La implementación real vive en Ssal.Infrastructure.
public interface ICifradoContrasenas
{
    string Encriptar(string passwordPlano);

    string Desencriptar(string textoCifrado);
}
