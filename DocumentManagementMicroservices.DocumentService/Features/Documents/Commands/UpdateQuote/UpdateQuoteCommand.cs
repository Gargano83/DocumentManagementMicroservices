using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.UpdateQuote
{
    public record UpdateQuoteCommand(string QuoteId,
                                        string CustomerId,
                                        int ValidityDays,
                                        int ExpectedVersion
                                    ) : IRequest<QuoteUpdatedDto>;
}
