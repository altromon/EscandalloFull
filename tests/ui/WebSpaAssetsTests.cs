using Xunit;

namespace Escandallo.Tests.UI;

public class WebSpaAssetsTests
{
    [Fact]
    public void WebSpa_AssetsAndRazorPage_ExistAndContainRequiredComponents()
    {
        string projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../src/Escandallo.Web"));
        string htmlPath = Path.Combine(projectDir, "wwwroot", "index.html");
        string cssPath = Path.Combine(projectDir, "wwwroot", "css", "app.css");
        string jsPath = Path.Combine(projectDir, "wwwroot", "js", "app.js");
        string razorPagePath = Path.Combine(projectDir, "Pages", "Index.cshtml");

        Assert.True(File.Exists(htmlPath), "index.html must exist in wwwroot");
        Assert.True(File.Exists(cssPath), "app.css must exist in wwwroot/css");
        Assert.True(File.Exists(jsPath), "app.js must exist in wwwroot/js");
        Assert.True(File.Exists(razorPagePath), "Index.cshtml must exist in Pages");

        string html = File.ReadAllText(htmlPath);
        Assert.Contains("activeCenterBadge", html);
        Assert.Contains("stateLoading", html);
        Assert.Contains("stateEmpty", html);
        Assert.Contains("stateError", html);
        Assert.Contains("stateNominal", html);
        Assert.Contains("quotaBadge", html);
        Assert.Contains("upgradeModal", html);
    }
}
