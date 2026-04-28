using Microsoft.AspNetCore.Http;
using YuG.Infrastructure.Persistence;

namespace YuG.Infrastructure.Services;

public class HttpTenantProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpTenantProvider(IHttpContextAccessor accessor)
    {
        _httpContextAccessor = accessor;
    }

    public TenantContext GetTenant()
    {
        var tenantId = _httpContextAccessor?.HttpContext?.Request?.Headers["tenant-id"].ToString() ?? "yug";
        tenantId = string.IsNullOrWhiteSpace(tenantId) ? "yug" : tenantId;
        // 实际应该查数据库或缓存
        return new TenantContext
        {
            TenantId = tenantId,
            ConnectionString = $"Data Source={tenantId}.db"
        };
    }
}