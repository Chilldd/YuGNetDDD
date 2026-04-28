using YuG.Domain.Common;
using YuG.Domain.Identity.Enums;
using YuG.Domain.Permission.Entities;

namespace YuG.Domain.Identity.Entities;

/// <summary>
/// 角色领域对象（RBAC 核心聚合根）
/// </summary>
public class Role : AggregateRoot
{
    private readonly List<User> _users = [];
    private readonly List<Resource> _resources = [];

    /// <summary>
    /// 角色名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 角色编码（唯一，用于权限系统引用）
    /// </summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>
    /// 角色描述
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// 角色状态
    /// </summary>
    public RoleStatus Status { get; private set; } = RoleStatus.Active;

    /// <summary>
    /// 关联的用户集合（多对多，仅用于 EF Core 映射）
    /// </summary>
    public IReadOnlyCollection<User> Users => _users.AsReadOnly();

    /// <summary>
    /// 关联的资源集合（多对多）
    /// </summary>
    public IReadOnlyCollection<Resource> Resources => _resources.AsReadOnly();

    /// <summary>
    /// 创建角色（用于 ORM）
    /// </summary>
    private Role()
    {
    }

    /// <summary>
    /// 创建新角色
    /// </summary>
    /// <param name="name">角色名称</param>
    /// <param name="code">角色编码</param>
    /// <param name="description">角色描述（可选）</param>
    public Role(string name, string code, string? description)
    {
        ValidateName(name);
        ValidateCode(code);

        Name = name.Trim();
        Code = code.Trim();
        Description = description?.Trim();
        Status = RoleStatus.Active;
    }

    /// <summary>
    /// 重命名角色
    /// </summary>
    /// <param name="newName">新名称</param>
    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            throw new DomainException("角色名称不能为空");
        }

        if (newName.Length > 100)
        {
            throw new DomainException("角色名称长度不能超过 100 个字符");
        }

        Name = newName.Trim();
    }

    /// <summary>
    /// 修改角色编码
    /// </summary>
    /// <param name="newCode">新编码</param>
    public void ChangeCode(string newCode)
    {
        ValidateCode(newCode);
        Code = newCode.Trim();
    }

    /// <summary>
    /// 修改角色描述
    /// </summary>
    /// <param name="newDescription">新描述</param>
    public void ChangeDescription(string? newDescription)
    {
        if (newDescription?.Length > 500)
        {
            throw new DomainException("角色描述长度不能超过 500 个字符");
        }

        Description = newDescription?.Trim();
    }

    /// <summary>
    /// 激活角色
    /// </summary>
    public void Activate()
    {
        Status = RoleStatus.Active;
    }

    /// <summary>
    /// 禁用角色
    /// </summary>
    public void Disable()
    {
        Status = RoleStatus.Disabled;
    }

    /// <summary>
    /// 分配资源到角色
    /// </summary>
    /// <param name="resource">资源</param>
    public void AssignResource(Resource resource)
    {
        if (resource is null)
        {
            throw new DomainException("资源不能为空");
        }

        if (!_resources.Any(r => r.Id == resource.Id))
        {
            _resources.Add(resource);
        }
    }

    /// <summary>
    /// 从角色移除资源
    /// </summary>
    /// <param name="resource">资源</param>
    public void UnassignResource(Resource resource)
    {
        if (resource is null)
        {
            throw new DomainException("资源不能为空");
        }

        _resources.RemoveAll(r => r.Id == resource.Id);
    }

    /// <summary>
    /// 清空角色所有资源
    /// </summary>
    public void ClearResources()
    {
        _resources.Clear();
    }

    /// <summary>
    /// 验证角色名称
    /// </summary>
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("角色名称不能为空");
        }

        if (name.Length > 100)
        {
            throw new DomainException("角色名称长度不能超过 100 个字符");
        }
    }

    /// <summary>
    /// 验证角色编码
    /// </summary>
    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException("角色编码不能为空");
        }

        if (code.Length > 100)
        {
            throw new DomainException("角色编码长度不能超过 100 个字符");
        }

        if (!System.Text.RegularExpressions.Regex.IsMatch(code, @"^[a-zA-Z0-9_-]+$"))
        {
            throw new DomainException("角色编码只能包含字母、数字、下划线和短横线");
        }
    }
}
