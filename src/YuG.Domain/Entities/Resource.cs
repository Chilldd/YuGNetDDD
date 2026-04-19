using YuG.Domain.Common;
using YuG.Domain.Enums;

namespace YuG.Domain.Entities;

/// <summary>
/// 资源领域对象
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
    /// HTTP 方法
    /// </summary>
    public ResourceHttpMethod HttpMethod { get; private set; } = ResourceHttpMethod.Get;

    /// <summary>
    /// API 路径（如 /api/users）
    /// </summary>
    public string Path { get; private set; } = string.Empty;

    /// <summary>
    /// 父级资源标识（支持资源树结构）
    /// </summary>
    public Guid? ParentId { get; private set; }

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
    /// <param name="description">资源描述</param>
    /// <param name="httpMethod">HTTP 方法</param>
    /// <param name="path">API 路径</param>
    /// <param name="parentId">父级资源标识</param>
    /// <param name="sortOrder">排序顺序</param>
    /// <param name="status">资源状态</param>
    public Resource(
        string name,
        string code,
        string? description,
        ResourceHttpMethod httpMethod,
        string path,
        Guid? parentId,
        int sortOrder,
        ResourceStatus status)
    {
        ValidateBasicInfo(name, code, description);
        Name = name.Trim();
        Code = code.Trim();
        Description = description ?? string.Empty;

        ValidateEndpoint(path);
        HttpMethod = httpMethod;
        Path = path.Trim();

        // 初始设置父级不触发移动事件
        if (parentId == Id)
        {
            throw new DomainException("不能将资源设置为自己的子资源");
        }
        ParentId = parentId;

        SortOrder = sortOrder;
        ChangeStatus(status);
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
    /// 修改 API 端点信息
    /// </summary>
    /// <param name="path">API 路径</param>
    /// <param name="httpMethod">HTTP 方法</param>
    public void ChangeEndpoint(string path, ResourceHttpMethod httpMethod)
    {
        ValidateEndpoint(path);
        Path = path.Trim();
        HttpMethod = httpMethod;
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

        // 编码只允许字母、数字、下划线和短横线
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
    /// 移动资源到新的父级下
    /// </summary>
    /// <param name="parentId">新的父级资源标识</param>
    public void MoveTo(Guid? parentId)
    {
        // 不能将资源设置为自己的子资源
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
    /// 变更资源状态（内部使用，外部通过 Activate/Disable 控制状态机）
    /// </summary>
    /// <param name="status">新状态</param>
    private void ChangeStatus(ResourceStatus status)
    {
        Status = status;
    }
}
