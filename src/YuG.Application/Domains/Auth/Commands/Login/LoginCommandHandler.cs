using MediatR;
using YuG.Application.DTOs.Auth.Responses;
using YuG.Domain.Exceptions;
using YuG.Domain.Interfaces;
using YuG.Domain.Repositories;
using DomainRefreshToken = YuG.Domain.ValueObjects.RefreshToken;

namespace YuG.Application.Auth.Commands.Login;

/// <summary>
/// 登录命令处理器
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>
    /// 初始化登录命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="passwordHasher">密码哈希服务</param>
    /// <param name="jwtTokenService">JWT令牌服务</param>
    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// 处理登录命令
    /// </summary>
    /// <param name="request">登录命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录响应</returns>
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 获取用户
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user == null)
        {
            throw new InvalidCredentialsException();
        }

        // 验证密码
        if (!user.VerifyPassword(_passwordHasher, request.Password))
        {
            throw new InvalidCredentialsException();
        }

        // 生成访问令牌
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Username);

        // 生成刷新令牌
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var refreshToken = new DomainRefreshToken
        {
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        // 保存刷新令牌
        user.AddRefreshToken(refreshToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 计算过期时间
        var expiresAt = DateTime.UtcNow.AddMinutes(300); // 5小时

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = expiresAt
        };
    }
}
