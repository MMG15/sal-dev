using Ssal.Application.Sse;
using Ssal.Domain;

namespace Ssal.Infrastructure;

public class SsePdfGenerator : ISsePdfGenerator
{
    public byte[] Generar(Sse sse) => SsePdfBuilder.Generar(sse);
}
