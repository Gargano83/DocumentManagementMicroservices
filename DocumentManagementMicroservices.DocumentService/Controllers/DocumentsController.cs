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

        #region QUERIES (Letture)
        [HttpGet]
        public async Task<IActionResult> SearchDocuments([FromQuery] SearchDocumentsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
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
        #endregion

        #region COMMANDS (Scritture)
        [HttpPost("quotes")]
        public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteCommand command)
        {
            // MediatR in automatico invoca il CreateQuoteCommandHandler
            var result = await _mediator.Send(command);

            // Restituisco un codice HTTP 201 Created con l'Id del nuovo documento nel body
            return CreatedAtAction(nameof(GetDocumentById), new { id = result.Id }, result);
        }

        [HttpPut("quotes/{id}")]
        public async Task<IActionResult> UpdateQuote(string id, [FromBody] UpdateQuoteRequest request)
        {
            // Combina l'ID proveniente dall'URL con i dati del body per formare il Command completo
            var command = new UpdateQuoteCommand(id, request.CustomerId, request.ValidityDays, request.ExpectedVersion);
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPost("{id}/proforma")]
        public async Task<IActionResult> CreateProformaFromQuote(string id)
        {
            var command = new CreateProformaFromQuoteCommand(id);
            var result = await _mediator.Send(command);

            // Ritorna 201 Created con location fittizia
            return CreatedAtAction(nameof(GetDocumentById), new { id = result.Id }, result);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeDocumentStatus(string id, [FromBody] ChangeDocumentStatusRequest request)
        {
            // Combina l'ID proveniente dall'URL con lo stato e la versione provenienti dal body
            var command = new ChangeDocumentStatusCommand(id, request.NewStatus, request.ExpectedVersion);
            var result = await _mediator.Send(command);

            return Ok(result);
        }
        #endregion
    }
}
