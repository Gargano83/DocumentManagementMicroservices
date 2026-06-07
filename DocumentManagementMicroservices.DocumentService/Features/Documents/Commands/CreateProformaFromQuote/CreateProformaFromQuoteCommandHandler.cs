using DocumentManagementMicroservices.BuildingBlocks.Exceptions;
using DocumentManagementMicroservices.DocumentService.Domain.Entities;
using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Infrastracture.Repositories;
using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateProformaFromQuote
{
    public class CreateProformaFromQuoteCommandHandler : IRequestHandler<CreateProformaFromQuoteCommand, ProformaCreatedDto>
    {
        private readonly IDocumentRepository _repository;

        public CreateProformaFromQuoteCommandHandler(IDocumentRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProformaCreatedDto> Handle(CreateProformaFromQuoteCommand request, CancellationToken cancellationToken)
        {
            var document = await _repository.GetByIdAsync<DocumentBase>(request.QuoteId);

            if (document is null || document is not Quote quote)
            {
                throw new NotFoundException("Quote", request.QuoteId);
            }

            if (quote.Status != DocumentStatus.Approved && quote.Status != DocumentStatus.Complete)
            {
                throw new DomainException($"Cannot create a Proforma from a Quote in status: {quote.Status}");
            }

            var proforma = new Proforma
            {
                DocumentNumber = $"PROF-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..4]}",
                IssueDate = DateTime.UtcNow,
                CustomerId = quote.CustomerId,
                Status = DocumentStatus.Draft,
                Version = 1,
                CreatedAt = DateTime.UtcNow,
                SourceQuoteId = quote.Id
            };

            await _repository.CreateAsync(proforma);

            return new ProformaCreatedDto(proforma.Id, proforma.DocumentNumber, proforma.Status);
        }
    }
}
