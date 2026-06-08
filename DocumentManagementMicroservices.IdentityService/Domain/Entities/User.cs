using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DocumentManagementMicroservices.IdentityService.Domain.Entities
{
    /// <summary>
    /// Entità di dominio principale per il microservizio di Identity.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Identificatore univoco dell'utente.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        /// <summary>
        /// Identificativo univoco utilizzato per la procedura di login.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Hash crittografico della password.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Ruolo autorizzativo assegnato all'utente per la gestione dei permessi.
        /// </summary>
        public string Role { get; set; } = "User";

        /// <summary>
        /// Timestamp di creazione dell'utenza.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
