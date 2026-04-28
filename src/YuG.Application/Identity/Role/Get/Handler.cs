using MediatR;
using YuG.Domain.Identity.Repositories;

namespace YuG.Application.Identity.Role.Get;

/// <summary>
/// 获取单个角色查询处理器
/// </summary>
public class Handler : IRequestHandler<GetRoleQuery, GetRoleResult?>
{
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化获取单个角色查询处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理获取单个角色查询
    /// </summary>
    /// <param name="query">获取单个角色查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色查询结果，不存在则返回 null</returns>
    public async Task<GetRoleResult?> Handle(GetRoleQuery query, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdWithResourcesAsync(query.Id, cancellationToken);
        if (role is null)
        {
            return null;
        }

        return new GetRoleResult
        {
            Id = role.Id,
            Name = role.Name,
            Code = role.Code,
            Description = role.Description,
            Status = role.Status.ToString(),
            ResourceIds = role.Resources.Select(r => r.Id).ToList(),
            CreatedAt = role.CreatedAt,
            UpdatedAt = role.UpdatedAt
        };
    }
}
