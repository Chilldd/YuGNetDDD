using YuG.Domain.Common;
using YuG.Domain.Permission.Enums;

namespace YuG.Domain.Permission.Entities;

/// <summary>
/// 资源领域对象（支持三层树形结构：菜单 Menu → 页面 Page → API Api）
/// </summary>
public class Resource : AggregateRoot
{
    /// <summary>
    /// 资源名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 资源编码（唯一，用于权限系统引用）
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 资源描述
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// 资源类型
    /// </summary>
    public ResourceType Type { get; private set; }

    /// <summary>
    /// HTTP 方法（仅 API 类型资源有效）
    /// </summary>
    public ResourceHttpMethod? HttpMethod { get; private set; }

    /// <summary>
    /// API 路径（仅 API 类型资源有效）
    /// </summary>
    public string? Path { get; private set; } = string.Empty;

    /// <summary>
    /// 菜单图标（仅菜单类型资源有效）
    /// </summary>
    public string? Icon { get; private set; }

    /// <summary>
    /// 前端路由（菜单/页面类型有效）
    /// </summary>
    public string? Route { get; private set; }

/// <summary>
    /// 是否隐藏（仅菜单类型资源有效，用于隐藏菜单但保留路由）
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// 菜单角标（仅菜单类型资源有效）
    /// </summary>
    public string? Badge { get; private set; }

    /// <summary>
    /// 权限编码（页面/API 类型有效，如 user:create、user:export）
    /// </summary>
    public string? PermissionCode { get; private set; }

    /// <summary>
    /// 父级资源标识（支持资源树结构）
    /// </summary>
    public long? ParentId { get; private set; }

    /// <summary>
    /// 排序顺序
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 资源状态
    /// </summary>
    public ResourceStatus Status { get; private set; } = ResourceStatus.Active;

    /// <summary>
    /// 创建资源（用于 ORM）
    /// </summary>
    private Resource()
    {
    }

    /// <summary>
    /// 创建新资源
    /// </summary>
    /// <param name="name">资源名称</param>
    /// <param name="code">资源编码</param>
    /// <param name="type">资源类型</param>
    /// <param name="description">资源描述（可选）</param>
    /// <param name="parentId">父级资源标识（可选）</param>
    /// <param name="sortOrder">排序顺序</param>
    /// <param name="status">资源状态</param>
    public Resource(
        string name,
        string code,
        ResourceType type,
        string? description,
        long? parentId,
        int sortOrder,
        ResourceStatus status)
    {
        ValidateBasicInfo(name, code, description);
        Name = name.Trim();
        Code = code.Trim();
        Description = description ?? string.Empty;
        Type = type;

        if (parentId == Id)
        {
            throw new DomainException("不能将资源设置为自己的子资源");
        }
        ParentId = parentId;

        SortOrder = sortOrder;
        ChangeStatus(status);
    }

    /// <summary>
    /// 配置 API 端点信息（仅 API 类型可调用）
    /// </summary>
    /// <param name="path">API 路径</param>
    /// <param name="httpMethod">HTTP 方法</param>
    public void ChangeEndpoint(string path, ResourceHttpMethod httpMethod)
    {
        if (Type != ResourceType.Api)
        {
            throw new DomainException("只有 API 类型资源可以配置端点信息");
        }

        ValidateEndpoint(path);
        Path = path.Trim();
        HttpMethod = httpMethod;
    }

    /// <summary>
    /// 配置菜单资源信息（仅菜单类型可调用）
    /// </summary>
    /// <param name="icon">菜单图标（可选）</param>
    /// <param name="route">前端路由（可选）</param>
    /// <param name="isHidden">是否隐藏（可选）</param>
    /// <param name="badge">菜单角标（可选）</param>
    public void ConfigureMenu(string? icon, string? route, bool? isHidden, string? badge)
    {
        if (Type != ResourceType.Menu)
        {
            throw new DomainException("只有菜单类型资源可以配置菜单信息");
        }

        if (icon?.Length > 100)
        {
            throw new DomainException("菜单图标长度不能超过 100 个字符");
        }

        if (route?.Length > 500)
        {
            throw new DomainException("前端路由长度不能超过 500 个字符");
        }

if (badge?.Length > 50)
        {
            throw new DomainException("菜单角标长度不能超过 50 个字符");
        }

        Icon = icon;
        Route = route;
        if (isHidden.HasValue)
        {
            IsHidden = isHidden.Value;
        }
        Badge = badge;
    }

