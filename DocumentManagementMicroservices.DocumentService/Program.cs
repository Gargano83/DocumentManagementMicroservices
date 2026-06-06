using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Aggiunge i default di Aspire (Telemetria, HealthChecks, ecc.)
builder.AddServiceDefaults();

builder.Services.AddControllers();

builder.Services.AddOpenApi();

#region REGISTRAZIONE SERVIZI E INFRASTRUTTURA
// Registrazione del client di MongoDB fornito da Aspire puntando al collegamento 'documentdb'
builder.AddMongoDBClient("documentdb");

// Registrazione del Repository Document per la Dependency Injection
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();

// Registrazione del servizio in background per il Data Seeding
builder.Services.AddHostedService<DocumentManagementMicroservices.DocumentService.Infrastracture.Data.MongoDbSeeder>();

// Registrazione di MediatR per l'implementazione del pattern CQRS
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Registrazione di HybridCache (utilizzerà automaticamente Redis come L2 poiché è stato iniettato da Aspire)
builder.Services.AddHybridCache();

// Registrazione di MassTransit E RabbitMq
builder.Services.AddMassTransit(x =>
{
    // Formatto i nomi delle code in stile "kebab-case" (es. document-created-event)
    x.SetKebabCaseEndpointNameFormatter();

    x.UsingRabbitMq((context, cfg) =>
    {
        // Aspire inietta magicamente la stringa di connessione tramite il nome "rabbitmq" che è stata definita nel file Program.cs dell'AppHost.
        var connectionString = builder.Configuration.GetConnectionString("rabbitmq");

        if (!string.IsNullOrEmpty(connectionString))
        {
            cfg.Host(connectionString);
        }

        cfg.ConfigureEndpoints(context);
    });
});
#endregion

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
