using FluentValidation;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentByIdQueryValidator : AbstractValidator<GetDocumentByIdQuery>
    {
        public GetDocumentByIdQueryValidator()
        {
            RuleFor(x => x.DocumentId).NotEmpty().WithMessage("Il Document ID è obbligatorio.")
                                        .Length(24).WithMessage("Il Document ID deve essere un ObjectId di MongoDB valido (24 caratteri).");
        }
    }
}
