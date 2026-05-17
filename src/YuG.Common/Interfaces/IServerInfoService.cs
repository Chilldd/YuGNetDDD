namespace YuG.Common.Interfaces;

/// <summary>
/// 服务器信息
/// </summary>
public record ServerInfo
{
    /// <summary>
    /// 主机名
    /// </summary>
    public string Hostname { get; init; } = string.Empty;

    /// <summary>
    /// 操作系统描述
    /// </summary>
    public string OsDescription { get; init; } = string.Empty;

    /// <summary>
    /// 操作系统架构
    /// </summary>
    public string OsArchitecture { get; init; } = string.Empty;

    /// <summary>
    /// 进程架构
    /// </summary>
    public string ProcessArchitecture { get; init; } = string.Empty;

    /// <summary>
    /// 运行时版本
    /// </summary>
    public string RuntimeVersion { get; init; } = string.Empty;

    /// <summary>
    /// 运行时标识
    /// </summary>
    public string RuntimeIdentifier { get; init; } = string.Empty;

    /// <summary>
    /// CPU 核心数
    /// </summary>
    public int CpuCores { get; init; }

    /// <summary>
    /// 内存信息
    /// </summary>
    public MemoryInfo Memory { get; init; } = new();

    /// <summary>
    /// 磁盘信息
    /// </summary>
    public DiskInfo Disk { get; init; } = new();

    /// <summary>
    /// 系统运行时长
    /// </summary>
    public TimeSpan Uptime { get; init; }

    /// <summary>
    /// 应用程序名称
    /// </summary>
    public string ApplicationName { get; init; } = string.Empty;

    /// <summary>
    /// 应用程序版本
    /// </summary>
    public string ApplicationVersion { get; init; } = string.Empty;

    /// <summary>
    /// 运行环境（Development / Staging / Production）
    /// </summary>
    public string Environment { get; init; } = string.Empty;

    /// <summary>
    /// 进程启动时间
    /// </summary>
    public DateTime StartTime { get; init; }
}

/// <summary>
/// 内存信息
/// </summary>
public record MemoryInfo
{
    /// <summary>
    /// 总内存（MB）
    /// </summary>
    public long TotalMB { get; init; }

    /// <summary>
    /// 已用内存（MB）
    /// </summary>
    public long UsedMB { get; init; }

    /// <summary>
    /// 可用内存（MB）
    /// </summary>
    public long AvailableMB { get; init; }

    /// <summary>
    /// 内存使用率（百分比）
    /// </summary>
    public double UsagePercent { get; init; }
}

/// <summary>
/// 磁盘信息
/// </summary>
public record DiskInfo
{
    /// <summary>
    /// 总空间（GB）
    /// </summary>
    public double TotalGB { get; init; }

    /// <summary>
    /// 已用空间（GB）
    /// </summary>
    public double UsedGB { get; init; }

    /// <summary>
    /// 可用空间（GB）
    /// </summary>
    public double AvailableGB { get; init; }

    /// <summary>
    /// 磁盘使用率（百分比）
    /// </summary>
    public double UsagePercent { get; init; }
}

/// <summary>
/// 服务器信息服务接口
/// </summary>
public interface IServerInfoService
{
    /// <summary>
    /// 获取服务器信息
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>服务器信息</returns>
    Task<ServerInfo> GetServerInfoAsync(CancellationToken cancellationToken = default);
}
