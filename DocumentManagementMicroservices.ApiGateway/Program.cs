using DocumentManagementMicroservices.ApiGateway.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Aggiunge i default di Aspire (Telemetria, HealthChecks, ecc.)
builder.AddServiceDefaults();

// Configura YARP e l'Autenticazione JWT
builder.AddGatewayConfiguration();

var app = builder.Build();

// Configura la pipeline HTTP
app.ConfigurePipeline();

app.Run();
