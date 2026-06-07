using DocumentManagementMicroservices.DocumentService.Domain.Enums;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.ChangeDocumentStatus
{
    /// <summary>
    /// Payload HTTP per il cambio di stato.
    /// L'Id viene omesso perché viene recuperato dalla Url.
    /// </summary>
    public record ChangeDocumentStatusRequest(DocumentStatus NewStatus, int ExpectedVersion);
}
