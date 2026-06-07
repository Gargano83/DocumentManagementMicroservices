using FluentValidation;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.SearchDocuments
{
    public class SearchDocumentsQueryValidator : AbstractValidator<SearchDocumentsQuery>
    {
        public SearchDocumentsQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100).WithMessage("La dimensione della pagina deve essere compresa tra 1 e 100.");
        }
    }
}
