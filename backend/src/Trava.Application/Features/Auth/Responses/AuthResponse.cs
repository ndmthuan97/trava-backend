namespace Trava.Application.Features.Auth.Responses;

public record AuthResponse
{
    public string AccessToken { get; init; } = default!;
    public string RefreshToken { get; init; } = default!;
    public int ExpiresIn { get; init; }
    public string? Email { get; init; }
}
