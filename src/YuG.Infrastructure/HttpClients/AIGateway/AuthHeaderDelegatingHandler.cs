using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace YuG.Infrastructure.HttpClients.AIGateway;

/// <summary>自动将当前 HTTP 请求的 Authorization 头转发到 AI.Gateway 出站请求。</summary>
public class AuthHeaderDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>初始化 <see cref="AuthHeaderDelegatingHandler"/> 实例。</summary>
    /// <param name="httpContextAccessor">HTTP 上下文访问器</param>
    public AuthHeaderDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is not null)
        {
            var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader is not null)
            {
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
