namespace YuG.Application.Permission.UserPermission.GetPageApiPermissions;

/// <summary>
/// 获取页面 API 权限响应
/// </summary>
public record GetPageApiPermissionsResult
{
    /// <summary>
    /// 页面拥有的 API 权限编码列表
    /// </summary>
    public IReadOnlyList<string> PermissionCodes { get; init; } = [];
}
