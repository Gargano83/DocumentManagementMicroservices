using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DocumentManagementMicroservices.IdentityService.Domain.Entities
{
    public class User
    {
        // Lascio che sia MongoDB a generare l'ObjectId
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
