namespace YuG.Domain.ValueObjects;

/// <summary>
/// 资源状态值对象
/// </summary>
public record ResourceStatus
{
    /// <summary>
    /// 激活状态
    /// </summary>
    public static readonly ResourceStatus Active = new("Active");

    /// <summary>
    /// 禁用状态
    /// </summary>
    public static readonly ResourceStatus Disabled = new("Disabled");

    /// <summary>
    /// 状态名称
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// 根据 HTTP 方法名称创建值对象
    /// </summary>
    /// <param name="status">状态名称</param>
    /// <returns>对应的 ResourceStatus 实例</returns>
    /// <exception cref="ArgumentException">不支持的状态</exception>
    public static ResourceStatus FromString(string status)
    {
        return status switch
        {
            "Active" => Active,
            "Disabled" => Disabled,
            _ => throw new ArgumentException($"不支持的状态: {status}")
        };
    }

    /// <summary>
    /// 私有构造函数，防止外部直接创建
    /// </summary>
    /// <param name="status">状态名称</param>
    private ResourceStatus(string status)
    {
        Status = status;
    }

    /// <summary>
    /// 重写 ToString，返回状态名称
    /// </summary>
    public override string ToString() => Status;

    /// <summary>
    /// 判断是否为激活状态
    /// </summary>
    public bool IsActive() => this == Active;

    /// <summary>
    /// 判断是否为禁用状态
    /// </summary>
    public bool IsDisabled() => this == Disabled;
}
