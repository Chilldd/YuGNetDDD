using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using YuG.AI.Gateway.Configuration;
using YuG.AI.Gateway.Extensions;
using YuG.AI.Gateway.Middleware;
using YuG.Common.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT 认证（与主 API 共享同一 SecretKey/Issuer，只认证不鉴权）
builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddAiOptions(builder.Configuration);
builder.Services.AddAiCoreServices();
builder.Services.AddSingleton<AiExceptionHandlingMiddleware>();
builder.Services.AddSingleton<RequestBodyParsingMiddleware>();

// 速率限制：按 sessionId 限流，60 次/分钟，未传 sessionId 时按 IP
builder.Services.AddRateLimiter(options =>
{
    options.AddPolicy<string, YuG.AI.Gateway.Middleware.SessionRateLimiterPolicy>("Chat");
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// 启动校验
var aiOptions = app.Services.GetRequiredService<IOptions<AiOptions>>().Value;
aiOptions.Validate();

app.UseMiddleware<RequestBodyParsingMiddleware>();
app.UseRateLimiter();
app.UseMiddleware<AiExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

await app.RunAsync();
