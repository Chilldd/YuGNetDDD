using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace YuG.Api.Authorization;

/// <summary>
/// 权限编码策略提供者：未注册的策略名自动解析为 PermissionRequirement
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly AuthorizationOptions _options;

    /// <summary>
    /// 初始化权限编码策略提供者
    /// </summary>
    /// <param name="options">授权选项</param>
    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// 获取默认策略
    /// </summary>
    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return Task.FromResult(_options.DefaultPolicy);
    }

    /// <summary>
    /// 获取回退策略
    /// </summary>
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return Task.FromResult(_options.FallbackPolicy);
    }

    /// <summary>
    /// 根据策略名获取策略：未在 AddAuthorization 中注册的策略名，视为权限编码
    /// </summary>
    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var policy = _options.GetPolicy(policyName);
        if (policy is not null)
        {
            // 静态注册的策略（如 FallbackPolicy）直接返回
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // 未注册的策略名视为权限编码，动态创建 PermissionRequirement
        var permissionPolicy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(permissionPolicy);
    }
}
