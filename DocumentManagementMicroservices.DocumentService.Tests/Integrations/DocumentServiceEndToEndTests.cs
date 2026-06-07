using DocumentManagementMicroservices.DocumentService.Domain.Enums;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.ChangeDocumentStatus;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateProformaFromQuote;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.CreateQuote;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Commands.UpdateQuote;
using DocumentManagementMicroservices.DocumentService.Features.Documents.Queries.SearchDocuments;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocumentManagementMicroservices.DocumentService.Tests.Integrations
{
    public class DocumentServiceEndToEndTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public DocumentServiceEndToEndTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task CompleteDocumentLifecycle_ShouldPassSuccessfully()
        {
            var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            jsonOptions.Converters.Add(new JsonStringEnumConverter());

            #region STEP 1: Creazione del Preventivo (Simula Request 1)
            var createCommand = new CreateQuoteCommand("TEST-CUST-001", 30);
            var createResponse = await _client.PostAsJsonAsync("/api/v1/documents/quotes", createCommand);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var createdQuote = await createResponse.Content.ReadFromJsonAsync<QuoteCreatedDto>(jsonOptions);
            Assert.NotNull(createdQuote);

            var documentId = createdQuote.Id;
            #endregion

            #region STEP 2 & 3: Lettura e Caching Redis (Simula Request 2)
            var getResponse1 = await _client.GetAsync($"/api/v1/documents/{documentId}");
            Assert.Equal(HttpStatusCode.OK, getResponse1.StatusCode);

            var getResponse2 = await _client.GetAsync($"/api/v1/documents/{documentId}");
            Assert.Equal(HttpStatusCode.OK, getResponse2.StatusCode);
            #endregion

            #region STEP 3.5: Aggiorna Preventivo (Simula Request 3)
            var updateRequest = new UpdateQuoteRequest("TEST-CUST-MODIFIED", 15, 1);
            var updateResponse = await _client.PutAsJsonAsync($"/api/v1/documents/quotes/{documentId}", updateRequest);

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
            var updatedQuote = await updateResponse.Content.ReadFromJsonAsync<QuoteUpdatedDto>(jsonOptions);
            Assert.NotNull(updatedQuote);
            Assert.Equal(2, updatedQuote.NewVersion);
            #endregion

            #region STEP 4: Cambio Stato e Innesco RabbitMQ (Simula Request 4)
            var changeStatusRequest = new ChangeDocumentStatusRequest(DocumentStatus.Complete, 2);
            var changeStatusResponse = await _client.PatchAsJsonAsync($"/api/v1/documents/{documentId}/status", changeStatusRequest);

            Assert.Equal(HttpStatusCode.OK, changeStatusResponse.StatusCode);
            var statusResult = await changeStatusResponse.Content.ReadFromJsonAsync<DocumentStatusChangedDto>(jsonOptions);
            Assert.Equal("Complete", statusResult.NewStatus);
            #endregion

            #region STEP 5: Genera Proforma (Simula Request 5)
            var proformaResponse = await _client.PostAsync($"/api/v1/documents/{documentId}/proforma", null);

            Assert.Equal(HttpStatusCode.Created, proformaResponse.StatusCode);
            var createdProforma = await proformaResponse.Content.ReadFromJsonAsync<ProformaCreatedDto>(jsonOptions);
            Assert.NotNull(createdProforma);
            Assert.StartsWith("PROF-", createdProforma.DocumentNumber);
            #endregion

            #region STEP 6: Ricerca Documenti (Simula Request 6)
            var searchResponse = await _client.GetAsync("/api/v1/documents?status=Complete&pageNumber=1&pageSize=10");

            Assert.Equal(HttpStatusCode.OK, searchResponse.StatusCode);
            var searchResult = await searchResponse.Content.ReadFromJsonAsync<PaginatedDocumentResultDto>(jsonOptions);

            Assert.NotNull(searchResult);
            Assert.True(searchResult.TotalCount >= 1);
            Assert.Contains(searchResult.Items, doc => doc.CustomerId == "TEST-CUST-MODIFIED");
            #endregion
        }
    }
}
