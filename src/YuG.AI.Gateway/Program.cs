using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using YuG.AI.Gateway.Configuration;
using YuG.AI.Gateway.Extensions;
using YuG.AI.Gateway.Middleware;
using YuG.AI.Gateway.Plugins;
using YuG.AI.Gateway.Services;
using YuG.Common.Extensions;
using YuG.Common.Interfaces;
using YuG.Common.Jwt;

var builder = WebApplication.CreateBuilder(args);

// 加载本地开发配置（仅本地，不提交到 git）
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddSingleton<ICache, InMemoryCache>();
builder.Services.AddSwaggerServices("YuG AI Gateway API");

// JWT 认证（与主 API 共享同一 SecretKey/Issuer，只认证不鉴权）
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAiOptions(builder.Configuration);
builder.Services.AddAiPlugin<TimePlugin>();
builder.Services.AddAiCoreServices();
builder.Services.AddSingleton<AiExceptionHandlingMiddleware>();

// 速率限制：按用户 ID 限流，60 次/分钟，未登录时按 IP
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy<string, UserRateLimiterPolicy>("Chat");
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// 启动校验
var aiOptions = app.Services.GetRequiredService<IOptions<AiOptions>>().Value;
aiOptions.Validate();

app.UseRateLimiter();
app.UseMiddleware<AiExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerServices("YuG AI Gateway API");
}

app.MapControllers();

await app.RunAsync();
