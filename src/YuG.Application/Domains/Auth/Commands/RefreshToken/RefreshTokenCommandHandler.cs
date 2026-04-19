using MediatR;
using YuG.Application.DTOs.Auth.Responses;
using YuG.Domain.Common;
using YuG.Domain.Identity.Entities;
using YuG.Domain.Identity.Repositories;
using YuG.Domain.Identity.ValueObjects;
using YuG.Domain.Common.Interfaces;
using DomainRefreshToken = YuG.Domain.Identity.ValueObjects.RefreshToken;

namespace YuG.Application.Domains.Auth.Commands.RefreshToken;

/// <summary>
/// 刷新令牌命令处理器
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>
    /// 初始化刷新令牌命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="jwtTokenService">JWT令牌服务</param>
    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// 处理刷新令牌命令
    /// </summary>
    /// <param name="request">刷新令牌命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>刷新令牌响应</returns>
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 通过刷新令牌查找用户
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.RefreshTokens.Any(rt => rt.Token == request.RefreshToken));

        if (user == null)
        {
            throw new DomainException("无效的刷新令牌");
        }

        // 验证刷新令牌是否有效
        var existingToken = user.GetValidRefreshToken(request.RefreshToken);
        if (existingToken == null)
        {
            throw new DomainException("刷新令牌已过期或已撤销");
        }

        // 撤销旧的刷新令牌
        user.RevokeRefreshToken(request.RefreshToken);

        // 生成新的访问令牌
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Username);

        // 生成新的刷新令牌
        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var newRefreshToken = new DomainRefreshToken
        {
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        // 保存新的刷新令牌
        user.AddRefreshToken(newRefreshToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 计算过期时间
        var expiresAt = DateTime.UtcNow.AddMinutes(300); // 5小时

        return new RefreshTokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshTokenValue,
            ExpiresAt = expiresAt
        };
    }
}
