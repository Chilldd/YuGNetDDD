using MediatR;
using YuG.Domain.Common;
using YuG.Domain.Identity.Repositories;
using YuG.Application.Identity.Role.DTOs;
using RoleEntity = YuG.Domain.Identity.Entities.Role;

namespace YuG.Application.Identity.Role.Commands.Create;

/// <summary>
/// 创建角色命令处理器
/// </summary>
public class Handler : IRequestHandler<CreateRoleCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化创建角色命令处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理创建角色命令
    /// </summary>
    /// <param name="request">创建角色命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色响应</returns>
    public async Task<RoleResult> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        // 检查编码是否已存在
        if (await _roleRepository.CodeExistsAsync(request.Code, cancellationToken))
        {
            throw new DomainException($"角色编码 '{request.Code}' 已存在");
        }

        // 创建角色
        var role = new RoleEntity(request.Name, request.Code, request.Description);

        // 保存到数据库
        await _roleRepository.AddAsync(role, cancellationToken);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        // 返回响应
        return RoleResult.FromEntity(role);
    }
}
