using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DocumentManagementMicroservices.DocumentService.Domain.Entities
{
    /// <summary>
    /// Classe astratta base per tutti i documenti commerciali.
    /// Definisce il contratto comune e gestisce la concorrenza ottimistica.
    /// </summary>
    [BsonDiscriminator(RootClass = true)]
    [BsonKnownTypes(typeof(Quote), typeof(Proforma), typeof(SalesOrder))]
    public abstract class DocumentBase
    {
        /// <summary>
        /// Identificativo univoco nativo di MongoDB
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Numero progressivo del documento
        /// </summary>
        public string DocumentNumber { get; set; } = string.Empty;

        /// <summary>
        /// Data di emissione
        /// </summary>
        public DateTime IssueDate { get; set; }

        /// <summary>
        /// Riferimento al cliente (Referencing pattern, non facciamo l'embedding dell'intera anagrafica)
        /// </summary>
        public string CustomerId { get; set; } = string.Empty;

        /// <summary>
        /// Stato attuale del documento
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public DocumentStatus Status { get; set; } = DocumentStatus.Draft;

        /// <summary>
        /// Audit fields
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gestione della concorrenza ottimistica: va gestita manualmente nel Repository/Service layer.
        /// </summary>
        [BsonElement("version")]
        public long Version { get; set; }
    }
}
