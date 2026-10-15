using Escandallo.Core.Services;
using Escandallo.Core.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddSingleton<IEscandalloCostEngine, EscandalloCostEngine>();
builder.Services.AddSingleton<IMarginCalculator, MarginCalculator>();
builder.Services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
builder.Services.AddSingleton<IEscandalloRepository, EscandalloRepository>();
builder.Services.AddSingleton<ICostConfigurationRepository, CostConfigurationRepository>();

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapRazorPages();
app.MapFallbackToPage("/Index");

var apiBaseUrl = app.Configuration["ApiBaseUrl"] ?? "http://localhost:5000";
app.Map("/api/{**path}", async (HttpContext ctx, IHttpClientFactory factory) =>
{
    await ProxyApiCall(ctx, factory.CreateClient(), apiBaseUrl);
});

app.Run();

static async Task ProxyApiCall(HttpContext ctx, HttpClient client, string apiBaseUrl)
{
    var targetUrl = $"{apiBaseUrl}{ctx.Request.Path}{ctx.Request.QueryString}";
    var req = new HttpRequestMessage(new HttpMethod(ctx.Request.Method), targetUrl);
    if (ctx.Request.ContentLength > 0 || ctx.Request.ContentType != null)
    {
        req.Content = new StreamContent(ctx.Request.Body);
        if (ctx.Request.ContentType != null)
            req.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(ctx.Request.ContentType);
    }
    foreach (var h in ctx.Request.Headers)
    {
        if (!h.Key.StartsWith("Host", StringComparison.OrdinalIgnoreCase))
            req.Headers.TryAddWithoutValidation(h.Key, h.Value.ToArray());
    }
    var res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
    ctx.Response.StatusCode = (int)res.StatusCode;
    foreach (var h in res.Headers) ctx.Response.Headers[h.Key] = h.Value.ToArray();
    foreach (var h in res.Content.Headers) ctx.Response.Headers[h.Key] = h.Value.ToArray();
    ctx.Response.Headers.Remove("transfer-encoding");
    await res.Content.CopyToAsync(ctx.Response.Body);
}

public partial class Program { }
