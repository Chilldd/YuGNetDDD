using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Identity.Repositories;
using UserResult = YuG.Application.Identity.User.Commands.Create.UserResult;
using UserEntity = YuG.Domain.Identity.Entities.User;

namespace YuG.Application.Identity.User.Commands.Disable;

/// <summary>
/// 禁用用户命令处理器
/// </summary>
public class Handler : IRequestHandler<DisableUserCommand, UserResult>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// 初始化禁用用户命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    public Handler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// 处理禁用用户命令
    /// </summary>
    /// <param name="request">禁用用户命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户响应</returns>
    public async Task<UserResult> Handle(DisableUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException(nameof(UserEntity), request.Id);
        }

        user.Disable();

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
