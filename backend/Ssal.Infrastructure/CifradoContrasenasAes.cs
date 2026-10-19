using Microsoft.Extensions.Configuration;
using Ssal.Application.Comun;

namespace Ssal.Infrastructure;

public class CifradoContrasenasAes(IConfiguration config) : ICifradoContrasenas
{
    private string ClaveCifrado => config["MailImport:ClaveCifrado"]
        ?? throw new InvalidOperationException("MailImport:ClaveCifrado no está configurada.");

    public string Encriptar(string passwordPlano) => Cifrado.Encriptar(passwordPlano, ClaveCifrado);

    public string Desencriptar(string textoCifrado) => Cifrado.Desencriptar(textoCifrado, ClaveCifrado);
}
