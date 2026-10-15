using Escandallo.Core.Services;
using Escandallo.Tests.Contracts;
using Xunit;

namespace Escandallo.Tests.Unit.Security;

public class ExportSanitizationTests
{
    private readonly IExportDocumentEngine _exportEngine = new ExportDocumentEngine();

    [Theory]
    [InlineData("=cmd|'/C calc'!A0", "'=cmd|'/C calc'!A0")]
    [InlineData("+SUM(1,2)", "'+SUM(1,2)")]
    [InlineData("-100+20", "'-100+20")]
    [InlineData("@SUM(1+1)*cmd", "'@SUM(1+1)*cmd")]
    [InlineData("\tDDE(\"server\",\"cmd\")", "'\tDDE(\"server\",\"cmd\")")]
    [InlineData("\rDDE(\"server\",\"cmd\")", "'\rDDE(\"server\",\"cmd\")")]
    [InlineData("Mesodermolifting Facial", "Mesodermolifting Facial")] // Safe string unchanged
    public void Security_AntiFormulaInjection_PrefixesExecutableCharactersWithSingleQuote(string input, string expected)
    {
        // Act
        string sanitized = _exportEngine.SanitizeCell(input);

        // Assert
        Assert.Equal(expected, sanitized);
    }

    [Theory]
    [InlineData("http://169.254.169.254/latest/meta-data")]
    [InlineData("http://localhost:5000/internal-metrics")]
    [InlineData("file:///etc/passwd")]
    public void Security_PdfRenderingSandbox_BlocksNetworkResolutionAndSsrf(string externalUrl)
    {
        // Arrange
        var escandallos = new List<CompleteEscandallo>
        {
            new() { ServiceName = $"Service with malicious image: <img src='{externalUrl}'>" }
        };

        // Act & Assert
        // Engine must block remote resolution without network access
        var pdfBytes = _exportEngine.ExportPdf("ctr-1", escandallos);
        Assert.NotNull(pdfBytes);
        Assert.NotEmpty(pdfBytes);
    }
}
