using FluentValidation;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote
{
    public class CreateQuoteCommandValidator : AbstractValidator<CreateQuoteCommand>
    {
        public CreateQuoteCommandValidator()
        {
            RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Il CustomerId è obbligatorio.");

            RuleFor(x => x.ValidityDays).GreaterThan(0).WithMessage("I giorni di validità devono essere maggiori di zero.");
        }
    }
}
