using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Identity.Repositories;
using YuG.Application.Identity.User.DTOs;
using UserEntity = YuG.Domain.Identity.Entities.User;

namespace YuG.Application.Identity.User.Commands.Activate;

/// <summary>
/// 启用用户命令处理器
/// </summary>
public class Handler : IRequestHandler<ActivateUserCommand, UserResult>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// 初始化启用用户命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    public Handler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// 处理启用用户命令
    /// </summary>
    /// <param name="request">启用用户命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户响应</returns>
    public async Task<UserResult> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException(nameof(UserEntity), request.Id);
        }

        user.Activate();

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
