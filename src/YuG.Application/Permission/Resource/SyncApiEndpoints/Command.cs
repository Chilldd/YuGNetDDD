using FluentValidation;
using YuG.Application.Common;
using YuG.Domain.Permission.Enums;

namespace YuG.Application.Permission.Resource.SyncApiEndpoints;

/// <summary>
/// 扫描结果（包含端点列表）
/// </summary>
public record DiscoveredApiScanResult(
    IReadOnlyList<DiscoveredEndpointInfo> Endpoints);

/// <summary>
/// 已发现的端点信息
/// </summary>
public record DiscoveredEndpointInfo(
    string Path,
    ResourceHttpMethod HttpMethod,
    string DisplayName,
    string GeneratedCode,
    string Description);

/// <summary>
/// 同步 API 端点命令
/// </summary>
public class SyncApiEndpointsCommand : CommandBase<SyncApiEndpointsResult>
{
    /// <summary>
    /// 扫描到的端点列表
    /// </summary>
    public IReadOnlyList<DiscoveredEndpointInfo> Endpoints { get; init; } = [];
}

/// <summary>
/// 同步 API 端点命令验证器
/// </summary>
public class SyncApiEndpointsCommandValidator : AbstractValidator<SyncApiEndpointsCommand>
{
    /// <summary>
    /// 初始化同步 API 端点命令验证器
    /// </summary>
    public SyncApiEndpointsCommandValidator()
    {
        RuleFor(x => x.Endpoints).NotEmpty();
        RuleForEach(x => x.Endpoints).ChildRules(endpoint =>
        {
            endpoint.RuleFor(x => x.Path).NotEmpty().MaximumLength(500);
            endpoint.RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(200);
            endpoint.RuleFor(x => x.GeneratedCode).NotEmpty().MaximumLength(100)
                .Matches(@"^[a-zA-Z0-9_-]+$").WithMessage("资源编码只能包含字母、数字、下划线和短横线");
            endpoint.RuleFor(x => x.Description).MaximumLength(500)
                .WithMessage("资源描述长度不能超过 500 个字符");
        });
    }
}
