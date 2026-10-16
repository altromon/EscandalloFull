using System.Text;
using Escandallo.Core.Contracts;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Escandallo.Core.Services;

/// <summary>
/// Motor documental de exportación y sanitización anti-formula injection (FR-EXPORT-EXEC-001, SEC-REQ-EXPORT-SANITIZATION-001).
/// </summary>
public class ExportDocumentEngine : IExportDocumentEngine
{
    private static readonly char[] DangerousPrefixes = { '=', '+', '-', '@', '\t', '\r' };

    public string SanitizeCell(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        if (DangerousPrefixes.Contains(input[0]))
            return "'" + input;

        return input;
    }

    public byte[] ExportPdf(string centerId, IEnumerable<CompleteEscandallo> escandallos)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1.5f, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header()
                    .Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("INFORME DE ESCANDALLOS Y RENTABILIDAD").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                            col.Item().Text($"Centro: {SanitizeCell(centerId)}").FontSize(11).FontColor(Colors.Grey.Darken2);
                        });
                        row.ConstantItem(120).AlignRight().Text($"{DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                    });

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                            columns.RelativeColumn(1.5f);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(CellStyle).Text("Tratamiento").Bold();
                            header.Cell().Element(CellStyle).Text("Tiempo").Bold();
                            header.Cell().Element(CellStyle).Text("Coste").Bold();
                            header.Cell().Element(CellStyle).Text("PVP Sug.").Bold();
                            header.Cell().Element(CellStyle).Text("PVR").Bold();
                            header.Cell().Element(CellStyle).Text("Margen").Bold();
                            header.Cell().Element(CellStyle).Text("Semáforo").Bold();

                            static IContainer CellStyle(IContainer container) =>
                                container.Background(Colors.Grey.Lighten3).Padding(5);
                        });

                        foreach (var item in escandallos)
                        {
                            string semaforo = item.TrafficLightColor.ToString();
                            string colorHex = item.TrafficLightColor == TrafficLightColor.Verde ? "#16a34a" :
                                              item.TrafficLightColor == TrafficLightColor.Rojo ? "#dc2626" : "#ca8a04";

                            table.Cell().Element(ContentStyle).Text(SanitizeCell(item.ServiceName)).Bold();
                            table.Cell().Element(ContentStyle).Text($"{item.DurationMinutes} min");
                            table.Cell().Element(ContentStyle).Text($"{item.TotalEscandalloCost:F2} €");
                            table.Cell().Element(ContentStyle).Text($"{item.SuggestedPvp:F2} €");
                            table.Cell().Element(ContentStyle).Text($"{item.AppliedPvr:F2} €");
                            table.Cell().Element(ContentStyle).Text($"{item.NetMarginPercentage:F1}%");
                            table.Cell().Element(ContentStyle).Text(semaforo).FontColor(colorHex).Bold();

                            static IContainer ContentStyle(IContainer container) =>
                                container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5);
                        }
                    });

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Página ");
                        x.CurrentPageNumber();
                        x.Span(" de ");
                        x.TotalPages();
                    });
            });
        });

        return doc.GeneratePdf();
    }

    public byte[] ExportXlsx(string centerId, IEnumerable<CompleteEscandallo> escandallos)
    {
        var sb = new StringBuilder();
        sb.AppendLine("ID,ServiceName,TotalCost,SuggestedPvp,AppliedPvr");
        foreach (var item in escandallos)
        {
            sb.AppendLine($"{SanitizeCell(item.Id)},{SanitizeCell(item.ServiceName)},{item.TotalEscandalloCost},{item.SuggestedPvp},{item.AppliedPvr}");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }
}
