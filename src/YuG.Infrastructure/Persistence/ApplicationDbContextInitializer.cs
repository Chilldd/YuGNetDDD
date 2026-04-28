using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YuG.Domain.Identity.Entities;
using YuG.Domain.Common.Interfaces;

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

            // 创建默认管理员用户（领域实体，直接保存）
            var adminPasswordHash = _passwordHasher.Hash("admin123");
            var admin = new User("admin", adminPasswordHash);

            await _context.Users.AddAsync(admin, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("种子数据填充完成：已创建默认管理员账户 (用户名: admin, 密码: admin123)");

            // 创建默认角色
            if (!await _context.Roles.AnyAsync(cancellationToken))
            {
                var adminRole = new Role("管理员", "admin", "系统管理员，拥有所有权限");
                var userRole = new Role("普通用户", "user", "普通用户，拥有基础权限");

                await _context.Roles.AddRangeAsync([adminRole, userRole], cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("种子数据填充完成：已创建默认角色 (admin, user)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "种子数据填充失败");
            throw;
        }
    }
}
