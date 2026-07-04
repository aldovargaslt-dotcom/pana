using Pana.Api.Domain.Identity;

namespace Pana.Tests.Domain;

public class UserTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User(TenantId, "test@bakery.com", hash, "Test User", User.Roles.Admin);

        Assert.Equal("test@bakery.com", user.Email);
        Assert.Equal("Test User", user.DisplayName);
        Assert.Equal(User.Roles.Admin, user.Role);
        Assert.True(user.IsActive);
    }

    [Fact]
    public void SetRole_InvalidRole_ShouldThrow()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("password123");
        var user = new User(TenantId, "test@bakery.com", hash, "Test", User.Roles.Staff);

        Assert.Throws<ArgumentException>(() => user.SetRole("SuperAdmin"));
    }

    [Fact]
    public void IsPasswordValid_CorrectPassword_ShouldReturnTrue()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("secure123");
        var user = new User(TenantId, "test@bakery.com", hash, "Test", User.Roles.Staff);

        Assert.True(user.IsPasswordValid("secure123"));
    }

    [Fact]
    public void IsPasswordValid_WrongPassword_ShouldReturnFalse()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("secure123");
        var user = new User(TenantId, "test@bakery.com", hash, "Test", User.Roles.Staff);

        Assert.False(user.IsPasswordValid("wrongpassword"));
    }

    [Fact]
    public void SetRefreshToken_ShouldStoreToken()
    {
        var hash = BCrypt.Net.BCrypt.HashPassword("secure123");
        var user = new User(TenantId, "test@bakery.com", hash, "Test", User.Roles.Staff);
        var expiry = DateTime.UtcNow.AddDays(7);

        user.SetRefreshToken("token-abc", expiry);

        Assert.Equal("token-abc", user.RefreshToken);
        Assert.Equal(expiry, user.RefreshTokenExpiry);
    }
}
