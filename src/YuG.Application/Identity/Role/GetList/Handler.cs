using MediatR;
using YuG.Domain.Identity.Repositories;

namespace YuG.Application.Identity.Role.GetList;

/// <summary>
/// 获取角色列表查询处理器
/// </summary>
public class Handler : IRequestHandler<GetRoleListQuery, GetRoleListResult>
{
    private readonly IRoleRepository _roleRepository;

    /// <summary>
    /// 初始化获取角色列表查询处理器
    /// </summary>
    /// <param name="roleRepository">角色仓储</param>
    public Handler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    /// <summary>
    /// 处理获取角色列表查询
    /// </summary>
    /// <param name="query">获取角色列表查询</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>角色列表结果</returns>
    public async Task<GetRoleListResult> Handle(GetRoleListQuery query, CancellationToken cancellationToken)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        var totalCount = roles.Count;

        var items = roles.Select(r => new RoleListItem
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            Description = r.Description,
            Status = r.Status.ToString(),
            CreatedAt = r.CreatedAt
        }).ToList();

        return new GetRoleListResult
        {
            Items = items,
            TotalCount = totalCount
        };
    }
}
