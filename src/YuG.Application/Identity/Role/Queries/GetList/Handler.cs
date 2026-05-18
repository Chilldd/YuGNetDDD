using MediatR;
using YuG.Common.Models;
using YuG.Domain.Identity.Repositories;

namespace YuG.Application.Identity.Role.Queries.GetList;

/// <summary>
/// 获取角色列表查询处理器
/// </summary>
public class Handler : IRequestHandler<GetRoleListQuery, PageResult<RoleListItem>>
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

    /// <inheritdoc />
    public async Task<PageResult<RoleListItem>> Handle(GetRoleListQuery query, CancellationToken cancellationToken)
    {
        var pageResult = await _roleRepository.GetRolesPagedAsync(query.Page, query.PageSize, cancellationToken);

        var items = pageResult.Items.Select(r => new RoleListItem
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            Description = r.Description,
            Status = r.Status.ToString(),
            IsSystem = r.IsSystem,
            CreatedAt = r.CreatedAt,
        }).ToList();

        return new PageResult<RoleListItem>
        {
            Items = items,
            TotalCount = pageResult.TotalCount,
            Page = pageResult.Page,
            PageSize = pageResult.PageSize,
        };
    }
}
