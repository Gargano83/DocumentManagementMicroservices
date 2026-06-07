namespace DocumentManagementMicroservices.AppHost.Models
{
    public record InfrastructureResources(
        IResourceBuilder<RedisResource> Redis, 
        IResourceBuilder<RabbitMQServerResource> RabbitMQ, 
        IResourceBuilder<MongoDBDatabaseResource> DocumentDb,
        IResourceBuilder<MongoDBDatabaseResource> AuditLogDb,
        IResourceBuilder<MongoDBDatabaseResource> IdentityDb
    );
}
