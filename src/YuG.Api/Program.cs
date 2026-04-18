using Microsoft.OpenApi.Models;
using YuG.Api.Extensions;
using YuG.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// 显式配置 Kestrel 监听地址（解决 WSL2 端口转发问题）
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // 监听所有网络接口的 5000 端口
});

// 注册基础设施层服务（数据库、仓储等）
builder.Services.AddInfrastructure(builder.Configuration);

// 注册应用层服务（MediatR、FluentValidation）
builder.Services.AddApplicationServices();

// 注册控制器服务
builder.Services.AddControllers();

// 添加 JWT 认证
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"] ?? string.Empty))
        };
    });

builder.Services.AddAuthorization();

// 注册 Swagger 服务
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "YuG API",
        Version = "v1",
        Description = "YuG 项目 API 文档",
        Contact = new OpenApiContact
        {
            Name = "YuG"
        }
    });

    // 添加 JWT 认证支持
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT 授权令牌，请在下方输入框输入 Bearer {token}（注意中间有一个空格）",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    // 启用 XML 注释
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// 初始化数据库
await app.InitializeDatabaseAsync();

// 配置 HTTP 请求管道
app.UseExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "YuG API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
