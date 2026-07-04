using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pana.Api.Domain.Identity;
using Pana.Api.Infrastructure.Data;

namespace Pana.Api.Application.Identity;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken ct = default);
}

public class AuthService : IAuthService
{
    private readonly PanaDbContext _db;
    private readonly ITenantContext _tenantContext;
    private readonly IConfiguration _configuration;

    public AuthService(PanaDbContext db, ITenantContext tenantContext, IConfiguration configuration)
    {
        _db = db;
        _tenantContext = tenantContext;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.Users
            .AnyAsync(u => u.Email == email, ct);

        if (exists)
            throw new InvalidOperationException("A user with this email already exists.");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = new User(_tenantContext.TenantId, email, passwordHash, request.DisplayName, User.Roles.Admin);

        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        var token = GenerateToken(user);
        var refreshToken = GenerateRefreshToken(user);
        return new AuthResponse(token, refreshToken, MapToDto(user));
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, ct);

        if (user is null || !user.IsPasswordValid(request.Password))
            return null;

        user.RecordLogin();
        await _db.SaveChangesAsync(ct);

        var token = GenerateToken(user);
        var refreshToken = GenerateRefreshToken(user);
        return new AuthResponse(token, refreshToken, MapToDto(user));
    }

    private string GenerateToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("JWT key is not configured.");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim("tenant_id", user.TenantId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken(User user)
    {
        var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            + Convert.ToBase64String(Guid.NewGuid().ToByteArray());

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        return refreshToken;
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u =>
                u.RefreshToken == refreshToken &&
                u.RefreshTokenExpiry > DateTime.UtcNow &&
                u.IsActive, ct);

        if (user is null)
            return null;

        var newToken = GenerateToken(user);
        var newRefreshToken = GenerateRefreshToken(user);
        await _db.SaveChangesAsync(ct);

        return new AuthResponse(newToken, newRefreshToken, MapToDto(user));
    }

    private static UserDto MapToDto(User u) => new(
        u.Id,
        u.Email,
        u.DisplayName,
        u.Role,
        u.IsActive,
        u.CreatedAt,
        u.LastLoginAt
    );
}
