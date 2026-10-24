namespace Ssal.Application.Firmas;

// Funciones puras compartidas por varios casos de uso del módulo: arman el DocInfoDto a partir
// de la entidad real (Informe o Presupuesto) y calculan qué puede hacer el usuario actual con
// un documento según su rol y el estado del ciclo de firma.
internal static class MapeoDocumentos
{
    public static DocInfoDto MapInforme(Ssal.Domain.Informe i)
    {
        var sse = i.Muestra.RotuloInterno.Sse;
        return new DocInfoDto(
            "informe",
            i.IdInforme,
            i.Codigo,
            DestinatarioSse(sse),
            EmailSse(sse),
            sse.Codigo,
            i.Muestra.RotuloInterno.NumeroUnico,
            i.FechaGeneracion,
            i.Muestra.Resultados.Select(r => r.Analisis.Area).Distinct().OrderBy(a => a).ToList());
    }

    public static DocInfoDto MapPresupuesto(Ssal.Domain.Presupuesto p)
    {
        var cliente = p.Consulta.Cliente;
        var destinatario = cliente is not null
            ? $"{cliente.Nombre} {cliente.Apellido}"
            : p.Consulta.Empresa?.RazonSocial ?? "—";

        // Las áreas que tienen que firmar salen tanto de los análisis sueltos como de los
        // incluidos en cada paquete — si el presupuesto tiene solo paquetes, igual hay que pedir
        // la firma de los responsables de esas áreas.
        var areas = p.Items.Select(i => i.Analisis.Area)
            .Concat(p.Paquetes.SelectMany(pp => pp.Paquete.Items.Select(pa => pa.Analisis.Area)))
            .Distinct().OrderBy(a => a).ToList();

        return new DocInfoDto(
            "presupuesto",
            p.IdPresupuesto,
            p.CodigoRpo ?? $"PRES-{p.IdPresupuesto}",
            destinatario,
            null,
            null,
            null,
            p.Fecha,
            areas);
    }

    private static string DestinatarioSse(Ssal.Domain.Sse s) =>
        s.Cliente is not null ? $"{s.Cliente.Nombre} {s.Cliente.Apellido}"
        : s.Presupuesto?.Consulta.Empresa?.RazonSocial ?? "—";

    private static string? EmailSse(Ssal.Domain.Sse s) =>
        s.Cliente?.Email ?? s.Presupuesto?.Consulta.Empresa?.Email;

    public static string? AreaDelRol(string rol) => rol switch
    {
        "ROL-08" => "MIC",
        "ROL-09" => "FQ",
        _ => null
    };

    public static PermisosDto CalcularPermisos(DocInfoDto doc, Ssal.Domain.CicloFirma.Resultado ciclo, string rolActual)
    {
        var area = AreaDelRol(rolActual);
        var esResponsable = area is not null;
        var esAdmin = rolActual == "ROL-01";

        var puedeFirmar = esResponsable
            && ciclo.Estado == Ssal.Domain.CicloFirma.PendienteFirma
            && doc.Areas.Contains(area!)
            && !ciclo.AreasFirmadas.Contains(area!);

        var puedeAprobar = esAdmin && ciclo.Estado == Ssal.Domain.CicloFirma.Firmado;

        var puedeRechazar = (esAdmin && ciclo.Estado == Ssal.Domain.CicloFirma.Firmado)
            || (esResponsable && ciclo.Estado == Ssal.Domain.CicloFirma.PendienteFirma && doc.Areas.Contains(area!));

        var rolesReenvio = doc.Tipo == "informe"
            ? new[] { "ROL-01", "ROL-07", "ROL-08", "ROL-09" }
            : new[] { "ROL-01", "ROL-03" };
        var puedeReenviar = ciclo.Estado == Ssal.Domain.CicloFirma.Rechazado && rolesReenvio.Contains(rolActual);

        return new PermisosDto(puedeFirmar, puedeAprobar, puedeRechazar, puedeReenviar);
    }
}
