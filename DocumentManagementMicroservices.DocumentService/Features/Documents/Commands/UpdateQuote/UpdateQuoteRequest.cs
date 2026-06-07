namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.UpdateQuote
{
    /// <summary>
    /// Payload HTTP per la modifica del preventivo. 
    /// L'Id viene omesso perché viene recuperato dalla Url.
    /// </summary>
    public record UpdateQuoteRequest(string CustomerId, int ValidityDays, int ExpectedVersion);
}
