using Microsoft.AspNetCore.Authorization;

namespace YuG.Api.Authorization;

/// <summary>
/// 权限编码授权要求
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// 需要的权限编码
    /// </summary>
    public string PermissionCode { get; }

    /// <summary>
    /// 初始化权限编码授权要求
    /// </summary>
    /// <param name="permissionCode">权限编码</param>
    public PermissionRequirement(string permissionCode)
    {
        PermissionCode = permissionCode;
    }
}
