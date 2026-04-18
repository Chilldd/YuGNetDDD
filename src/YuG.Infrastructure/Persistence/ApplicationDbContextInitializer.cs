using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YuG.Domain.Entities;
using YuG.Domain.Interfaces;
using YuG.Infrastructure.Data.Mappings;
using YuG.Infrastructure.Services;
using YuG.Infrastructure.Data.Entities;

namespace YuG.Infrastructure.Persistence;

/// <summary>
/// 数据库初始化器，负责执行迁移和种子数据
/// </summary>
public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// 初始化数据库初始化器
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="context">数据库上下文</param>
    /// <param name="passwordHasher">密码哈希服务</param>
    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _logger = logger;
        _context = context;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 初始化数据库（执行迁移）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始初始化数据库...");
            await _context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("数据库初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库初始化失败");
            throw;
        }
    }

    /// <summary>
    /// 填充种子数据
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始填充种子数据...");

            // 检查是否已有用户
            if (await _context.Users.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("数据库已包含用户数据，跳过种子数据填充");
                return;
            }

            // 创建默认管理员用户（领域实体）
            var adminPasswordHash = _passwordHasher.Hash("admin123");
            var admin = new User("admin", adminPasswordHash);

            // 转换为数据库实体并保存
            var adminEntity = admin.ToEntity();
            await _context.Users.AddAsync(adminEntity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("种子数据填充完成：已创建默认管理员账户 (用户名: admin, 密码: admin123)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "种子数据填充失败");
            throw;
        }
    }
}
