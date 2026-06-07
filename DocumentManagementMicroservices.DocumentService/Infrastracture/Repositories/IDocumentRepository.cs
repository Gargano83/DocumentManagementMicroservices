using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;

namespace DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories
{
    /// <summary>
    /// Interfaccia del repository per l'accesso ai dati dei documenti commerciali.
    /// Focalizzata sulle operazioni del lato Command (CQRS).
    /// </summary>
    public interface IDocumentRepository
    {
        /// <summary>
        /// Recupera un documento tramite ID sfruttando i generics per supportare il polimorfismo (Quote, Proforma, ecc.)
        /// </summary>
        Task<T?> GetByIdAsync<T>(string id) where T : DocumentBase;

        /// <summary>
        /// Inserisce un nuovo documento nel database
        /// </summary>
        Task CreateAsync(DocumentBase document);

        /// <summary>
        /// Aggiorna un documento esistente applicando la concorrenza ottimistica
        /// </summary>
        Task UpdateAsync(DocumentBase document);

        Task<bool> UpdateStatusWithConcurrencyAsync(string id, DocumentStatus newStatus, int expectedVersion);

        Task<bool> UpdateQuoteAsync(Quote quote, int expectedVersion);

        Task<(IEnumerable<DocumentBase> Items, long TotalCount)> SearchAsync(string? customerId, string? status, string? documentType, int pageNumber, int pageSize);
    }
}
