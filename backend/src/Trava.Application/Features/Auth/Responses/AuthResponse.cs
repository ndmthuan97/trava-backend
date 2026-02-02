namespace Trava.Application.Features.Auth.Responses;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string? Email
);