    /// <summary>
    /// 配置页面资源信息（仅页面类型可调用）
    /// </summary>
    /// <param name="route">前端路由（可选）</param>
    /// <param name="permissionCode">页面权限编码（可选，如 user:manage）</param>
    public void ConfigurePage(string? route, string? permissionCode)
    {
        if (Type != ResourceType.Page)
        {
            throw new DomainException("只有页面类型资源可以配置页面信息");
        }

        if (route?.Length > 500)
        {
            throw new DomainException("前端路由长度不能超过 500 个字符");
        }

if (permissionCode?.Length > 100)
        {
            throw new DomainException("权限编码长度不能超过 100 个字符");
        }

        Route = route;
        PermissionCode = permissionCode;
    }

    /// <summary>
    /// 配置 API 权限编码（仅 API 类型可调用）
    /// </summary>
    /// <param name="permissionCode">权限编码（如 user:create）</param>
    public void ConfigureApiPermission(string? permissionCode)
    {
        if (Type != ResourceType.Api)
        {
            throw new DomainException("只有 API 类型资源可以配置权限编码");
        }

        if (permissionCode?.Length > 100)
        {
            throw new DomainException("权限编码长度不能超过 100 个字符");
        }

        PermissionCode = permissionCode;
    }

    /// <summary>
    /// 重命名资源
    /// </summary>
    /// <param name="newName">新名称</param>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new DomainException("资源名称不能为空");
        }

        if (newName.Length > 200)
        {
            throw new DomainException("资源名称长度不能超过 200 个字符");
        }

        Name = newName.Trim();
    }

    /// <summary>
    /// 修改资源编码
    /// </summary>
    /// <param name="newCode">新编码</param>
    public void ChangeCode(string newCode)
    {
        if (string.IsNullOrWhiteSpace(newCode))
        {
            throw new DomainException("资源编码不能为空");
        }

        if (newCode.Length > 100)
        {
            throw new DomainException("资源编码长度不能超过 100 个字符");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(newCode, @"^[a-zA-Z0-9_-]+$"))
        {
            throw new DomainException("资源编码只能包含字母、数字、下划线和短横线");
        }

        Code = newCode.Trim();
    }

    /// <summary>
    /// 修改资源描述
    /// </summary>
    /// <param name="newDescription">新描述</param>
    public void ChangeDescription(string? newDescription)
    {
        if (newDescription?.Length > 500)
        {
            throw new DomainException("资源描述长度不能超过 500 个字符");
        }

        Description = newDescription ?? string.Empty;
    }

    /// <summary>
    /// 移动资源到新的父级下
    /// </summary>
    /// <param name="parentId">新的父级资源标识</param>
    public void MoveTo(long? parentId)
    {
        if (parentId == Id)
        {
            throw new DomainException("不能将资源设置为自己的子资源");
        }

        if (ParentId != parentId)
        {
            ParentId = parentId;
        }
    }

    /// <summary>
    /// 验证资源父子类型层级关系
    /// </summary>
    /// <param name="childType">子资源类型</param>
    /// <param name="parentType">父级资源类型（null 表示根级别）</param>
    /// <exception cref="DomainException">层级关系不合法时抛出</exception>
    public static void ValidateParentChildType(ResourceType childType, ResourceType? parentType)
    {
        if (parentType is null)
        {
            if (childType != ResourceType.Menu && childType != ResourceType.Page)
            {
                throw new DomainException("根级别资源只能是 Menu 或 Page 类型");
            }
            return;
        }

        switch (parentType.Value)
        {
            case ResourceType.Menu:
                if (childType != ResourceType.Menu && childType != ResourceType.Page)
                {
                    throw new DomainException("Menu 类型的子资源只能是 Menu 或 Page 类型");
                }
                break;

            case ResourceType.Page:
                if (childType != ResourceType.Api)
                {
                    throw new DomainException("Page 类型的子资源只能是 Api 类型");
                }
                break;

            case ResourceType.Api:
                throw new DomainException("Api 类型资源不能作为父级");
        }
    }

    /// <summary>
    /// 变更排序顺序
    /// </summary>
    /// <param name="sortOrder">新的排序顺序</param>
    public void ChangeSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 激活资源
    /// </summary>
    public void Activate()
    {
        ChangeStatus(ResourceStatus.Active);
    }

    /// <summary>
    /// 禁用资源
    /// </summary>
    public void Disable()
    {
        ChangeStatus(ResourceStatus.Disabled);
    }

    /// <summary>
    /// 根据端点发现结果同步更新资源属性。返回是否有变更。
    /// </summary>
    /// <param name="name">资源名称</param>
    /// <param name="code">资源编码</param>
    /// <param name="description">资源描述</param>
    /// <param name="path">API 路径</param>
    /// <param name="httpMethod">HTTP 方法</param>
    /// <param name="permissionCode">权限编码</param>
    /// <returns>是否有属性发生变更</returns>
    public bool SyncFromEndpoints(
        string name,
        string code,
        string description,
        string path,
        ResourceHttpMethod httpMethod,
        string? permissionCode)
    {
        var changed = false;

        if (Name != name) { Rename(name); changed = true; }
        if (Code != code) { ChangeCode(code); changed = true; }
        if (Description != description) { ChangeDescription(description); changed = true; }
        if (Path != path || HttpMethod != httpMethod) { ChangeEndpoint(path, httpMethod); changed = true; }
        if (PermissionCode != permissionCode) { ConfigureApiPermission(permissionCode); changed = true; }

        return changed;
    }

    /// <summary>
    /// 获取资源的所有后代列表，按从深到浅的顺序排列（叶子在前，适合删除）
    /// </summary>
    /// <param name="resourceId">资源标识</param>
    /// <param name="allResources">全部资源列表</param>
    /// <returns>后代资源列表，叶子节点在前</returns>
    public static IReadOnlyList<Resource> GetDescendantsInDeleteOrder(
        long resourceId,
        IReadOnlyCollection<Resource> allResources)
    {
        var childrenMap = allResources
            .Where(r => r.ParentId.HasValue)
            .GroupBy(r => r.ParentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<Resource>();
        CollectDescendants(resourceId, childrenMap, result);
        return result;
    }

    /// <summary>
    /// 验证移动资源不会导致循环引用
    /// </summary>
    /// <param name="resourceId">要移动的资源标识</param>
    /// <param name="targetParentId">目标父级标识（null 表示根级别）</param>
    /// <param name="allResources">全部资源列表</param>
    /// <exception cref="DomainException">会导致循环引用时抛出</exception>
    public static void ValidateNoCircularReference(
        long resourceId,
        long? targetParentId,
        IReadOnlyCollection<Resource> allResources)
    {
        if (!targetParentId.HasValue) return;

        var descendants = GetDescendantsInDeleteOrder(resourceId, allResources);
        if (descendants.Any(d => d.Id == targetParentId.Value))
        {
            throw new DomainException("不能将资源移动到自己或自己的子资源下");
        }
    }

    /// <summary>
    /// 递归收集所有子资源（深度优先，子节点在结果中按从深到浅排列）
    /// </summary>
    private static void CollectDescendants(
        long parentId,
        Dictionary<long, List<Resource>> childrenMap,
        List<Resource> result)
    {
        if (!childrenMap.TryGetValue(parentId, out var children)) return;

        foreach (var child in children)
        {
            CollectDescendants(child.Id, childrenMap, result);
            result.Add(child);
        }
    }

    /// <summary>
    /// 验证基础信息
    /// </summary>
    private static void ValidateBasicInfo(string name, string code, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("资源名称不能为空");
        }

        if (name.Length > 200)
        {
            throw new DomainException("资源名称长度不能超过 200 个字符");
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("资源编码不能为空");
        }

        if (code.Length > 100)
        {
            throw new DomainException("资源编码长度不能超过 100 个字符");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(code, @"^[a-zA-Z0-9_-]+$"))
        {
            throw new DomainException("资源编码只能包含字母、数字、下划线和短横线");
        }

        if (description?.Length > 500)
        {
            throw new DomainException("资源描述长度不能超过 500 个字符");
        }
    }

    /// <summary>
    /// 验证端点信息
    /// </summary>
    private static void ValidateEndpoint(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new DomainException("API 路径不能为空");
        }

        if (path.Length > 500)
        {
            throw new DomainException("API 路径长度不能超过 500 个字符");
        }
    }

    /// <summary>
    /// 变更资源状态（内部使用，外部通过 Activate/Disable 控制状态机）
    /// </summary>
    private void ChangeStatus(ResourceStatus status)
    {
        Status = status;
    }
}
