using MediatR;

namespace DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById
{
    // LA QUERY: Richiede in input l'Id e restituisce il DTO DocumentDto
    public class GetDocumentByIdQuery : IRequest<DocumentDto?>
    {
        public string Id { get; set; }

        public GetDocumentByIdQuery(string id)
        {
            Id = id;
        }
    }
}
