using Escandallo.Core.Services;
using Escandallo.Core.Contracts;
using Escandallo.Api.Logging;

var builder = WebApplication.CreateBuilder(args);

var logBuffer = new SystemLogBuffer();
builder.Services.AddSingleton<ISystemLogBuffer>(logBuffer);

// Configuración de Logging estructurado
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddProvider(new MemoryLoggerProvider(logBuffer));

// Habilitar CORS para consumo desacoplado desde Escandallo.Web y otros clientes
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Registro de servicios del núcleo de dominio
builder.Services.AddSingleton<IEscandalloCostEngine, EscandalloCostEngine>();
builder.Services.AddSingleton<IMarginCalculator, MarginCalculator>();
builder.Services.AddSingleton<ISubscriptionManager, SubscriptionManager>();
builder.Services.AddSingleton<IExportDocumentEngine, ExportDocumentEngine>();
builder.Services.AddSingleton<IAuthGateway, AuthGateway>();
builder.Services.AddSingleton<IEscandalloRepository, EscandalloRepository>();
builder.Services.AddSingleton<ICostConfigurationRepository, CostConfigurationRepository>();
builder.Services.AddSingleton<IDemoManager, DemoManager>();
builder.Services.AddSingleton<IAuditTrailRepository, AuditTrailRepository>();
builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();

// API REST Controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();
app.UseRouting();
app.MapControllers();

app.Run();

// Definición para integración en WebApplicationFactory de tests
public partial class Program { }
