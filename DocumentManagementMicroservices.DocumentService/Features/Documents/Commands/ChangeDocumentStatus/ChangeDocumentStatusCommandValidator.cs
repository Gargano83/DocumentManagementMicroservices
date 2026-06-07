using FluentValidation;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.ChangeDocumentStatus
{
    public class ChangeDocumentStatusCommandValidator : AbstractValidator<ChangeDocumentStatusCommand>
    {
        public ChangeDocumentStatusCommandValidator()
        {
            RuleFor(x => x.DocumentId).NotEmpty().WithMessage("Il Document ID è obbligatorio.")
                                        .Length(24).WithMessage("Formato ObjectId non valido.");

            RuleFor(x => x.NewStatus).IsInEnum().WithMessage("Stato documento non valido.");

            RuleFor(x => x.ExpectedVersion).GreaterThan(0).WithMessage("La versione attesa deve essere specificata per il controllo di concorrenza.");
        }
    }
}
