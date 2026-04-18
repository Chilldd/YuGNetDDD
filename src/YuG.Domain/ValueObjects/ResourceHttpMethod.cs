namespace YuG.Domain.ValueObjects;

/// <summary>
/// 资源 HTTP 方法值对象
/// </summary>
public record ResourceHttpMethod
{
    /// <summary>
    /// GET 方法
    /// </summary>
    public static readonly ResourceHttpMethod Get = new("GET");

    /// <summary>
    /// POST 方法
    /// </summary>
    public static readonly ResourceHttpMethod Post = new("POST");

    /// <summary>
    /// PUT 方法
    /// </summary>
    public static readonly ResourceHttpMethod Put = new("PUT");

    /// <summary>
    /// DELETE 方法
    /// </summary>
    public static readonly ResourceHttpMethod Delete = new("DELETE");

    /// <summary>
    /// 方法名称
    /// </summary>
    public string Method { get; init; } = string.Empty;

    /// <summary>
    /// 根据 HTTP 方法名称创建值对象
    /// </summary>
    /// <param name="method">HTTP 方法名称</param>
    /// <returns>对应的 ResourceHttpMethod 实例</returns>
    /// <exception cref="ArgumentException">不支持的 HTTP 方法</exception>
    public static ResourceHttpMethod FromString(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "GET" => Get,
            "POST" => Post,
            "PUT" => Put,
            "DELETE" => Delete,
            _ => throw new ArgumentException($"不支持的 HTTP 方法: {method}")
        };
    }

    /// <summary>
    /// 私有构造函数，防止外部直接创建
    /// </summary>
    /// <param name="method">HTTP 方法名称</param>
    private ResourceHttpMethod(string method)
    {
        Method = method;
    }

    /// <summary>
    /// 重写 ToString，返回 HTTP 方法名称
    /// </summary>
    public override string ToString() => Method;
}
