using DocumentManagementMicroservices.IdentityService.Domain.Entities;
using DocumentManagementMicroservices.IdentityService.Services;
using MongoDB.Driver;

namespace DocumentManagementMicroservices.IdentityService.Infrastructure.Data
{
    public class IdentityDbSeeder : IHostedService
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ILogger<IdentityDbSeeder> _logger;

        public IdentityDbSeeder(IMongoCollection<User> usersCollection, IPasswordHasher passwordHasher, ILogger<IdentityDbSeeder> logger)
        {
            _usersCollection = usersCollection;
            _passwordHasher = passwordHasher;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Inizializzazione del database Identity in corso...");

            // Creazione Indice Univoco per lo Username
            var indexOptions = new CreateIndexOptions { Unique = true };
            var indexKeys = Builders<User>.IndexKeys.Ascending(u => u.Username);
            var indexModel = new CreateIndexModel<User>(indexKeys, indexOptions);

            await _usersCollection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);

            // Controllo se esistono già utenti
            var usersCount = await _usersCollection.CountDocumentsAsync(FilterDefinition<User>.Empty, cancellationToken: cancellationToken);

            if (usersCount == 0)
            {
                _logger.LogInformation("Nessun utente trovato. Creazione dell'utente Admin di default...");

                var adminUser = new User
                {
                    Username = "admin",
                    PasswordHash = _passwordHasher.Hash("password"),
                    Role = "Administrator",
                    CreatedAt = DateTime.UtcNow
                };

                await _usersCollection.InsertOneAsync(adminUser, cancellationToken: cancellationToken);
                _logger.LogInformation("Utente Admin creato con successo.");
            }
            else
            {
                _logger.LogInformation("Database Identity già popolato. Nessuna azione necessaria.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
