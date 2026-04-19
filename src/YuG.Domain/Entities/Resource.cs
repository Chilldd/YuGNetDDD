using YuG.Domain.Common;
using YuG.Domain.ValueObjects;

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
        string description,
        ResourceHttpMethod httpMethod,
        string path,
        Guid? parentId,
        int sortOrder,
        ResourceStatus status)
    {
        UpdateName(name);
        UpdateCode(code);
        UpdateDescription(description);
        UpdateHttpMethod(httpMethod);
        UpdatePath(path);
        UpdateParentId(parentId);
        UpdateSortOrder(sortOrder);
        UpdateStatus(status);
    }

    /// <summary>
    /// 更新资源名称
    /// </summary>
    /// <param name="name">新名称</param>
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("资源名称不能为空");
        }

        if (name.Length > 200)
        {
            throw new DomainException("资源名称长度不能超过 200 个字符");
        }

        Name = name.Trim();
    }

    /// <summary>
    /// 更新资源编码
    /// </summary>
    /// <param name="code">新编码</param>
    public void UpdateCode(string code)
    {
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

        Code = code.Trim();
    }

    /// <summary>
    /// 更新资源描述
    /// </summary>
    /// <param name="description">新描述</param>
    public void UpdateDescription(string description)
    {
        if (description.Length > 500)
        {
            throw new DomainException("资源描述长度不能超过 500 个字符");
        }

        Description = description;
    }

    /// <summary>
    /// 更新 HTTP 方法
    /// </summary>
    /// <param name="httpMethod">新 HTTP 方法</param>
    public void UpdateHttpMethod(ResourceHttpMethod httpMethod)
    {
        HttpMethod = httpMethod;
    }

    /// <summary>
    /// 更新 API 路径
    /// </summary>
    /// <param name="path">新路径</param>
    public void UpdatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new DomainException("API 路径不能为空");
        }

        if (path.Length > 500)
        {
            throw new DomainException("API 路径长度不能超过 500 个字符");
        }

        Path = path.Trim();
    }

    /// <summary>
    /// 更新父级资源标识
    /// </summary>
    /// <param name="parentId">新的父级资源标识</param>
    public void UpdateParentId(Guid? parentId)
    {
        // 不能将资源设置为自己的子资源
        if (parentId == Id)
        {
            throw new DomainException("不能将资源设置为自己的子资源");
        }

        ParentId = parentId;
    }

    /// <summary>
    /// 更新排序顺序
    /// </summary>
    /// <param name="sortOrder">新的排序顺序</param>
    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 更新资源状态
    /// </summary>
    /// <param name="status">新状态</param>
    public void UpdateStatus(ResourceStatus status)
    {
        Status = status;
    }

    /// <summary>
    /// 激活资源
    /// </summary>
    public void Activate()
    {
        Status = ResourceStatus.Active;
    }

    /// <summary>
    /// 禁用资源
    /// </summary>
    public void Disable()
    {
        Status = ResourceStatus.Disabled;
    }

    /// <summary>
    /// 批量更新资源属性
    /// </summary>
    /// <param name="name">新名称</param>
    /// <param name="code">新编码</param>
    /// <param name="description">新描述</param>
    /// <param name="httpMethod">新 HTTP 方法</param>
    /// <param name="path">新路径</param>
    /// <param name="parentId">新父级资源标识</param>
    /// <param name="sortOrder">新排序顺序</param>
    /// <param name="status">新状态</param>
    public void Update(
        string name,
        string code,
        string description,
        ResourceHttpMethod httpMethod,
        string path,
        Guid? parentId,
        int sortOrder,
        ResourceStatus status)
    {
        UpdateName(name);
        UpdateCode(code);
        UpdateDescription(description);
        UpdateHttpMethod(httpMethod);
        UpdatePath(path);
        UpdateParentId(parentId);
        UpdateSortOrder(sortOrder);
        UpdateStatus(status);
    }
}
