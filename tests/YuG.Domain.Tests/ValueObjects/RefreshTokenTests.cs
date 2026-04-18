using YuG.Domain.ValueObjects;

namespace YuG.Domain.Tests.ValueObjects;

public class RefreshTokenTests
{
    [Fact]
    public void IsValid_WhenTokenNotRevokedAndNotExpired_ReturnsTrue()
    {
        var token = new RefreshToken
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        bool isValid = token.IsValid();

        Assert.True(isValid);
    }

    [Fact]
    public void IsValid_WhenTokenRevoked_ReturnsFalse()
    {
        var token = new RefreshToken
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = true
        };

        bool isValid = token.IsValid();

        Assert.False(isValid);
    }

    [Fact]
    public void IsValid_WhenTokenExpired_ReturnsFalse()
    {
        var token = new RefreshToken
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            IsRevoked = false
        };

        bool isValid = token.IsValid();

        Assert.False(isValid);
    }

    [Fact]
    public void Revoke_WhenTokenNotRevoked_ReturnsRevokedToken()
    {
        var token = new RefreshToken
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        var revokedToken = token.Revoke();

        Assert.True(revokedToken.IsRevoked);
        Assert.Equal(token.Token, revokedToken.Token);
    }

    [Fact]
    public void Revoke_WhenTokenAlreadyRevoked_ReturnsSameInstance()
    {
        var token = new RefreshToken
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = true
        };

        var revokedToken = token.Revoke();

        Assert.Same(token, revokedToken);
    }

    [Fact]
    public void Empty_ReturnsEmptyRefreshToken()
    {
        var emptyToken = RefreshToken.Empty;

        Assert.Empty(emptyToken.Token);
        Assert.False(emptyToken.IsRevoked);
    }
}
