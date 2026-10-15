using Escandallo.Core.Services;
using Escandallo.Core.Contracts;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Logging estructurado
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

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
builder.Services.AddSingleton<IDemoManager, DemoManager>();
builder.Services.AddSingleton<IAuditTrailRepository, AuditTrailRepository>();
builder.Services.AddSingleton<IHealthCheckService, HealthCheckService>();

builder.Services.AddSingleton<IEscandalloRepository, EscandalloRepository>();
builder.Services.AddSingleton<ICostConfigurationRepository, CostConfigurationRepository>();

// API REST Controllers
builder.Services.AddControllers();

var app = builder.Build();

app.UseCors();
app.UseRouting();
app.MapControllers();

app.Run();

// Definición para integración en WebApplicationFactory de tests
public partial class Program { }
