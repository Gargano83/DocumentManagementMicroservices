using FluentValidation;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.UpdateQuote
{
    public class UpdateQuoteCommandValidator : AbstractValidator<UpdateQuoteCommand>
    {
        public UpdateQuoteCommandValidator()
        {
            RuleFor(x => x.QuoteId).NotEmpty().Length(24);
            RuleFor(x => x.CustomerId).NotEmpty();
            RuleFor(x => x.ValidityDays).GreaterThan(0);
            RuleFor(x => x.ExpectedVersion).GreaterThan(0);
        }
    }
}
