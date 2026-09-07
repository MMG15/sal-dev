using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ssal.Api.Models;

namespace Ssal.Api.Services;

public static class InformePdfBuilder
{
    public static byte[] Generar(Informe informe)
    {
        var muestra = informe.Muestra;
        var sse = muestra.RotuloInterno.Sse;
        var cliente = sse.Cliente;
        var empresa = cliente?.Empresa;

        string nombreDestinatario = cliente is not null
            ? $"{cliente.Nombre} {cliente.Apellido}"
            : empresa?.RazonSocial ?? "—";

        var resultados = muestra.Resultados
            .OrderBy(r => r.Analisis.Area)
            .ThenBy(r => r.Analisis.Nombre)
            .ToList();

        var documento = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(t => t.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Text("LABORATORIO CONTROL DE CALIDAD \"DR. ALBERTO GRAFFIGNA\" - UCCUYO")
                        .FontSize(13).Bold();
                    col.Item().PaddingTop(2).Text("INFORME DE RESULTADOS DE ANÁLISIS").FontSize(11).SemiBold();
                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Informe: ").SemiBold(); t.Span(informe.Codigo); });
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
                            c.ConstantColumn(35);  // Área
                            c.RelativeColumn(3);   // Análisis
                            c.RelativeColumn(1.4f); // Resultado
                            c.RelativeColumn(1);   // Unidad
                            c.RelativeColumn(1.8f); // Referencia
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CeldaEncabezado).Text("Área");
                            header.Cell().Element(CeldaEncabezado).Text("Análisis");
                            header.Cell().Element(CeldaEncabezado).Text("Resultado");
                            header.Cell().Element(CeldaEncabezado).Text("Unidad");
                            header.Cell().Element(CeldaEncabezado).Text("Referencia");

                            static IContainer CeldaEncabezado(IContainer c) => c
                                .DefaultTextStyle(t => t.SemiBold().FontColor(Colors.White))
                                .Background(Colors.Grey.Darken2)
                                .Padding(5);
                        });

                        foreach (var r in resultados)
                        {
                            var referencia = r.Analisis.ValorEsperado is not null
                                ? $"Esperado: {r.Analisis.ValorEsperado}"
                                : (r.Analisis.RangoMin is not null || r.Analisis.RangoMax is not null)
                                    ? $"{r.Analisis.RangoMin?.ToString() ?? "—"} a {r.Analisis.RangoMax?.ToString() ?? "—"}"
                                    : "—";

                            table.Cell().Element(Celda).Text(r.Analisis.Area);
                            table.Cell().Element(Celda).Text(r.Analisis.Nombre);
                            table.Cell().Element(Celda).Text(r.Valor ?? "—");
                            table.Cell().Element(Celda).Text(r.Analisis.Unidad ?? "—");
                            table.Cell().Element(Celda).Text(referencia);

                            static IContainer Celda(IContainer c) => c
                                .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5);
                        }
                    });

                    col.Item().PaddingTop(6).Text(
                        $"Todos los análisis listados fueron validados por el Responsable de Área correspondiente antes de la emisión de este informe."
                    ).FontSize(8.5f).Italic().FontColor(Colors.Grey.Darken1);
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text($"Generado por {informe.GeneradoPorUsuario.Nombre} {informe.GeneradoPorUsuario.Apellido} · {informe.FechaGeneracion:dd/MM/yyyy HH:mm}")
                            .FontSize(8).FontColor(Colors.Grey.Darken1);
                        row.AutoItem().Text(t =>
                        {
                            t.Span("Página ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            t.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                            t.Span(" de ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            t.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            });
        });

        return documento.GeneratePdf();
    }
}
