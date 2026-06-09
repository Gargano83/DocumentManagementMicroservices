using Asp.Versioning;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.ChangeDocumentStatus;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateProformaFromQuote;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.UpdateQuote;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.GetDocumentById;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.SearchDocuments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DocumentManagementMicroservices.DocumentService.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class DocumentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        // Per il controller la sua unica responsabilità è prendere la richiesta HTTP e instradarla verso il giusto Handler tramite MediatR.
        public DocumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        #region QUERIES (Letture)
        [HttpGet]
        [ProducesResponseType(typeof(PaginatedDocumentResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchDocuments([FromQuery] SearchDocumentsQuery query)
        {
            var result = await _mediator.Send(request: query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDocumentById(string id)
        {
            var query = new GetDocumentByIdQuery(DocumentId: id);
            var document = await _mediator.Send(request: query);

            if (document == null)
            {
                return NotFound(new { Message = $"Documento con ID {id} non trovato." });
            }

            return Ok(document);
        }
        #endregion

        #region COMMANDS (Scritture)
        [HttpPost("quotes")]
        [ProducesResponseType(typeof(QuoteCreatedDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteCommand command)
        {
            // MediatR in automatico invoca il CreateQuoteCommandHandler
            var result = await _mediator.Send(request: command);

            // Restituisco un codice HTTP 201 Created con l'Id del nuovo documento nel body
            return CreatedAtAction(nameof(GetDocumentById), new { id = result.Id }, result);
        }

        [HttpPut("quotes/{id}")]
        [ProducesResponseType(typeof(QuoteUpdatedDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateQuote(string id, [FromBody] UpdateQuoteRequest request)
        {
            // Combina l'ID proveniente dall'URL con i dati del body per formare il Command completo
            var command = new UpdateQuoteCommand(QuoteId: id, CustomerId: request.CustomerId, ValidityDays: request.ValidityDays, ExpectedVersion: request.ExpectedVersion);
            var result = await _mediator.Send(request: command);

            return Ok(result);
        }

        [HttpPost("{id}/proforma")]
        [ProducesResponseType(typeof(ProformaCreatedDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProformaFromQuote(string id)
        {
            var command = new CreateProformaFromQuoteCommand(QuoteId: id);
            var result = await _mediator.Send(request: command);

            // Ritorna 201 Created con location fittizia
            return CreatedAtAction(nameof(GetDocumentById), new { id = result.Id }, result);
        }

        [HttpPatch("{id}/status")]
        [ProducesResponseType(typeof(DocumentStatusChangedDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeDocumentStatus(string id, [FromBody] ChangeDocumentStatusRequest request)
        {
            // Combina l'ID proveniente dall'URL con lo stato e la versione provenienti dal body
            var command = new ChangeDocumentStatusCommand(DocumentId: id, NewStatus: request.NewStatus, ExpectedVersion: request.ExpectedVersion);
            var result = await _mediator.Send(request: command);

            return Ok(result);
        }
        #endregion
    }
}
