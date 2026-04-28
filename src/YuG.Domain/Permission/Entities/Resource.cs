using YuG.Domain.Common;
using YuG.Domain.Permission.Enums;

namespace YuG.Domain.Permission.Entities;

/// <summary>
/// 资源领域对象（支持菜单、API、按钮三种资源类型）
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
    /// 前端路由（仅菜单类型资源有效）
    /// </summary>
    public string? Route { get; private set; }

    /// <summary>
    /// 组件路径（仅菜单类型资源有效）
    /// </summary>
    public string? Component { get; private set; }

    /// <summary>
    /// 是否隐藏（仅菜单类型资源有效，用于隐藏菜单但保留路由）
    /// </summary>
    public bool IsHidden { get; private set; }

    /// <summary>
    /// 菜单角标（仅菜单类型资源有效）
    /// </summary>
    public string? Badge { get; private set; }

    /// <summary>
    /// 权限编码（仅按钮类型资源有效，如 user:create）
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
    /// <param name="component">组件路径（可选）</param>
    /// <param name="isHidden">是否隐藏（可选）</param>
    /// <param name="badge">菜单角标（可选）</param>
    public void ConfigureMenu(string? icon, string? route, string? component, bool? isHidden, string? badge)
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

        if (component?.Length > 500)
        {
            throw new DomainException("组件路径长度不能超过 500 个字符");
        }

        if (badge?.Length > 50)
        {
            throw new DomainException("菜单角标长度不能超过 50 个字符");
        }

        Icon = icon;
        Route = route;
        Component = component;
        if (isHidden.HasValue)
        {
            IsHidden = isHidden.Value;
        }
        Badge = badge;
    }

    /// <summary>
    /// 配置按钮权限编码（仅按钮类型可调用）
    /// </summary>
    /// <param name="permissionCode">权限编码（如 user:create）</param>
    public void ConfigureButton(string? permissionCode)
    {
        if (Type != ResourceType.Button)
        {
            throw new DomainException("只有按钮类型资源可以配置权限编码");
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
