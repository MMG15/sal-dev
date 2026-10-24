using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Ssal.Domain;

namespace Ssal.Infrastructure;

public static class PresupuestoPdfBuilder
{
    public static byte[] Generar(Presupuesto presupuesto, IReadOnlyList<FirmaDocumento> firmasCicloActual)
    {
        var consulta = presupuesto.Consulta;
        var cliente = consulta.Cliente;
        var empresa = consulta.Empresa ?? cliente?.Empresa;

        string nombreDestinatario = cliente is not null
            ? $"{cliente.Nombre} {cliente.Apellido}"
            : empresa?.RazonSocial ?? "—";

        var ahora = DateTime.Now;
        var responsable = $"{presupuesto.CreadoPorUsuario.Nombre} {presupuesto.CreadoPorUsuario.Apellido}";
        var firmantes = PdfEstilo.ArmarFirmantes(firmasCicloActual);

        var items = presupuesto.Items.OrderBy(i => i.Analisis.Area).ThenBy(i => i.Analisis.Nombre).ToList();
        var paquetes = presupuesto.Paquetes.OrderBy(p => p.Paquete.Area).ThenBy(p => p.Paquete.Nombre).ToList();
        var subtotal = items.Sum(i => i.PrecioUsdSnapshot) + paquetes.Sum(p => p.PrecioUsdSnapshot);

        var documento = Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.DefaultTextStyle(t => t.FontSize(9.5f).FontColor(PdfEstilo.GrisTexto));

                page.Header().Element(c => PdfEstilo.Encabezado(c, "PRESUPUESTO DE ANÁLISIS", presupuesto.CodigoRpo ?? $"PRES-{presupuesto.IdPresupuesto}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Background(PdfEstilo.TealClaro).Padding(8).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Cliente / Solicitante: ").SemiBold(); t.Span(nombreDestinatario); });
                            c.Item().Text(t => { t.Span("Consulta: ").SemiBold(); t.Span(consulta.Codigo ?? "—"); });
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Elaborado por: ").SemiBold(); t.Span(responsable); });
                            c.Item().Text(t => { t.Span("Fecha: ").SemiBold(); t.Span(presupuesto.Fecha.ToLocalTime().ToString("dd/MM/yyyy")); });
                        });
                    });

                    // ── Trazabilidad y seguridad ──
                    col.Item().Column(c =>
                    {
                        c.Item().Background(Colors.Grey.Lighten3).Padding(4).Text("ASPECTOS RELACIONADOS CON LA TRAZABILIDAD Y SEGURIDAD").Bold().FontSize(9).FontColor(PdfEstilo.VerdeOscuro);
                        c.Item().PaddingTop(3).Text("Las muestras enviadas al LCC deberán cumplir con los siguientes requerimientos:").FontSize(8.5f).Italic();
                        string[] puntos =
                        [
                            "Presentar las muestras con su identificación mediante rótulos legibles.",
                            "Análisis solicitado: informar o acordar con el personal técnico la norma, metodología requerida, técnica analítica solicitada y/o especificaciones de análisis requeridas.",
                            "Información toxicológica, en el caso que el tipo de muestra lo requiera.",
                            "Se recomienda adjuntar un detalle escrito del pedido solicitado con cada muestra enviada.",
                            "De no aportar el cliente alguno de estos requerimientos técnicos, deberá solicitar la asesoría técnica del personal del LCC para la aplicación de las normas legales nacionales vigentes para la categoría del producto, bibliografía internacional y/o recursos técnicos propios.",
                            "Salvo que se especifique lo contrario, el LCC no es responsable por el muestreo ni por el rotulado de las muestras recibidas para su análisis.",
                        ];
                        foreach (var p in puntos)
                            c.Item().PaddingLeft(6).PaddingTop(1).Text($"•  {p}").FontSize(8.5f);
                    });

                    // ── Detalles administrativos ──
                    col.Item().Column(c =>
                    {
                        c.Item().Background(Colors.Grey.Lighten3).Padding(4).Text("DETALLES ADMINISTRATIVOS DEL SERVICIO").Bold().FontSize(9).FontColor(PdfEstilo.VerdeOscuro);

                        c.Item().PaddingTop(4).Text("Precios del Servicio:").SemiBold().FontSize(8.5f);
                        c.Item().Text("- Los precios son unitarios por muestra.").FontSize(8.5f);
                        c.Item().Text("- Los precios son por muestras puestas en el LCC.").FontSize(8.5f);

                        c.Item().PaddingTop(4).Text("Los cambios respecto del pedido de servicio inicial, ya realizado, que se requieran con posterioridad a la SS (Solicitud de Servicio), deben realizarse por escrito (nota firmada o email), únicamente.").FontSize(8.5f);

                        c.Item().PaddingTop(4).Text(t => { t.Span("Plazo estimado de ejecución del Servicio: ").SemiBold().FontSize(8.5f); t.Span("Mic. 10 días hábiles, FQ 10 días hábiles, se cuentan desde la confirmación del trabajo con el pago.").FontSize(8.5f); });

                        c.Item().PaddingTop(4).Text(t => { t.Span("Horario de Recepción de muestras y atención administrativa: ").SemiBold().FontSize(8.5f); t.Span("Lunes a Viernes de 8:00hs. a 14:00hs.").FontSize(8.5f); });

                        c.Item().PaddingTop(4).Text("Forma de pago:").SemiBold().FontSize(8.5f);
                        c.Item().Text("EFECTIVO: Cancelar al ingresar la muestra. CHEQUE al día de firma propia.").FontSize(8.5f);
                        c.Item().Text("DEPÓSITO O TRANSFERENCIA BANCARIA (informar previamente vía e-mail o telefónicamente): Bco. Santander Río de San Juan; CBU: 07201796-20000001793772, Cta. Cte. de Fundación M. F. Manfredi Nº: 179-000179377.").FontSize(8.5f);

                        c.Item().PaddingTop(4).Text(t => { t.Span("Observaciones: ").SemiBold().FontSize(8.5f); t.Span("El envío de informes en formato digital por correo solo se realiza con los trabajos pagados por una de las formas propuestas. La presente no incluye el envío de informes por correo postal.").FontSize(8.5f); });
                    });

                    // ── Tabla de paquetes y análisis ──
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(34);
                            c.RelativeColumn(4);
                            c.RelativeColumn(1.1f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(PdfEstilo.CeldaEncabezadoTabla).Text("Área");
                            header.Cell().Element(PdfEstilo.CeldaEncabezadoTabla).Text("Análisis / Paquete");
                            header.Cell().Element(PdfEstilo.CeldaEncabezadoTabla).AlignRight().Text("Precio USD");
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
                                var detalle = new List<string>();
                                if (!string.IsNullOrWhiteSpace(p.Paquete.Norma)) detalle.Add($"Norma: {p.Paquete.Norma}");
                                if (!string.IsNullOrWhiteSpace(p.Paquete.CantidadMuestra)) detalle.Add($"Muestra: {p.Paquete.CantidadMuestra}");
                                detalle.Add($"Incluye: {string.Join(", ", p.Paquete.Items.Select(i => i.Analisis.Nombre))}");
                                c.Item().Text(string.Join(" · ", detalle)).FontSize(7.8f).FontColor(Colors.Grey.Darken1);
                            });
                            table.Cell().Element(PdfEstilo.CeldaTabla).AlignRight().AlignMiddle().Text($"USD {p.PrecioUsdSnapshot:0.00}").FontSize(9);
                        }

                        foreach (var i in items)
                        {
                            table.Cell().Element(PdfEstilo.CeldaTabla).Text(i.Analisis.Area);
                            table.Cell().Element(PdfEstilo.CeldaTabla).Column(c =>
                            {
                                c.Item().Text(i.Analisis.Nombre).SemiBold().FontSize(9);
                                if (!string.IsNullOrWhiteSpace(i.Analisis.Metodo))
                                    c.Item().Text(i.Analisis.Metodo).FontSize(7.8f).FontColor(Colors.Grey.Darken1);
                            });
                            table.Cell().Element(PdfEstilo.CeldaTabla).AlignRight().AlignMiddle().Text($"USD {i.PrecioUsdSnapshot:0.00}").FontSize(9);
                        }
                    });

                    // ── Totales ──
                    var mostrarUsd = presupuesto.MostrarMoneda != "ars";
                    var mostrarArs = presupuesto.MostrarMoneda != "usd";
                    col.Item().Background(PdfEstilo.TealClaro).Padding(10).AlignRight().Column(c =>
                    {
                        var tipo = presupuesto.TipoAsesoramiento switch
                        {
                            "consulta" => "Solo consulta",
                            "terreno" => "Visita de terreno",
                            "proceso_completo" => "Proceso completo (consulta + muestreo + entrega)",
                            _ => presupuesto.TipoAsesoramiento ?? "sin especificar"
                        };

                        if (mostrarUsd)
                        {
                            c.Item().Text(t => { t.Span("Subtotal análisis: ").SemiBold(); t.Span($"USD {subtotal:0.00}"); });
                            if (presupuesto.AdicionalAsesoramiento > 0)
                                c.Item().Text(t => { t.Span($"Asesoramiento técnico ({tipo}): ").SemiBold(); t.Span($"USD {presupuesto.AdicionalAsesoramiento:0.00}"); });
                            c.Item().PaddingTop(3).Text(t => { t.Span("TOTAL USD: ").Bold().FontSize(12).FontColor(PdfEstilo.VerdeOscuro); t.Span($"USD {presupuesto.ImporteDolares:0.00}").Bold().FontSize(12).FontColor(PdfEstilo.VerdeOscuro); });
                        }
                        else
                        {
                            c.Item().Text(t => { t.Span("Subtotal análisis: ").SemiBold(); t.Span($"$ {subtotal * presupuesto.Cotizacion:0.00}"); });
                            if (presupuesto.AdicionalAsesoramiento > 0)
                                c.Item().Text(t => { t.Span($"Asesoramiento técnico ({tipo}): ").SemiBold(); t.Span($"$ {presupuesto.AdicionalAsesoramiento * presupuesto.Cotizacion:0.00}"); });
                        }

                        if (mostrarUsd && mostrarArs)
                            c.Item().Text(t => { t.Span("Cotización USD/ARS: ").SemiBold(); t.Span($"$ {presupuesto.Cotizacion:0}"); });

                        if (mostrarArs)
                            c.Item().PaddingTop(mostrarUsd ? 0 : 3).Text(t => { t.Span("TOTAL ARS: ").Bold().FontSize(12).FontColor(PdfEstilo.VerdeOscuro); t.Span($"$ {presupuesto.ImportePesos:0.00}").Bold().FontSize(12).FontColor(PdfEstilo.VerdeOscuro); });
                    });

                    // ── Vigencia y condiciones ──
                    col.Item().Column(c =>
                    {
                        c.Item().Text(t =>
                        {
                            t.Span("Validez de la oferta: ").SemiBold().FontSize(8.5f);
                            t.Span($"hasta {presupuesto.Fecha.ToLocalTime().AddDays(30):dd/MM/yyyy}.").FontSize(8.5f);
                        });
                        c.Item().PaddingTop(2).Text("Vencido este plazo, el laboratorio se reserva el derecho de modificar los precios sin previo aviso. En caso de aceptación de este presupuesto, el nuevo cliente deberá aportar los siguientes datos: razón social, N° de CUIT, domicilio, código postal, N° de teléfono, mail, nombre del contacto.")
                            .FontSize(8f).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingTop(2).Text("La factura se emitirá en pesos, tomando la cotización del dólar tipo vendedor del BNA (Banco de la Nación Argentina) vigente al día hábil inmediato anterior al de la fecha de solicitud de la factura. La misma tendrá vigencia de siete (7) días corridos respecto de la fecha de factura, para cancelarla. El ingreso del pago es requisito necesario para la entrega de informes.")
                            .FontSize(8f).FontColor(Colors.Grey.Darken1);
                        c.Item().PaddingTop(2).Text("Las muestras facturadas mantendrán el precio en la medida que ingresen dentro de los 30 días hábiles posteriores a su pago; caso contrario, se enviará otra factura de actualización de valores. En caso de sufrir imprevistos durante la ejecución del servicio, se comunicará al cliente a fin de revisar el plan de trabajo cotizado.")
                            .FontSize(8f).FontColor(Colors.Grey.Darken1);
                    });

                    // ── Firmas digitales (reemplaza la línea en blanco de firma del cliente) ──
                    col.Item().PaddingTop(6).Element(c => PdfEstilo.BloqueFirmas(c, firmantes, ahora));
                });

                page.Footer().Element(c => PdfEstilo.Pie(c, responsable, ahora));
            });
        });

        return documento.GeneratePdf();
    }
}
