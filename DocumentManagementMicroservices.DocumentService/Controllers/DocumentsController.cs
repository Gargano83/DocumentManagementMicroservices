using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagementMicroservices.DocumentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        // Per il controller la sua unica responsabilità è prendere la richiesta HTTP e instradarla verso il giusto Handler tramite MediatR.
        public DocumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("quotes")]
        public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteCommand command)
        {
            // MediatR in automatico invoca il CreateQuoteCommandHandler
            var documentId = await _mediator.Send(command);

            // Restituisco un codice HTTP 201 Created con l'Id del nuovo documento nel body
            return Created($"/api/documents/{documentId}", new { Id = documentId });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDocumentById(string id)
        {
            var query = new GetDocumentByIdQuery(id);
            var document = await _mediator.Send(query);

            if (document == null)
            {
                return NotFound(new { Message = $"Documento con ID {id} non trovato." });
            }

            return Ok(document);
        }
    }
}
