using Ssal.Application.Resultados;
using Ssal.Domain;

namespace Ssal.Infrastructure;

public class InformePdfGenerator : IInformePdfGenerator
{
    public byte[] Generar(Informe informe, string estadoFirma, IReadOnlyList<FirmaDocumento> firmas) =>
        InformePdfBuilder.Generar(informe, estadoFirma, firmas);
}
