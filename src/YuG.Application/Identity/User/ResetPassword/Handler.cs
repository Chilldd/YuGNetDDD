using MediatR;
using YuG.Application.Common;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Common.Interfaces;
using YuG.Domain.Identity.Repositories;
using UserResult = YuG.Application.Identity.User.Create.UserResult;
using UserEntity = YuG.Domain.Identity.Entities.User;

namespace YuG.Application.Identity.User.ResetPassword;

/// <summary>
/// 重置用户密码命令处理器
/// </summary>
public class Handler : IRequestHandler<ResetPasswordCommand, UserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICache _cache;

    /// <summary>
    /// 初始化重置用户密码命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="passwordHasher">密码哈希服务</param>
    /// <param name="cache">缓存服务</param>
    public Handler(IUserRepository userRepository, IPasswordHasher passwordHasher, ICache cache)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _cache = cache;
    }

    /// <summary>
    /// 处理重置用户密码命令
    /// </summary>
    /// <param name="request">重置用户密码命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户响应</returns>
    public async Task<UserResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithRefreshTokensAsync(request.Id, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException(nameof(UserEntity), request.Id);
        }

        const string defaultPassword = "123456";
        var passwordHash = _passwordHasher.Hash(defaultPassword);

        user.ResetPassword(passwordHash);
        user.RevokeAllRefreshTokens();
        user.IncrementGeneration();

        // 更新缓存中的世代版本，使该用户所有旧 accessToken 立即失效
        await _cache.SetAsync(CacheKeys.TokenGeneration(user.Id), user.Generation.ToString(),
            TimeSpan.FromDays(7), cancellationToken);

        _userRepository.Update(user);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new UserResult
        {
            Id = user.Id,
            Username = user.Username,
            CreatedAt = user.CreatedAt
        };
    }
}
