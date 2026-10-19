namespace Ssal.Application;

// Resultado de un caso de uso que puede fallar por una regla de negocio (no por una excepción).
// El caso de uso no sabe nada de HTTP: el controller traduce Resultado.Error a un 400/404.
public class Resultado
{
    public bool Ok { get; }
    public string? Error { get; }

    protected Resultado(bool ok, string? error)
    {
        Ok = ok;
        Error = error;
    }

    public static Resultado Exitoso() => new(true, null);
    public static Resultado Fallido(string error) => new(false, error);
}

public class Resultado<T> : Resultado
{
    public T? Valor { get; }

    private Resultado(bool ok, T? valor, string? error) : base(ok, error)
    {
        Valor = valor;
    }

    public static Resultado<T> Exitoso(T valor) => new(true, valor, null);
    public static new Resultado<T> Fallido(string error) => new(false, default, error);
}
