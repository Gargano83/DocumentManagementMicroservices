using DocumentManagementMicroservices.IdentityService.Features.Auth.Queries.Login;
using System.Net;
using System.Net.Http.Json;

namespace DocumentManagementMicroservices.IdentityService.Tests.Integrations
{
    public class AuthEndToEndTests : IClassFixture<IdentityWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public AuthEndToEndTests(IdentityWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_WithValidCredentials_ShouldReturnToken()
        {
            // Arrange: Il Seeder ha già creato l'utente "admin" con password "password" all'avvio del container
            var request = new LoginQuery("admin", "password");

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.NotNull(content);
            Assert.False(string.IsNullOrWhiteSpace(content.Token));
            Assert.Equal(7200, content.ExpiresIn);
        }

        [Fact]
        public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
        {
            // Arrange
            var request = new LoginQuery("admin", "WrongPassword123");

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Login_WithEmptyInput_ShouldReturnBadRequest()
        {
            // Arrange: Testo che FluentValidation stia intercettando le richieste malformate
            var request = new LoginQuery("", "");

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/auth/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
