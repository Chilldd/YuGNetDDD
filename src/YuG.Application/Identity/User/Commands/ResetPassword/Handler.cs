using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Common.Interfaces;
using YuG.Domain.Identity.Repositories;
using YuG.Application.Identity.User.DTOs;
using UserEntity = YuG.Domain.Identity.Entities.User;

namespace YuG.Application.Identity.User.Commands.ResetPassword;

/// <summary>
/// 重置用户密码命令处理器
/// </summary>
public class Handler : IRequestHandler<ResetPasswordCommand, UserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// 初始化重置用户密码命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="passwordHasher">密码哈希服务</param>
    public Handler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
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

        // ResetPassword 内部自动递增世代版本、撤销刷新令牌、触发领域事件
        user.ResetPassword(passwordHash);

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
