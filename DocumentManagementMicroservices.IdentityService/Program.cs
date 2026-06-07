using DocumentManagementMicroservices.IdentityService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Aggiunge i default di Aspire (Telemetria, HealthChecks, ecc.)
builder.AddServiceDefaults();

#region CONFIGURAZIONE API E WEB
builder.AddApiConfiguration();
#endregion

#region CONFIGURAZIONE APPLICATION (HASHING PASSWORD)
builder.AddApplicationServices();
#endregion

#region CONFIGURAZIONE INFRASTRUTTURA (DB E CACHE)
builder.AddInfrastructureServices();
#endregion

var app = builder.Build();

// Configurazione della pipeline HTTP
app.ConfigurePipeline();

app.Run();

public partial class Program { }
