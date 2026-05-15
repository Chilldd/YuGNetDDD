using System.Diagnostics;
using System.Runtime.InteropServices;
using YuG.Domain.Common.Interfaces;

namespace YuG.Infrastructure.Services;

/// <summary>
/// 服务器信息服务实现
/// </summary>
public class ServerInfoService : IServerInfoService
{
    private readonly string _hostname;
    private readonly DateTime _processStartTime;
    private readonly string _applicationName;
    private readonly string _applicationVersion;
    private readonly string _environment;

    /// <summary>
    /// 初始化服务器信息服务
    /// </summary>
    public ServerInfoService()
    {
        _hostname = Environment.MachineName;
        _processStartTime = Process.GetCurrentProcess().StartTime;

        var assembly = System.Reflection.Assembly.GetEntryAssembly();
        _applicationName = assembly?.GetName().Name ?? "Unknown";
        _applicationVersion = assembly?.GetName().Version?.ToString() ?? "0.0.0.0";
        _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                       ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                       ?? "Production";
    }

    /// <inheritdoc />
    public Task<ServerInfo> GetServerInfoAsync(CancellationToken cancellationToken = default)
    {
        var uptime = DateTime.UtcNow - _processStartTime.ToUniversalTime();

        return Task.FromResult(new ServerInfo
        {
            Hostname = _hostname,
            OsDescription = RuntimeInformation.OSDescription,
            OsArchitecture = RuntimeInformation.OSArchitecture.ToString(),
            ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
            RuntimeVersion = RuntimeInformation.FrameworkDescription,
            RuntimeIdentifier = RuntimeInformation.RuntimeIdentifier,
            CpuCores = Environment.ProcessorCount,
            Memory = GetMemoryInfo(),
            Disk = GetDiskInfo(),
            Uptime = uptime,
            ApplicationName = _applicationName,
            ApplicationVersion = _applicationVersion,
            Environment = _environment,
            StartTime = _processStartTime
        });
    }

    /// <summary>
    /// 获取内存信息
    /// </summary>
    private static MemoryInfo GetMemoryInfo()
    {
        // 获取进程内存信息作为已用内存参考
        var process = Process.GetCurrentProcess();
        var usedBytes = process.WorkingSet64;

        // 获取总可用内存（GC 可用的内存）
        var gcInfo = GC.GetGCMemoryInfo();
        var totalBytes = gcInfo.TotalAvailableMemoryBytes;

        // 如果 TotalAvailableMemoryBytes 为 0（某些环境不支持），退化为进程内存
        long totalMB, usedMB, availableMB;
        double usagePercent;

        if (totalBytes > 0)
        {
            totalMB = totalBytes / (1024 * 1024);
            usedMB = usedBytes / (1024 * 1024);
            availableMB = totalMB - usedMB;
            usagePercent = totalMB > 0 ? Math.Round((double)usedMB / totalMB * 100, 1) : 0;
        }
        else
        {
            totalMB = usedMB = usedBytes / (1024 * 1024);
            availableMB = 0;
            usagePercent = 0;
        }

        return new MemoryInfo
        {
            TotalMB = totalMB,
            UsedMB = usedMB,
            AvailableMB = availableMB,
            UsagePercent = usagePercent
        };
    }

    /// <summary>
    /// 获取磁盘信息
    /// </summary>
    private static DiskInfo GetDiskInfo()
    {
        var appDirectory = AppContext.BaseDirectory;

        // 找到应用所在磁盘的根路径
        var rootPath = Path.GetPathRoot(appDirectory) ?? "/";

        var drives = DriveInfo.GetDrives();
        var targetDrive = Array.Find(drives, d =>
            d.IsReady && d.Name.Equals(rootPath, StringComparison.OrdinalIgnoreCase));

        if (targetDrive is null || !targetDrive.IsReady)
        {
            return new DiskInfo();
        }

        var totalGB = Math.Round(targetDrive.TotalSize / (double)(1024 * 1024 * 1024), 1);
        var availableGB = Math.Round(targetDrive.AvailableFreeSpace / (double)(1024 * 1024 * 1024), 1);
        var usedGB = Math.Round(totalGB - availableGB, 1);
        var usagePercent = totalGB > 0 ? Math.Round(usedGB / totalGB * 100, 1) : 0;

        return new DiskInfo
        {
            TotalGB = totalGB,
            UsedGB = usedGB,
            AvailableGB = availableGB,
            UsagePercent = usagePercent
        };
    }
}
