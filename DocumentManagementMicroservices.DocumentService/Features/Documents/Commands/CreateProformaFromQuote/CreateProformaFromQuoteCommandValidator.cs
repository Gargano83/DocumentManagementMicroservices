using FluentValidation;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateProformaFromQuote
{
    public class CreateProformaFromQuoteCommandValidator : AbstractValidator<CreateProformaFromQuoteCommand>
    {
        public CreateProformaFromQuoteCommandValidator()
        {
            RuleFor(x => x.QuoteId).NotEmpty().WithMessage("L'ID del preventivo è obbligatorio.")
                                    .Length(24).WithMessage("L'ID del preventivo deve essere un ObjectId di MongoDB valido (24 caratteri).");
        }
    }
}
