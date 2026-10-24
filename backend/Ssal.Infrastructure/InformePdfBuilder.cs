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

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(34);
                            c.RelativeColumn(3);
                            c.RelativeColumn(1.4f);
                            c.RelativeColumn(1);
                            c.RelativeColumn(1.8f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(PdfEstilo.CeldaEncabezadoTabla).Text("Área");
                            header.Cell().Element(PdfEstilo.CeldaEncabezadoTabla).Text("Análisis");
                            header.Cell().Element(PdfEstilo.CeldaEncabezadoTabla).Text("Resultado");
                            header.Cell().Element(PdfEstilo.CeldaEncabezadoTabla).Text("Unidad");
                            header.Cell().Element(PdfEstilo.CeldaEncabezadoTabla).Text("Referencia");
                        });

                        foreach (var r in resultados)
                        {
                            var referencia = r.Analisis.ValorEsperado is not null
                                ? $"Esperado: {r.Analisis.ValorEsperado}"
                                : (r.Analisis.RangoMin is not null || r.Analisis.RangoMax is not null)
                                    ? $"{r.Analisis.RangoMin?.ToString() ?? "—"} a {r.Analisis.RangoMax?.ToString() ?? "—"}"
                                    : "—";

                            table.Cell().Element(PdfEstilo.CeldaTabla).Text(r.Analisis.Area);
                            table.Cell().Element(PdfEstilo.CeldaTabla).Text(r.Analisis.Nombre).FontSize(9);
                            table.Cell().Element(PdfEstilo.CeldaTabla).Text(r.Valor ?? "—").FontSize(9);
                            table.Cell().Element(PdfEstilo.CeldaTabla).Text(r.Analisis.Unidad ?? "—").FontSize(9);
                            table.Cell().Element(PdfEstilo.CeldaTabla).Text(referencia).FontSize(8.5f);
                        }
                    });

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
}
