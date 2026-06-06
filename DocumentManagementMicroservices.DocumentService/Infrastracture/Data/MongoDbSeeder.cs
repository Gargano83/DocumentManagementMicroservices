using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using MongoDB.Driver;

namespace DocumentManagementMicroservices.DocumentService.Infrastracture.Data
{
    /// <summary>
    /// Servizio in background che si occupa di inizializzare il database MongoDB all'avvio dell'applicazione.
    /// Garantisce la creazione degli indici e l'inserimento di dati di base (Seed) se il db è vuoto.
    /// </summary>
    public class MongoDbSeeder : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MongoDbSeeder> _logger;

        public MongoDbSeeder(IServiceProvider serviceProvider, ILogger<MongoDbSeeder> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Inizio verifica e autoconfigurazione di MongoDB...");

            // Uso uno scope per risolvere i servizi necessari (come il MongoClient iniettato da Aspire)
            using var scope = _serviceProvider.CreateScope();
            var mongoClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();

            var database = mongoClient.GetDatabase("documentdb");
            var collection = database.GetCollection<DocumentBase>("Documents");

            // Creazione degli indici
            await ConfigureIndexesAsync(collection, cancellationToken);

            // Popolo il database con dati di test se risulta vuoto
            await SeedDataAsync(collection, cancellationToken);

            _logger.LogInformation("Autoconfigurazione di MongoDB completata.");
        }

        private async Task ConfigureIndexesAsync(IMongoCollection<DocumentBase> collection, CancellationToken cancellationToken)
        {
            // Indice univoco sul DocumentNumber: previene l'inserimento di due documenti con lo stesso numero
            var documentNumberIndexKeys = Builders<DocumentBase>.IndexKeys.Ascending(doc => doc.DocumentNumber);
            var documentNumberIndexOptions = new CreateIndexOptions { Unique = true };
            var documentNumberIndexModel = new CreateIndexModel<DocumentBase>(documentNumberIndexKeys, documentNumberIndexOptions);

            // Indice sullo Status: ottimizza le query di ricerca (es. "trova tutti i documenti in stato Draft")
            var statusIndexKeys = Builders<DocumentBase>.IndexKeys.Ascending(doc => doc.Status);
            var statusIndexModel = new CreateIndexModel<DocumentBase>(statusIndexKeys);

            // Applico gli indici sul database
            await collection.Indexes.CreateManyAsync(
                new[] { documentNumberIndexModel, statusIndexModel },
                cancellationToken);
        }

        private async Task SeedDataAsync(IMongoCollection<DocumentBase> collection, CancellationToken cancellationToken)
        {
            // Controllo quanti documenti ci sono nella collection
            var count = await collection.CountDocumentsAsync(Builders<DocumentBase>.Filter.Empty, cancellationToken: cancellationToken);

            if (count == 0)
            {
                _logger.LogInformation("Database vuoto rilevato. Inizio l'inserimento dei dati di base (Seeding)...");

                var seedDocuments = new List<DocumentBase>
                {
                    // Preventivo in stato Draft
                    new Quote
                    {
                        DocumentNumber = "PREV-2026-001",
                        IssueDate = DateTime.UtcNow.AddDays(-5),
                        CustomerId = "CUST-A",
                        Status = DocumentStatus.Draft,
                        ValidUntil = DateTime.UtcNow.AddDays(25),
                        CreatedAt = DateTime.UtcNow.AddDays(-5),
                        CreatedBy = "System_Seeder",
                        Version = 1
                    },
                    // Preventivo Approvato
                    new Quote
                    {
                        DocumentNumber = "PREV-2026-002",
                        IssueDate = DateTime.UtcNow.AddDays(-10),
                        CustomerId = "CUST-B",
                        Status = DocumentStatus.Approved,
                        ValidUntil = DateTime.UtcNow.AddDays(20),
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        CreatedBy = "System_Seeder",
                        Version = 1
                    },
                    // Proforma (generata idealmente dal preventivo 002)
                    new Proforma
                    {
                        DocumentNumber = "PROF-2026-001",
                        IssueDate = DateTime.UtcNow.AddDays(-2),
                        CustomerId = "CUST-B",
                        Status = DocumentStatus.Complete,
                        SourceQuoteId = "PREV-2026-002",
                        CreatedAt = DateTime.UtcNow.AddDays(-2),
                        CreatedBy = "System_Seeder",
                        Version = 1
                    },
                    // Ordine di vendita finale
                    new SalesOrder
                    {
                        DocumentNumber = "ORD-2026-001",
                        IssueDate = DateTime.UtcNow,
                        CustomerId = "CUST-B",
                        Status = DocumentStatus.Complete,
                        SourceProformaId = "PROF-2026-001",
                        ShippingAddress = "Via delle Industrie 10, 20100 Milano (MI)",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System_Seeder",
                        Version = 1
                    }
                };

                // Utilizzo InsertManyAsync per inserire l'intera lista
                await collection.InsertManyAsync(seedDocuments, cancellationToken: cancellationToken);

                _logger.LogInformation("Dati di base inseriti con successo. (Creato Preventivo PREV-2026-001)");
            }
            else
            {
                _logger.LogInformation("Database già popolato (Trovati {Count} documenti). Salto il processo di seeding.", count);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
