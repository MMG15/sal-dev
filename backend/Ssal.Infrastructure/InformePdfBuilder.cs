using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ssal.Domain;

namespace Ssal.Infrastructure;

public static class InformePdfBuilder
{
    public static byte[] Generar(Informe informe, string estadoFirma, IReadOnlyList<FirmaDocumento> firmas)
    {
        var muestra = informe.Muestra;
        var sse = muestra.RotuloInterno.Sse;
        var cliente = sse.Cliente;
        // La empresa puede venir por el cliente de la SSE o por la consulta del presupuesto
        var empresa = cliente?.Empresa ?? sse.Presupuesto?.Consulta.Empresa;

        string nombreDestinatario = cliente is not null
            ? $"{cliente.Nombre} {cliente.Apellido}"
            : empresa?.RazonSocial ?? "—";

        var resultados = muestra.Resultados
            .OrderBy(r => r.Analisis.Area)
            .ThenBy(r => r.Analisis.Nombre)
            .ToList();

        var responsable = $"{informe.GeneradoPorUsuario.Nombre} {informe.GeneradoPorUsuario.Apellido}";
        var ahora = DateTime.Now;
        var firmantes = PdfEstilo.ArmarFirmantes(firmas);

        var documento = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(t => t.FontSize(9.5f).FontColor(PdfEstilo.GrisTexto));

                page.Header().Element(c => PdfEstilo.Encabezado(c, "INFORME DE RESULTADOS", informe.Codigo));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(10);

                    if (estadoFirma != CicloFirma.Aprobado)
                    {
                        col.Item().Background(Colors.Red.Lighten4).Padding(6).Text("BORRADOR: sin firma y aprobación final. No válido para entrega al cliente.")
                            .FontSize(9).SemiBold().FontColor(Colors.Red.Darken2);
                    }

                    col.Item().Background(PdfEstilo.TealClaro).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("SSE: ").SemiBold(); t.Span(sse.Codigo ?? "—"); });
                            c.Item().Text(t => { t.Span("N° Rótulo: ").SemiBold(); t.Span(muestra.RotuloInterno.NumeroUnico); });
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Cliente: ").SemiBold(); t.Span(nombreDestinatario); });
                            if (empresa is not null)
                                c.Item().Text(t => { t.Span("Empresa: ").SemiBold(); t.Span(empresa.RazonSocial); });
                            c.Item().Text(t => { t.Span("Fecha: ").SemiBold(); t.Span(informe.FechaGeneracion.ToString("dd/MM/yyyy")); });
                        });
                    });

                    foreach (var r in resultados)
                        col.Item().Element(c => BloqueAnalisis(c, r));

                    col.Item().Text(
                        "Todos los análisis listados fueron validados por el Responsable de Área correspondiente antes de la emisión de este informe."
                    ).FontSize(8.5f).Italic().FontColor(Colors.Grey.Darken1);

                    // ── Firmas digitales ──
                    col.Item().PaddingTop(6).Element(c => PdfEstilo.BloqueFirmas(c, firmantes, ahora));
                });

                page.Footer().Element(c => PdfEstilo.Pie(c, responsable, ahora));
            });
        });

        return documento.GeneratePdf();
    }

    // Un bloque detallado por análisis (resultado, método + límite de detección, referencia
    // normativa y criterio de aceptación), siguiendo el nivel de detalle del informe en papel del
    // laboratorio, con los colores/estilo propios del sistema.
    private static void BloqueAnalisis(IContainer container, Resultado r)
    {
        var fueraDeRango = EvaluadorDeRango.FueraDeRango(r.Valor, r.Analisis) ?? false;
        var a = r.Analisis;

        var referenciaSimple = a.ValorEsperado is not null
            ? $"Esperado: {a.ValorEsperado}"
            : (a.RangoMin is not null || a.RangoMax is not null)
                ? $"Rango: {a.RangoMin?.ToString() ?? "—"} a {a.RangoMax?.ToString() ?? "—"} {a.Unidad ?? ""}".TrimEnd()
                : null;

        container.Border(1).BorderColor(PdfEstilo.GrisBorde).Padding(8).Column(col =>
        {
            col.Spacing(3);

            col.Item().Row(row =>
            {
                row.RelativeItem().Text(t =>
                {
                    t.Span($"{a.Area} · ").FontColor(PdfEstilo.Teal).SemiBold().FontSize(8.5f);
                    t.Span(a.Nombre).SemiBold().FontSize(10).FontColor(PdfEstilo.VerdeOscuro);
                });
                if (r.FechaResultado is not null)
                    row.AutoItem().Text($"F.R. {r.FechaResultado:dd/MM/yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            // Para resultados cualitativos (ValorEsperado tipo "Ausencia") la unidad del análisis
            // suele describir el formato de reporte (ej. "Ausencia/Presencia en 100 mL"), que ya
            // queda implícito en el valor — no se repite para no duplicar el texto.
            var textoResultado = string.IsNullOrWhiteSpace(r.Valor)
                ? "—"
                : a.ValorEsperado is not null
                    ? r.Valor
                    : $"{r.Valor} {a.Unidad}".Trim();

            col.Item().Text(t =>
            {
                t.Span("Resultado: ").SemiBold().FontSize(9.5f);
                if (fueraDeRango)
                    t.Span(textoResultado).SemiBold().FontSize(9.5f).FontColor(Colors.Red.Darken2);
                else
                    t.Span(textoResultado).FontSize(9.5f);
            });

            if (fueraDeRango)
                col.Item().Text("Resultado fuera del valor/rango de referencia establecido.")
                    .FontSize(8).SemiBold().FontColor(Colors.Red.Darken2);

            if (a.Metodo is not null || !string.IsNullOrWhiteSpace(a.LimiteDeteccion))
            {
                col.Item().Text(t =>
                {
                    t.Span("Método: ").SemiBold().FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                    t.Span(a.Metodo?.Nombre ?? "—").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                    if (!string.IsNullOrWhiteSpace(a.LimiteDeteccion))
                        t.Span($" — Límite de detección del método: {a.LimiteDeteccion}.").FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                });
            }

            if (!string.IsNullOrWhiteSpace(a.ReferenciaNormativa))
                col.Item().Text(a.ReferenciaNormativa).FontSize(8.5f).FontColor(Colors.Grey.Darken2);

            if (!string.IsNullOrWhiteSpace(a.CriterioAceptacion))
            {
                col.Item().Text(t =>
                {
                    t.Span("Criterio de aceptación: ").SemiBold().FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                    t.Span(a.CriterioAceptacion).FontSize(8.5f).FontColor(Colors.Grey.Darken2);
                });
            }

            // Si no se cargó ningún dato normativo detallado, al menos se muestra la referencia
            // simple (rango/valor esperado) que ya tenía el sistema, para no perder información.
            if (a.Metodo is null && string.IsNullOrWhiteSpace(a.ReferenciaNormativa) && string.IsNullOrWhiteSpace(a.CriterioAceptacion) && referenciaSimple is not null)
                col.Item().Text(referenciaSimple).FontSize(8.5f).FontColor(Colors.Grey.Darken2);
        });
    }
}
