namespace Pana.Api.Domain.Identity;

using Pana.Api.Domain.Common;

/// <summary>
/// A user account belonging to a tenant.
/// Roles: Admin, Manager, Staff.
/// </summary>
public class User : TenantEntity
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string DisplayName { get; private set; }
    public string Role { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAt { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Staff = "Staff";
    }

    private User() { } // EF Core

    public User(Guid tenantId, string email, string passwordHash, string displayName, string role)
        : base(tenantId)
    {
        SetEmail(email);
        PasswordHash = passwordHash;
        SetDisplayName(displayName);
        SetRole(role);
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        Email = email.Trim().ToLowerInvariant();
        MarkUpdated();
    }

    public void SetPasswordHash(string hash)
    {
        PasswordHash = hash;
        MarkUpdated();
    }

    public void SetDisplayName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Display name is required.", nameof(name));
        DisplayName = name.Trim();
        MarkUpdated();
    }

    public void SetRole(string role)
    {
        if (role is not (Roles.Admin or Roles.Manager or Roles.Staff))
            throw new ArgumentException($"Invalid role: {role}.", nameof(role));
        Role = role;
        MarkUpdated();
    }

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Activate() { IsActive = true; MarkUpdated(); }
    public void Deactivate() { IsActive = false; MarkUpdated(); }

    public bool IsPasswordValid(string password)
        => BCrypt.Net.BCrypt.Verify(password, PasswordHash);
}
