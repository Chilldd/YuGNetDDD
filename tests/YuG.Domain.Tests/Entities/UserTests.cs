using Moq;
using YuG.Domain.Entities;
using YuG.Domain.Interfaces;
using YuG.Domain.ValueObjects;

namespace YuG.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Constructor_WhenCalled_InitializesUserCorrectly()
    {
        var username = "testuser";
        var passwordHash = "hashed_password";

        var user = new User(username, passwordHash);

        Assert.Equal(username, user.Username);
        Assert.Equal(passwordHash, user.PasswordHash);
        Assert.Empty(user.RefreshTokens);
    }

    [Fact]
    public void VerifyPassword_WhenPasswordCorrect_ReturnsTrue()
    {
        var password = "password123";
        var passwordHash = "hashed_password";
        var user = new User("testuser", passwordHash);
        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock
            .Setup(x => x.Verify(password, passwordHash))
            .Returns(true);

        bool result = user.VerifyPassword(passwordHasherMock.Object, password);

        Assert.True(result);
        passwordHasherMock.Verify(x => x.Verify(password, passwordHash), Times.Once);
    }

    [Fact]
    public void VerifyPassword_WhenPasswordIncorrect_ReturnsFalse()
    {
        var password = "password123";
        var passwordHash = "hashed_password";
        var user = new User("testuser", passwordHash);
        var passwordHasherMock = new Mock<IPasswordHasher>();
        passwordHasherMock
            .Setup(x => x.Verify(password, passwordHash))
            .Returns(false);

        bool result = user.VerifyPassword(passwordHasherMock.Object, password);

        Assert.False(result);
    }

    [Fact]
    public void AddRefreshToken_WhenCalled_AddsTokenAndRemovesExpiredOnes()
    {
        var user = new User("testuser", "hashed_password");

        var expiredToken = new RefreshToken
        {
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            IsRevoked = false
        };

        var validToken = new RefreshToken
        {
            Token = "valid-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        var newToken = new RefreshToken
        {
            Token = "new-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        user.AddRefreshToken(expiredToken);
        user.AddRefreshToken(validToken);
        user.AddRefreshToken(newToken);

        Assert.DoesNotContain(user.RefreshTokens, t => t.Token == "expired-token");
        Assert.Contains(user.RefreshTokens, t => t.Token == "valid-token");
        Assert.Contains(user.RefreshTokens, t => t.Token == "new-token");
    }

    [Fact]
    public void AddRefreshToken_WhenCalled_RemovesRevokedTokens()
    {
        var user = new User("testuser", "hashed_password");

        var revokedToken = new RefreshToken
        {
            Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = true
        };

        var newToken = new RefreshToken
        {
            Token = "new-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };

        user.AddRefreshToken(revokedToken);
        user.AddRefreshToken(newToken);

        Assert.DoesNotContain(user.RefreshTokens, t => t.Token == "revoked-token");
        Assert.Contains(user.RefreshTokens, t => t.Token == "new-token");
    }

    [Fact]
    public void RevokeRefreshToken_WhenTokenExists_ReturnsTrue()
    {
        var user = new User("testuser", "hashed_password");
        var token = new RefreshToken
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
        user.AddRefreshToken(token);

        bool result = user.RevokeRefreshToken("test-token");

        Assert.True(result);
        var revokedToken = user.RefreshTokens.First(t => t.Token == "test-token");
        Assert.True(revokedToken.IsRevoked);
    }

    [Fact]
    public void RevokeRefreshToken_WhenTokenNotExists_ReturnsFalse()
    {
        var user = new User("testuser", "hashed_password");

        bool result = user.RevokeRefreshToken("non-existent-token");

        Assert.False(result);
    }

    [Fact]
    public void GetValidRefreshToken_WhenTokenValid_ReturnsToken()
    {
        var user = new User("testuser", "hashed_password");
        var token = new RefreshToken
        {
            Token = "test-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false
        };
        user.AddRefreshToken(token);

        var result = user.GetValidRefreshToken("test-token");

        Assert.NotNull(result);
        Assert.Equal("test-token", result!.Token);
    }

    [Fact]
    public void GetValidRefreshToken_WhenTokenExpired_ReturnsNull()
    {
        var user = new User("testuser", "hashed_password");
        var token = new RefreshToken
        {
            Token = "expired-token",
            ExpiresAt = DateTime.UtcNow.AddDays(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            IsRevoked = false
        };
        user.AddRefreshToken(token);

        var result = user.GetValidRefreshToken("expired-token");

        Assert.Null(result);
    }

    [Fact]
    public void GetValidRefreshToken_WhenTokenRevoked_ReturnsNull()
    {
        var user = new User("testuser", "hashed_password");
        var token = new RefreshToken
        {
            Token = "revoked-token",
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            CreatedAt = DateTime.UtcNow,
            IsRevoked = true
        };
        user.AddRefreshToken(token);

        var result = user.GetValidRefreshToken("revoked-token");

        Assert.Null(result);
    }

    [Fact]
    public void GetValidRefreshToken_WhenTokenNotExists_ReturnsNull()
    {
        var user = new User("testuser", "hashed_password");

        var result = user.GetValidRefreshToken("non-existent-token");

        Assert.Null(result);
    }
}
