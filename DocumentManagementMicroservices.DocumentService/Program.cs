using DocumentManagementMicroservices.DocumentService.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Aggiunge i default di Aspire (Telemetria, HealthChecks, ecc.)
builder.AddServiceDefaults();

#region CONFIGURAZIONE API E WEB
builder.AddApiConfiguration();
#endregion

#region CONFIGURAZIONE APPLICATION (CQRS E VALIDAZIONE)
builder.AddApplicationServices();
#endregion

#region CONFIGURAZIONE INFRASTRUTTURA (DB E CACHE)
builder.AddInfrastructureServices();
#endregion

#region CONFIGURAZIONE MESSAGGISTICA (RABBITMQ)
builder.AddMessagingServices();
#endregion

var app = builder.Build();

// Configurazione della pipeline HTTP
app.ConfigurePipeline();

app.Run();

public partial class Program { }