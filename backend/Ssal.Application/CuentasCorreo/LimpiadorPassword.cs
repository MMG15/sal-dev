namespace Ssal.Application.CuentasCorreo;

// Google muestra la contraseña de aplicación separada en grupos de 4 letras para que
// sea fácil de leer, pero eso no es parte de la contraseña real. Además, copiar texto
// desde una página puede traer caracteres invisibles que no son espacios comunes
// (separadores "no-break", etc.) y que un filtro de solo espacios no detecta. Por eso
// nos quedamos únicamente con letras y números, que es lo único que una contraseña de
// aplicación de Google puede tener.
internal static class LimpiadorPassword
{
    public static string Limpiar(string password) =>
        new string(password.Where(char.IsLetterOrDigit).ToArray());
}
