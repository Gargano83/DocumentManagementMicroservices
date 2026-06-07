using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateProformaFromQuote
{
    public record CreateProformaFromQuoteCommand(string QuoteId) : IRequest<ProformaCreatedDto>;
}
