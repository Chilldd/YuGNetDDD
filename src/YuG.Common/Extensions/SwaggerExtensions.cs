using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace YuG.Common.Extensions;

/// <summary>
/// Swagger 服务和中间件扩展方法
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// 添加 Swagger 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="apiTitle">API 标题</param>
    /// <param name="apiDescription">API 描述</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddSwaggerServices(
        this IServiceCollection services,
        string apiTitle = "YuG API",
        string apiDescription = "")
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = apiTitle,
                Version = "v1",
                Description = apiDescription,
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

            // 启用 XML 注释（使用入口程序集的名称查找对应的 XML 文档文件）
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly is not null)
            {
                var xmlFile = $"{entryAssembly.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }
            }

            // 自定义 schemaId，使用完整类型名称避免不同命名空间下同名类冲突
            options.CustomSchemaIds(type => type.FullName?.Replace('.', '_'));
        });

        return services;
    }

    /// <summary>
    /// 使用 Swagger 中间件
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <param name="apiTitle">API 标题</param>
    /// <returns>应用构建器（支持链式调用）</returns>
    public static IApplicationBuilder UseSwaggerServices(
        this IApplicationBuilder app,
        string apiTitle = "YuG API")
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", $"{apiTitle} v1");
            options.RoutePrefix = "swagger";
            options.ConfigObject.PersistAuthorization = true;
        });

        return app;
    }
}
