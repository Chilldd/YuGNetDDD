namespace YuG.Application.Common.Interfaces;

/// <summary>
/// 应用层数据库上下文接口，解耦具体 ORM 实现
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// 保存所有变更到数据库
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>受影响的行数</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 异步初始化数据库（执行迁移等）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
