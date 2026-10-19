using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ssal.Domain;

namespace Ssal.Infrastructure;

public static class SsePdfBuilder
{
    public static byte[] Generar(Sse sse)
    {
        var cliente = sse.Cliente;
        var empresa = sse.Empresa ?? cliente?.Empresa;
        var etiquetaArea = sse.Area == "AMBAS" ? "MIC y FQ" : sse.Area;
        var analisis = sse.Presupuesto?.Items
            .OrderBy(i => i.Analisis.Area)
            .ThenBy(i => i.Analisis.Nombre)
            .ToList() ?? [];
        var rotulo = sse.RotuloInterno;

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
                    col.Item().PaddingTop(2).Text("SOLICITUD DE SERVICIO (SSE)").FontSize(11).SemiBold();
                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("SSE: ").SemiBold(); t.Span(sse.Codigo ?? "—"); });
                            c.Item().Text(t => { t.Span("Presupuesto: ").SemiBold(); t.Span(sse.CodigoRpo ?? "Sin presupuesto vinculado"); });
                            c.Item().Text(t => { t.Span("Área: ").SemiBold(); t.Span(etiquetaArea); });
                            c.Item().Text(t => { t.Span("Fecha: ").SemiBold(); t.Span(sse.Fecha.ToLocalTime().ToString("dd/MM/yyyy")); });
                        });
                        row.RelativeItem().Column(c =>
                        {
                            if (cliente is null && empresa is null)
                            {
                                c.Item().Text(t => { t.Span("Cliente: ").SemiBold(); t.Span("Sin cliente ni empresa asignados"); });
                            }
                            else
                            {
                                if (cliente is not null)
                                    c.Item().Text(t => { t.Span("Cliente: ").SemiBold(); t.Span($"{cliente.Nombre} {cliente.Apellido}"); });
                                if (empresa is not null)
                                    c.Item().Text(t => { t.Span("Empresa: ").SemiBold(); t.Span(empresa.RazonSocial); });
                                if (!string.IsNullOrWhiteSpace(cliente?.Email))
                                    c.Item().Text(t => { t.Span("Email: ").SemiBold(); t.Span(cliente.Email); });
                                if (!string.IsNullOrWhiteSpace(cliente?.Telefono))
                                    c.Item().Text(t => { t.Span("Teléfono: ").SemiBold(); t.Span(cliente.Telefono); });
                            }
                        });
                    });

                    if (rotulo is not null)
                    {
                        col.Item().PaddingTop(4).Background(Colors.Grey.Lighten3).Padding(8).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Rótulo interno: ").SemiBold(); t.Span(rotulo.NumeroUnico).FontSize(12).Bold(); });
                            row.RelativeItem().AlignRight().Text(t => { t.Span("Estado: ").SemiBold(); t.Span(rotulo.Estado); });
                        });

                        if (rotulo.Muestra is not null)
                        {
                            col.Item().Text(t =>
                            {
                                t.Span("Muestra: ").SemiBold();
                                t.Span($"{rotulo.Muestra.Tipo ?? "—"} · recibida {rotulo.Muestra.FechaRecepcion.ToLocalTime():dd/MM/yyyy} · estado {rotulo.Muestra.Estado}");
                            });
                        }
                    }

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(40);
                            c.RelativeColumn(3);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CeldaEncabezado).Text("Área");
                            header.Cell().Element(CeldaEncabezado).Text("Análisis solicitado");

                            static IContainer CeldaEncabezado(IContainer c) => c
                                .DefaultTextStyle(t => t.SemiBold().FontColor(Colors.White))
                                .Background(Colors.Grey.Darken2)
                                .Padding(5);
                        });

                        foreach (var item in analisis)
                        {
                            table.Cell().Element(Celda).Text(item.Analisis.Area);
                            table.Cell().Element(Celda).Text(item.Analisis.Nombre);

                            static IContainer Celda(IContainer c) => c
                                .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5);
                        }
                    });

                    col.Item().AlignRight().PaddingTop(4).Column(c =>
                    {
                        c.Item().Text(t => { t.Span("Forma de pago: ").SemiBold(); t.Span(sse.FormaPago ?? "—"); });
                        c.Item().Text(t => { t.Span("TOTAL USD: ").Bold().FontSize(11); t.Span($"USD {sse.ImporteDolares:0.00}").Bold().FontSize(11); });
                        c.Item().Text(t => { t.Span("TOTAL ARS: ").Bold().FontSize(11); t.Span($"$ {sse.ImportePesos:0.00}").Bold().FontSize(11); });
                    });

                    col.Item().PaddingTop(8).Text(
                        "Este documento acredita la solicitud de servicio registrada en el laboratorio. Consérvelo como constancia."
                    ).FontSize(8.5f).Italic().FontColor(Colors.Grey.Darken1);
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text($"Registrada por {sse.CreadoPorUsuario.Nombre} {sse.CreadoPorUsuario.Apellido} · {sse.Fecha.ToLocalTime():dd/MM/yyyy HH:mm}")
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
