using YuG.Api.Extensions;
using YuG.Api.Middleware;
using YuG.Infrastructure;
using YuG.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// 显式配置 Kestrel 监听地址（解决 WSL2 端口转发问题）
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // 监听所有网络接口的 5000 端口
});

// 注册应用层服务（MediatR、FluentValidation）
builder.Services.AddApplicationServices();

// 注册 HTTP 上下文访问器（用于多租户获取租户信息）
builder.Services.AddHttpContextAccessor();

// 多租户
builder.Services.AddScoped<HttpTenantProvider>();

// 注册 API 层工具服务
builder.Services.AddApiTools();

// 注册控制器服务
builder.Services.AddControllers();

// 添加 JWT 认证
builder.Services.AddJwtAuthentication(builder.Configuration);

// 添加 Swagger 服务
builder.Services.AddSwaggerServices();

// 注册基础设施层服务（数据库、仓储等）
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// 初始化数据库
await app.InitializeDatabaseAsync();

// 配置 HTTP 请求管道
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerServices();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
