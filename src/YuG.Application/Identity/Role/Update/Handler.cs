using MediatR;
using YuG.Application.Common.Exceptions;
using YuG.Domain.Common;
using YuG.Domain.Identity.Repositories;
using RoleResult = YuG.Application.Identity.Role.Create.RoleResult;
using RoleEntity = YuG.Domain.Identity.Entities.Role;

namespace YuG.Application.Identity.Role.Update;

/// <summary>
/// 更新角色命令处理器
/// </summary>
public class Handler : IRequestHandler<UpdateRoleCommand, RoleResult>
{
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化更新角色命令处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理更新角色命令
    /// </summary>
    /// <param name="request">更新角色命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色响应</returns>
    public async Task<RoleResult> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        // 获取角色
        var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role is null)
        {
            throw new NotFoundException(nameof(RoleEntity), request.Id);
        }

        // 检查编码唯一性（如果编码改变）
        if (role.Code != request.Code && await _roleRepository.CodeExistsAsync(request.Code, cancellationToken))
        {
            throw new DomainException($"角色编码 '{request.Code}' 已存在");
        }

        // 更新角色信息
        if (role.Name != request.Name)
        {
            role.Rename(request.Name);
        }

        if (role.Code != request.Code)
        {
            role.ChangeCode(request.Code);
        }

        if (role.Description != request.Description)
        {
            role.ChangeDescription(request.Description);
        }

        // 保存到数据库
        _roleRepository.Update(role);
        await _roleRepository.SaveChangesAsync(cancellationToken);

        // 返回响应
        return Create.Handler.MapToResult(role);
    }
}
