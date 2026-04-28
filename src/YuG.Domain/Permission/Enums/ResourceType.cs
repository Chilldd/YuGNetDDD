namespace YuG.Domain.Permission.Enums;

/// <summary>
/// 资源类型枚举
/// </summary>
public enum ResourceType
{
    /// <summary>
    /// 菜单资源（前端菜单、侧边栏）
    /// </summary>
    Menu = 0,

    /// <summary>
    /// API 端点资源（后端接口）
    /// </summary>
    Api = 1,

    /// <summary>
    /// 按钮资源（前端按钮、操作权限）
    /// </summary>
    Button = 2
}
