using FluentValidation;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateProformaFromQuote
{
    public class CreateProformaFromQuoteCommandValidator : AbstractValidator<CreateProformaFromQuoteCommand>
    {
        public CreateProformaFromQuoteCommandValidator()
        {
            RuleFor(x => x.QuoteId).NotEmpty().WithMessage("Quote ID is required.")
                                    .Length(24).WithMessage("Quote ID must be a valid 24-character MongoDB ObjectId.");
        }
    }
}
