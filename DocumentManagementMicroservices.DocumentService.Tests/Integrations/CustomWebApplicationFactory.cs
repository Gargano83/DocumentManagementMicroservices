using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MongoDb;
using Testcontainers.RabbitMq;
using Testcontainers.Redis;

namespace DocumentManagementMicroservices.DocumentService.Tests.Integrations
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoDbContainer;
        private readonly RedisContainer _redisContainer;
        private readonly RabbitMqContainer _rabbitMqContainer;

        public CustomWebApplicationFactory()
        {
            // Definiamo i 3 container Docker usa-e-getta
            _mongoDbContainer = new MongoDbBuilder().WithImage("mongo:latest").Build();
            _redisContainer = new RedisBuilder().WithImage("redis:latest").Build();
            _rabbitMqContainer = new RabbitMqBuilder().WithImage("rabbitmq:management").Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Sovrascrivo la configurazione originale di Aspire per puntare ai container di test
            builder.UseSetting("ConnectionStrings:documentdb", _mongoDbContainer.GetConnectionString());
            builder.UseSetting("ConnectionStrings:auditlogdb", _mongoDbContainer.GetConnectionString());
            builder.UseSetting("ConnectionStrings:redis", _redisContainer.GetConnectionString());
            builder.UseSetting("ConnectionStrings:rabbitmq", _rabbitMqContainer.GetConnectionString());
        }

        public async Task InitializeAsync()
        {
            // Avvio fisicamente i container su Docker Desktop prima di iniziare i test
            await Task.WhenAll(
                _mongoDbContainer.StartAsync(),
                _redisContainer.StartAsync(),
                _rabbitMqContainer.StartAsync()
            );
        }

        public new async Task DisposeAsync()
        {
            // Spengo e distruggo i container alla fine
            await Task.WhenAll(
                _mongoDbContainer.DisposeAsync().AsTask(),
                _redisContainer.DisposeAsync().AsTask(),
                _rabbitMqContainer.DisposeAsync().AsTask()
            );
        }
    }
}
