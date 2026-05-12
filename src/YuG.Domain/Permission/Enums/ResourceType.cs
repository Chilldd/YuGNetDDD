namespace YuG.Domain.Permission.Enums;

/// <summary>
/// 资源类型枚举（三层树形结构：Menu → Page → Api）
/// </summary>
public enum ResourceType
{
    /// <summary>
    /// 菜单资源（第一层：前端导航菜单）
    /// </summary>
    Menu = 0,

    /// <summary>
    /// API 资源（第三层：后端接口端点）
    /// </summary>
    Api = 1,

    /// <summary>
    /// 页面资源（第二层：前端页面/功能模块）
    /// </summary>
    Page = 2
}
