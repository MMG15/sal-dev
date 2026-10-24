using Ssal.Application.Presupuestos;
using Ssal.Domain;

namespace Ssal.Infrastructure;

public class PresupuestoPdfGenerator : IPresupuestoPdfGenerator
{
    public byte[] Generar(Presupuesto presupuesto, IReadOnlyList<FirmaDocumento> firmasCicloActual) =>
        PresupuestoPdfBuilder.Generar(presupuesto, firmasCicloActual);
}
