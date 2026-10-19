using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ssal.Domain;

namespace Ssal.Infrastructure;

public static class PresupuestoPdfBuilder
{
    public static byte[] Generar(Presupuesto presupuesto)
    {
        var consulta = presupuesto.Consulta;
        var cliente = consulta.Cliente;
        var empresa = consulta.Empresa ?? cliente?.Empresa;

        string nombreDestinatario = cliente is not null
            ? $"{cliente.Nombre} {cliente.Apellido}"
            : empresa?.RazonSocial ?? "—";

        var items = presupuesto.Items
            .OrderBy(i => i.Analisis.Area)
            .ThenBy(i => i.Analisis.Nombre)
            .ToList();

        var subtotalAnalisis = items.Sum(i => i.PrecioUsdSnapshot);

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
                    col.Item().PaddingTop(2).Text("PRESUPUESTO DE ANÁLISIS").FontSize(11).SemiBold();
                    col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Presupuesto: ").SemiBold(); t.Span(presupuesto.CodigoRpo ?? "—"); });
                            c.Item().Text(t => { t.Span("Consulta: ").SemiBold(); t.Span(consulta.Codigo ?? "—"); });
                            c.Item().Text(t => { t.Span("Fecha: ").SemiBold(); t.Span(presupuesto.Fecha.ToLocalTime().ToString("dd/MM/yyyy")); });
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Destinatario: ").SemiBold(); t.Span(nombreDestinatario); });
                            if (empresa is not null)
                                c.Item().Text(t => { t.Span("Empresa: ").SemiBold(); t.Span(empresa.RazonSocial); });
                            if (!string.IsNullOrWhiteSpace(cliente?.Email))
                                c.Item().Text(t => { t.Span("Email: ").SemiBold(); t.Span(cliente.Email); });
                        });
                    });

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(40);   // Área
                            c.RelativeColumn(3);    // Análisis
                            c.RelativeColumn(1.2f); // Precio
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CeldaEncabezado).Text("Área");
                            header.Cell().Element(CeldaEncabezado).Text("Análisis");
                            header.Cell().Element(CeldaEncabezado).AlignRight().Text("Precio USD");

                            static IContainer CeldaEncabezado(IContainer c) => c
                                .DefaultTextStyle(t => t.SemiBold().FontColor(Colors.White))
                                .Background(Colors.Grey.Darken2)
                                .Padding(5);
                        });

                        foreach (var i in items)
                        {
                            table.Cell().Element(Celda).Text(i.Analisis.Area);
                            table.Cell().Element(Celda).Text(i.Analisis.Nombre);
                            table.Cell().Element(Celda).AlignRight().Text($"USD {i.PrecioUsdSnapshot:0.00}");

                            static IContainer Celda(IContainer c) => c
                                .BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5);
                        }
                    });

                    col.Item().AlignRight().PaddingTop(4).Column(c =>
                    {
                        c.Item().Text(t => { t.Span("Subtotal análisis: ").SemiBold(); t.Span($"USD {subtotalAnalisis:0.00}"); });
                        if (presupuesto.AdicionalAsesoramiento > 0)
                        {
                            var tipo = presupuesto.TipoAsesoramiento switch
                            {
                                "consulta" => "Solo consulta",
                                "terreno" => "Visita de terreno",
                                "proceso_completo" => "Proceso completo (consulta + muestreo + entrega)",
                                _ => presupuesto.TipoAsesoramiento ?? "sin especificar"
                            };
                            c.Item().Text(t => { t.Span($"Asesoramiento técnico ({tipo}): ").SemiBold(); t.Span($"USD {presupuesto.AdicionalAsesoramiento:0.00}"); });
                        }
                        c.Item().PaddingTop(4).Text(t => { t.Span("TOTAL USD: ").Bold().FontSize(11); t.Span($"USD {presupuesto.ImporteDolares:0.00}").Bold().FontSize(11); });
                        c.Item().Text(t => { t.Span("Cotización USD/ARS: ").SemiBold(); t.Span($"$ {presupuesto.Cotizacion:0}"); });
                        c.Item().Text(t => { t.Span("TOTAL ARS: ").Bold().FontSize(11); t.Span($"$ {presupuesto.ImportePesos:0.00}").Bold().FontSize(11); });
                    });

                    col.Item().PaddingTop(8).Text(
                        "Presupuesto válido por 30 días desde la fecha de emisión. Ante cualquier consulta, no dude en comunicarse con nosotros."
                    ).FontSize(8.5f).Italic().FontColor(Colors.Grey.Darken1);
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten1);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text($"Generado por {presupuesto.CreadoPorUsuario.Nombre} {presupuesto.CreadoPorUsuario.Apellido} · {presupuesto.Fecha.ToLocalTime():dd/MM/yyyy HH:mm}")
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
