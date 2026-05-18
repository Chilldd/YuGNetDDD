using MediatR;
using YuG.Domain.Common;
using YuG.Common.Interfaces;
using YuG.Domain.Identity.Repositories;
using UserEntity = YuG.Domain.Identity.Entities.User;

namespace YuG.Application.Identity.User.Commands.Create;

/// <summary>
/// 创建用户命令处理器
/// </summary>
public class Handler : IRequestHandler<CreateUserCommand, UserResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// 初始化创建用户命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="passwordHasher">密码哈希服务</param>
    public Handler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 处理创建用户命令
    /// </summary>
    /// <param name="request">创建用户命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户响应</returns>
    public async Task<UserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // 检查用户名是否已存在
        if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
        {
            throw new DomainException($"用户名 '{request.Username}' 已存在");
        }

        // 哈希密码
        var passwordHash = _passwordHasher.Hash(request.Password);

        // 创建用户
        var user = new UserEntity(request.Username, passwordHash);

        // 保存到数据库
        await _userRepository.AddAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        return new UserResult
        {
            Id = user.Id,
            Username = user.Username,
            CreatedAt = user.CreatedAt
        };
    }
}
