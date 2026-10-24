using System.Reflection;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Ssal.Infrastructure;

// Encabezado, pie de página y paleta compartidos por los 3 PDFs del sistema (Presupuesto, SSE,
// Informe de resultados), para que se vean como el mismo documento institucional.
public static class PdfEstilo
{
    public const string VerdeOscuro = "#0b5f67";
    public const string Teal = "#0f7c86";
    public const string TealClaro = "#e3f4f5";
    public const string GrisTexto = "#444444";
    public const string GrisBorde = "#d1d5db";

    private static byte[]? _logoCache;

    // El logo se embebe en la DLL (ver Ssal.Infrastructure.csproj) para no depender de en qué
    // carpeta esté corriendo el servidor.
    public static byte[] Logo()
    {
        if (_logoCache is not null) return _logoCache;
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream("Ssal.Infrastructure.Assets.logo.png")
            ?? throw new InvalidOperationException("No se encontró el logo embebido (Assets/logo.png).");
        using var mem = new MemoryStream();
        stream.CopyTo(mem);
        _logoCache = mem.ToArray();
        return _logoCache;
    }

    public static void Encabezado(IContainer container, string tituloDocumento, string numero)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.ConstantItem(220).Image(Logo()).FitWidth();
                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text(tituloDocumento).FontSize(13).Bold().FontColor(VerdeOscuro);
                    c.Item().Text($"N° {numero}").FontSize(11).SemiBold().FontColor(Teal);
                    c.Item().PaddingTop(2).Text("Cel./WhatsApp: 264-4839453").FontSize(8.5f).FontColor(GrisTexto);
                });
            });
            col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(Teal);
        });
    }

    public static void Pie(IContainer container, string responsable, DateTime fechaEmision)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(0.5f).LineColor(GrisBorde);
            col.Item().PaddingTop(4).Row(row =>
            {
                row.RelativeItem().Text($"Responsable: {responsable} · Emitido el {fechaEmision:dd/MM/yyyy HH:mm}")
                    .FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                row.AutoItem().Text(t =>
                {
                    t.Span("Página ").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                    t.CurrentPageNumber().FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                    t.Span(" de ").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                    t.TotalPages().FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                });
            });
        });
    }

    public static IContainer CeldaEncabezadoTabla(IContainer c) => c
        .DefaultTextStyle(t => t.SemiBold().FontColor(Colors.White).FontSize(9))
        .Background(VerdeOscuro).Padding(6);

    public static IContainer CeldaTabla(IContainer c) => c
        .BorderBottom(0.5f).BorderColor(GrisBorde).Padding(6);

    // Bloque de firmas digitales reales (reemplaza la línea en blanco "Persona y cargo (sello)"
    // de los documentos en papel): solo lista a quienes ya firmaron, no a los que faltan.
    public static void BloqueFirmas(IContainer container, List<(string Nombre, string Cargo)> firmantes, DateTime fechaEmision)
    {
        if (firmantes.Count == 0) return;

        container.Border(1).BorderColor(GrisBorde).Padding(10).Column(c =>
        {
            c.Item().Text("Este documento fue validado digitalmente dentro del sistema por los siguientes responsables:")
                .FontSize(8.5f).Italic();
            c.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(cc =>
                {
                    cc.RelativeColumn(2.2f);
                    cc.RelativeColumn(2);
                    cc.RelativeColumn(1);
                });
                foreach (var f in firmantes)
                {
                    table.Cell().Element(Celda).Text(f.Nombre).SemiBold().FontSize(9);
                    table.Cell().Element(Celda).Text(f.Cargo).FontSize(8.5f).FontColor(Colors.Grey.Darken1);
                    table.Cell().Element(Celda).AlignRight().Text(fechaEmision.ToString("dd/MM/yyyy")).FontSize(8.5f);

                    static IContainer Celda(IContainer cc2) => cc2.PaddingVertical(3).BorderBottom(0.5f).BorderColor(GrisBorde);
                }
            });
        });
    }

    // A partir del historial de firmas del ciclo vigente, arma la lista (Nombre, Cargo) lista
    // para mostrar: un responsable por área que ya firmó, y la aprobación final si ya se dio.
    public static List<(string Nombre, string Cargo)> ArmarFirmantes(IEnumerable<Ssal.Domain.FirmaDocumento> eventosCicloActual)
    {
        var resultado = new List<(string, string)>();
        foreach (var e in eventosCicloActual.OrderBy(e => e.IdFirma))
        {
            var nombre = $"{e.Firmante.Nombre} {e.Firmante.Apellido}";
            if (e.TipoFirma == "responsable" && e.Area is not null)
                resultado.Add((nombre, $"Responsable de área {e.Area}"));
            else if (e.TipoFirma == "aprobacion")
                resultado.Add((nombre, "Aprobación final — Director Técnico"));
        }
        return resultado;
    }
}
