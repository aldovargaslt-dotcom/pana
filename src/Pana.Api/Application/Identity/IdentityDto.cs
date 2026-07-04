namespace Pana.Api.Application.Identity;

// ── DTOs ────────────────────────────────────────────────────────

public record UserDto(
    Guid Id,
    string Email,
    string DisplayName,
    string Role,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? LastLoginAt
);

public record RegisterRequest(
    string Email,
    string Password,
    string DisplayName
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string Token,
    string RefreshToken,
    UserDto User
);

public record RefreshRequest(
    string RefreshToken
);
