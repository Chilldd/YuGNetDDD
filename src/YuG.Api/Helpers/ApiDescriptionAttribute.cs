namespace YuG.Api.Helpers;

/// <summary>
/// 标注 API 接口的描述信息，用于端点同步时写入 Resource.Description
/// </summary>
/// <param name="description">接口描述</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ApiDescriptionAttribute(string description) : Attribute
{
    /// <summary>
    /// 接口描述
    /// </summary>
    public string Description { get; } = description;
}
