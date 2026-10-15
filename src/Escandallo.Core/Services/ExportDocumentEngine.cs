using System.Text;
using Escandallo.Core.Contracts;

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
        // Generación de sandbox seguro sin resolución de red / SSRF
        var sb = new StringBuilder();
        sb.AppendLine("%PDF-1.4");
        sb.AppendLine($"% Report for center {centerId}");
        foreach (var item in escandallos)
        {
            sb.AppendLine($"% Service: {SanitizeCell(item.ServiceName)} - Cost: {item.TotalEscandalloCost} EUR");
        }
        sb.AppendLine("%%EOF");

        return Encoding.UTF8.GetBytes(sb.ToString());
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
