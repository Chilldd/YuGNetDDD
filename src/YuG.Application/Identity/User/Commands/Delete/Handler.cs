using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Identity.Repositories;
using UserEntity = YuG.Domain.Identity.Entities.User;

namespace YuG.Application.Identity.User.Commands.Delete;

/// <summary>
/// 删除用户命令处理器
/// </summary>
public class Handler : IRequestHandler<DeleteUserCommand>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// 初始化删除用户命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    public Handler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// 处理删除用户命令
    /// </summary>
    /// <param name="request">删除用户命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException(nameof(UserEntity), request.Id);
        }

        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync(cancellationToken);
    }
}
