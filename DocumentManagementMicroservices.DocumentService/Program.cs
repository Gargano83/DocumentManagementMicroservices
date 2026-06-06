using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Aggiunge i default di Aspire (Telemetria, HealthChecks, ecc.)
builder.AddServiceDefaults();

builder.Services.AddControllers();

#region REGISTRAZIONE SERVIZI E INFRASTRUTTURA
// Registra il client di MongoDB fornito da Aspire puntando al collegamento 'documentdb'
builder.AddMongoDBClient("documentdb");

// Registra il nostro Repository per la Dependency Injection
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// Registra il servizio in background per il Data Seeding
builder.Services.AddHostedService<DocumentManagementMicroservices.DocumentService.Infrastracture.Data.MongoDbSeeder>();
#endregion

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseAuthorization();

app.MapControllers();

app.Run();
