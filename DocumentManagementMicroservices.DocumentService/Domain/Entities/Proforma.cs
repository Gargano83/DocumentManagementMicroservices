namespace DocumentManagementMicroservices.DocumentService.Domain.Entities
{
    public class Proforma : DocumentBase
    {
        /// <summary>
        /// Riferimento al preventivo da cui è stata generata
        /// </summary>
        public string? SourceQuoteId { get; set; }
    }
}
