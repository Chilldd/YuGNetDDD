using YuG.Application.Permission.Resource.SyncApiEndpoints;

namespace YuG.Api.Helpers;

/// <summary>
/// API 端点扫描器接口
/// </summary>
public interface IApiEndpointScanner
{
    /// <summary>
    /// 扫描应用程序中的所有 API 端点和控制器
    /// </summary>
    /// <returns>扫描结果，包含控制器和端点</returns>
    DiscoveredApiScanResult Scan();
}
