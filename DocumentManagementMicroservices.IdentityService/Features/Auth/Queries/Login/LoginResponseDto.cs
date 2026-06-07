namespace DocumentManagementMicroservices.IdentityService.Features.Auth.Queries.Login
{
    public record LoginResponseDto(string Token, int ExpiresIn);
}
