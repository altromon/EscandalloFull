using Escandallo.Core.Services;
using Escandallo.Core.Contracts;
using Escandallo.Api.Logging;

var builder = WebApplication.CreateBuilder(args);

var logBuffer = new SystemLogBuffer();
builder.Services.AddSingleton<ISystemLogBuffer>(logBuffer);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(new MemoryLoggerProvider(logBuffer));

builder.Services.AddSingleton<IEscandalloCostEngine, EscandalloCostEngine>();
builder.Services.AddSingleton<IMarginCalculator, MarginCalculator>();
builder.Services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
builder.Services.AddSingleton<IEscandalloRepository, EscandalloRepository>();
builder.Services.AddSingleton<ICostConfigurationRepository, CostConfigurationRepository>();
builder.Services.AddSingleton<IExportDocumentEngine, ExportDocumentEngine>();
builder.Services.AddSingleton<IAuthGateway, AuthGateway>();
builder.Services.AddSingleton<IDemoManager, DemoManager>();
builder.Services.AddSingleton<IAuditTrailRepository, AuditTrailRepository>();
builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();

builder.Services.AddControllers()
    .AddApplicationPart(typeof(Escandallo.Api.Controllers.CostsController).Assembly);
builder.Services.AddRazorPages();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapRazorPages();
app.MapFallbackToPage("/Index");

app.Run();

public partial class Program { }
