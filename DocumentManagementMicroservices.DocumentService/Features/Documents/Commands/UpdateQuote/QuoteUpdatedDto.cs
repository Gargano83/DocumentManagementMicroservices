namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.UpdateQuote
{
    public record QuoteUpdatedDto(string Id, int NewValidityDays, int NewVersion);
}
