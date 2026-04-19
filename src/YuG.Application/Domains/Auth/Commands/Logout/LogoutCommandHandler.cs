using MediatR;
using YuG.Domain.Interfaces;
using YuG.Domain.Repositories;

namespace YuG.Application.Domains.Auth.Commands.Logout;

/// <summary>
/// 登出命令处理器
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// 初始化登出命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    public LogoutCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// 处理登出命令
    /// </summary>
    /// <param name="request">登出命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>Unit</returns>
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // 通过刷新令牌查找用户
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.RefreshTokens.Any(rt => rt.Token == request.RefreshToken));

        if (user != null)
        {
            // 撤销刷新令牌
            user.RevokeRefreshToken(request.RefreshToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
