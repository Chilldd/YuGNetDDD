namespace YuG.Domain.Common.Constants;

/// <summary>
/// 系统角色编码常量
/// </summary>
public static class RoleCodes
{
    /// <summary>
    /// 超级管理员 — 拥有所有权限，不受角色-资源关联限制
    /// </summary>
    public const string SuperAdmin = "superadmin";

    /// <summary>
    /// 管理员
    /// </summary>
    public const string Admin = "admin";

    /// <summary>
    /// 普通用户
    /// </summary>
    public const string User = "user";
}
