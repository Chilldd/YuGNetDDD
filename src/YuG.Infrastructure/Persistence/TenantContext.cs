namespace YuG.Infrastructure.Persistence;

public class TenantContext
{
    /// <summary>
    /// 租户
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}