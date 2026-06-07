using DocumentManagementMicroservices.DocumentService.Domain.Enums;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateProformaFromQuote
{
    public record ProformaCreatedDto(string Id, string DocumentNumber, DocumentStatus Status);
}
