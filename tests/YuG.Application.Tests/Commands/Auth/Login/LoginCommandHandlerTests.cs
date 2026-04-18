using MediatR;
using Moq;
using YuG.Application.Auth.Commands.Login;
using YuG.Domain.Entities;
using YuG.Domain.Exceptions;
using YuG.Domain.Interfaces;
using YuG.Domain.Repositories;

namespace YuG.Application.Tests.Commands.Auth.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ThrowsInvalidCredentialsException()
    {
        var command = new LoginCommand { Username = "nonexistent", Password = "password" };
        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_WhenPasswordIncorrect_ThrowsInvalidCredentialsException()
    {
        var user = new User("testuser", "hashed_password");
        var command = new LoginCommand { Username = "testuser", Password = "wrong_password" };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(x => x.Verify(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            _handler.Handle(command, CancellationToken.None)
        );
    }

    [Fact]
    public async Task Handle_WhenCredentialsValid_ReturnsLoginResponse()
    {
        var user = new User("testuser", "hashed_password");
        var command = new LoginCommand { Username = "testuser", Password = "correct_password" };
        var expectedAccessToken = "access-token";
        var expectedRefreshToken = "refresh-token";

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _passwordHasherMock
            .Setup(x => x.Verify(command.Password, user.PasswordHash))
            .Returns(true);

        _jwtTokenServiceMock
            .Setup(x => x. GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns(expectedAccessToken);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns(expectedRefreshToken);

        var response = await _handler.Handle(command, CancellationToken.None);

        Assert.NotNull(response);
        Assert.Equal(expectedAccessToken, response.AccessToken);
        Assert.Equal(expectedRefreshToken, response.RefreshToken);
        Assert.Equal(DateTime.UtcNow.AddMinutes(300).Minute, response.ExpiresAt.Minute);

        _userRepositoryMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCredentialsValid_AddsRefreshTokenToUser()
    {
        var user = new User("testuser", "hashed_password");
        var command = new LoginCommand { Username = "testuser", Password = "correct_password" };

        _userRepositoryMock
            .Setup(x => x.GetByUsernameAsync(command.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _passwordHasherMock
            .Setup(x => x.Verify(command.Password, user.PasswordHash))
            .Returns(true);

        _jwtTokenServiceMock
            .Setup(x => x.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>()))
            .Returns("access-token");

        _jwtTokenServiceMock
            .Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        await _handler.Handle(command, CancellationToken.None);

        Assert.Single(user.RefreshTokens);
        Assert.Contains(user.RefreshTokens, t => t.Token == "refresh-token");
    }
}
