using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote
{
    public record CreateQuoteCommand(string CustomerId, int ValidityDays = 30) : IRequest<QuoteCreatedDto>;
}
