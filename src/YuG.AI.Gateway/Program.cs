using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using YuG.AI.Gateway.Configuration;
using YuG.AI.Gateway.Extensions;
using YuG.AI.Gateway.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAiOptions(builder.Configuration);
builder.Services.AddAiCoreServices();
builder.Services.AddSingleton<AiExceptionHandlingMiddleware>();

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

app.UseMiddleware<AiExceptionHandlingMiddleware>();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

await app.RunAsync();
