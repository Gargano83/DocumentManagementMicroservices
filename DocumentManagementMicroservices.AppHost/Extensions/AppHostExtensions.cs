using DocumentManagementMicroservices.AppHost.Models;

namespace DocumentManagementMicroservices.AppHost.Extensions
{
    public static class AppHostExtensions
    {
        /// <summary>
        /// Configura e registra i container delle risorse infrastrutturali (Redis, RabbitMQ, MongoDB).
        /// Inizializza i volumi per la persistenza dei dati e i relativi strumenti di management (UI).
        /// </summary>
        public static InfrastructureResources AddInfrastructure(this IDistributedApplicationBuilder builder)
        {
            // Redis con UI e persistenza su Volume Docker
            var redis = builder.AddRedis("redis")
                                .WithDataVolume()
                                .WithRedisCommander();

            // RabbitMQ con plugin di Management e persistenza
            var rabbitmq = builder.AddRabbitMQ("rabbitmq")
                                    .WithDataVolume()
                                    .WithManagementPlugin();

            // MongoDB con Mongo Express e persistenza
            var mongodb = builder.AddMongoDB("mongodb")
                                    .WithDataVolume()
                                    .WithMongoExpress();

            // Database logici distinti da iniettare nei servizi
            var documentDb = mongodb.AddDatabase("documentdb");
            var auditlogDb = mongodb.AddDatabase("auditlogdb");
            var identityDb = mongodb.AddDatabase("identitydb");

            // Configurazione del container SonarQube
            builder.AddContainer("sonarqube", "sonarqube", "lts-community")
                   .WithHttpEndpoint(port: 9000, targetPort: 9000, name: "sonarqube-ui")
                   .WithBindMount("./sonarqube_data", "/opt/sonarqube/data")
                   .WithBindMount("./sonarqube_extensions", "/opt/sonarqube/extensions")
                   .WithBindMount("./sonarqube_logs", "/opt/sonarqube/logs");

            return new InfrastructureResources(redis, rabbitmq, documentDb, auditlogDb, identityDb);
        }

        /// <summary>
        /// Configura e registra i microservizi core dell'applicazione (IdentityService, DocumentService).
        /// Gestisce l'iniezione delle dipendenze infrastrutturali e definisce l'ordine di avvio corretto tramite politiche di attesa.
        /// </summary>
        public static MicroserviceResources AddMicroservices(this IDistributedApplicationBuilder builder, InfrastructureResources infra)
        {
            var identityService = builder.AddProject<Projects.DocumentManagementMicroservices_IdentityService>("identityservice")
                                            .WithReference(infra.IdentityDb)
                                            .WithReference(infra.Redis)
                                            .WithReference(infra.RabbitMQ)
                                            .WaitFor(infra.IdentityDb)
                                            .WaitFor(infra.Redis)
                                            .WaitFor(infra.RabbitMQ);

            var documentService = builder.AddProject<Projects.DocumentManagementMicroservices_DocumentService>("documentservice")
                                            .WithReference(infra.DocumentDb)
                                            .WithReference(infra.AuditLogDb)
                                            .WithReference(infra.Redis)
                                            .WithReference(infra.RabbitMQ)
                                            .WaitFor(infra.DocumentDb)
                                            .WaitFor(infra.AuditLogDb)
                                            .WaitFor(infra.Redis)
                                            .WaitFor(infra.RabbitMQ);

            return new MicroserviceResources(identityService, documentService);
        }

        /// <summary>
        /// Configura e registra l'API Gateway dell'applicazione come punto di ingresso unico per il Reverse Proxy (YARP).
        /// Configura il gateway per attendere il corretto avvio di tutti i microservizi a cui deve reindirizzare il traffico.
        /// </summary>
        public static void AddApiGateway(this IDistributedApplicationBuilder builder, MicroserviceResources services)
        {
            builder.AddProject<Projects.DocumentManagementMicroservices_ApiGateway>("apigateway")
                    .WithReference(services.IdentityService)
                    .WithReference(services.DocumentService)
                    .WaitFor(services.IdentityService)
                    .WaitFor(services.DocumentService);
        }
    }
}
