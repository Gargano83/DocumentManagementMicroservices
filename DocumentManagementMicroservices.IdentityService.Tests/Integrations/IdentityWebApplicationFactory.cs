using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.MongoDb;

namespace DocumentManagementMicroservices.IdentityService.Tests.Integrations
{
    public class IdentityWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        private readonly MongoDbContainer _mongoDbContainer;

        public IdentityWebApplicationFactory()
        {
            // Setup del container MongoDB usa-e-getta
            _mongoDbContainer = new MongoDbBuilder().WithImage("mongo:latest").Build();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Sovrascrivo la stringa di connessione affinché punti al container Docker effimero
            builder.UseSetting("ConnectionStrings:identitydb", _mongoDbContainer.GetConnectionString());

            // Configuro i segreti JWT fittizi per l'ambiente di test
            builder.UseSetting("JwtSettings:SecretKey", "QuestaEUnaChiaveSegretaMoltoLungaESicuraPerGenerareIlTokenJWT2026-TEST!");
            builder.UseSetting("JwtSettings:Issuer", "TestIssuer");
            builder.UseSetting("JwtSettings:Audience", "TestAudience");
        }

        public async Task InitializeAsync()
        {
            await _mongoDbContainer.StartAsync();
        }

        public new async Task DisposeAsync()
        {
            await _mongoDbContainer.DisposeAsync().AsTask();
        }
    }
}
