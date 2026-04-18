using Microsoft.EntityFrameworkCore;
using YuG.Application.Interfaces;
using YuG.Infrastructure.Data.Entities.Auth;

namespace YuG.Infrastructure.Persistence;

/// <summary>
/// 应用数据库上下文，管理数据库连接和实体映射
/// </summary>
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    /// <summary>
    /// 初始化数据库上下文
    /// </summary>
    /// <param name="options">数据库上下文配置选项</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// 用户数据集
    /// </summary>
    public DbSet<UserEntity> Users => Set<UserEntity>();

    /// <summary>
    /// 保存所有变更到数据库
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>受影响的行数</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// 异步初始化数据库（执行迁移）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await Database.MigrateAsync(cancellationToken);
    }

    /// <summary>
    /// 配置实体映射
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 应用实体配置
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
