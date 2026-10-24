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
        var paquetes = sse.Presupuesto?.Paquetes
            .OrderBy(p => p.Paquete.Area)
            .ThenBy(p => p.Paquete.Nombre)
            .ToList() ?? [];
        var rotulos = sse.RotulosInternos.OrderBy(r => r.NumeroUnico).ToList();
        var responsable = $"{sse.CreadoPorUsuario.Nombre} {sse.CreadoPorUsuario.Apellido}";
        var ahora = DateTime.Now;

        var documento = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(t => t.FontSize(9.5f).FontColor(PdfEstilo.GrisTexto));

                page.Header().Element(c => PdfEstilo.Encabezado(c, "SOLICITUD DE SERVICIO (SSE)", sse.Codigo ?? $"SSE-{sse.IdSse}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Background(PdfEstilo.TealClaro).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
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
                            }
                        });
                    });

                    foreach (var rotulo in rotulos)
                    {
                        col.Item().Background(Colors.Grey.Lighten3).Padding(8).Row(row =>
                        {
                            row.RelativeItem().Text(t => { t.Span("Rótulo interno: ").SemiBold(); t.Span(rotulo.NumeroUnico).FontSize(12).Bold().FontColor(PdfEstilo.VerdeOscuro); });
                            row.RelativeItem().AlignRight().Text(t => { t.Span("Estado: ").SemiBold(); t.Span(rotulo.Estado); });
                        });

                        if (rotulo.Muestra is not null)
                        {
                            col.Item().Text(t =>
                            {
                                t.Span("Muestra: ").SemiBold().FontSize(8.5f);
                                t.Span($"{rotulo.Muestra.Tipo ?? "—"} · recibida {rotulo.Muestra.FechaRecepcion.ToLocalTime():dd/MM/yyyy} · estado {rotulo.Muestra.Estado}").FontSize(8.5f);
                            });
                        }
                    }

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(34);
                            c.RelativeColumn(4);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(PdfEstilo.CeldaEncabezadoTabla).Text("Área");
                            header.Cell().Element(PdfEstilo.CeldaEncabezadoTabla).Text("Análisis / Paquete solicitado");
                        });

                        foreach (var p in paquetes)
                        {
                            table.Cell().Element(PdfEstilo.CeldaTabla).Text(p.Paquete.Area);
                            table.Cell().Element(PdfEstilo.CeldaTabla).Column(c =>
                            {
                                c.Item().Text(t =>
                                {
                                    t.Span(p.Paquete.Nombre).SemiBold().FontSize(9);
                                    t.Span("  (paquete)").FontSize(8).FontColor(PdfEstilo.Teal).Italic();
                                });
                                c.Item().Text($"Incluye: {string.Join(", ", p.Paquete.Items.Select(i => i.Analisis.Nombre))}")
                                    .FontSize(7.8f).FontColor(Colors.Grey.Darken1);
                            });
                        }

                        foreach (var item in analisis)
                        {
                            table.Cell().Element(PdfEstilo.CeldaTabla).Text(item.Analisis.Area);
                            table.Cell().Element(PdfEstilo.CeldaTabla).Text(item.Analisis.Nombre).FontSize(9);
                        }
                    });

                    col.Item().Background(PdfEstilo.TealClaro).Padding(10).AlignRight().Column(c =>
                    {
                        var mostrarUsd = sse.Presupuesto?.MostrarMoneda != "ars";
                        var mostrarArs = sse.Presupuesto?.MostrarMoneda != "usd";

                        c.Item().Text(t => { t.Span("Forma de pago: ").SemiBold(); t.Span(sse.FormaPago ?? "—"); });
                        if (mostrarUsd)
                            c.Item().PaddingTop(3).Text(t => { t.Span("TOTAL USD: ").Bold().FontSize(12).FontColor(PdfEstilo.VerdeOscuro); t.Span($"USD {sse.ImporteDolares:0.00}").Bold().FontSize(12).FontColor(PdfEstilo.VerdeOscuro); });
                        if (mostrarArs)
                            c.Item().PaddingTop(mostrarUsd ? 0 : 3).Text(t => { t.Span("TOTAL ARS: ").Bold().FontSize(12).FontColor(PdfEstilo.VerdeOscuro); t.Span($"$ {sse.ImportePesos:0.00}").Bold().FontSize(12).FontColor(PdfEstilo.VerdeOscuro); });
                    });

                    col.Item().Text(
                        "Este documento acredita la solicitud de servicio registrada en el laboratorio. Consérvelo como constancia."
                    ).FontSize(8.5f).Italic().FontColor(Colors.Grey.Darken1);
                });

                page.Footer().Element(c => PdfEstilo.Pie(c, responsable, ahora));
            });
        });

        return documento.GeneratePdf();
    }
}
