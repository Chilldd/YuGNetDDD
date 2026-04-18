using YuG.Api.Middleware;
using YuG.Infrastructure.Persistence;

namespace YuG.Api.Extensions;

/// <summary>
/// 应用构建器扩展方法
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// 注册全局异常处理中间件
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <returns>应用构建器（支持链式调用）</returns>
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }

    /// <summary>
    /// 初始化数据库（执行迁移和种子数据）
    /// </summary>
    /// <param name="app">应用构建器</param>
    /// <returns>应用构建器（支持链式调用）</returns>
    public static async Task<IApplicationBuilder> InitializeDatabaseAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();

        await initializer.InitialiseAsync();
        await initializer.SeedAsync();

        return app;
    }
}
