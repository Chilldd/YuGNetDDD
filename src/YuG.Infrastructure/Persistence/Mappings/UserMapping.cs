using YuG.Domain.Entities;
using YuG.Domain.ValueObjects;
using YuG.Infrastructure.Persistence.Entities.Auth;

namespace YuG.Infrastructure.Persistence.Mappings;

/// <summary>
/// 用户 ORM 实体与领域实体的映射转换器
/// </summary>
public static class UserMapping
{
    /// <summary>
    /// 将 ORM 实体映射到领域实体
    /// </summary>
    /// <param name="entity">ORM 实体</param>
    /// <returns>领域实体</returns>
    public static User ToDomain(this UserEntity entity)
    {
        var user = new User(entity.Username, entity.PasswordHash)
        {
            Id = entity.Id
        };

        // 映射刷新令牌
        foreach (var refreshTokenEntity in entity.RefreshTokens)
        {
            var refreshToken = RefreshToken.Empty with
            {
                Token = refreshTokenEntity.Token,
                ExpiresAt = refreshTokenEntity.ExpiresAt,
                IsRevoked = refreshTokenEntity.IsRevoked,
                CreatedAt = refreshTokenEntity.CreatedAt
            };
            user.AddRefreshToken(refreshToken);
        }

        return user;
    }

    /// <summary>
    /// 将领域实体映射到 ORM 实体
    /// </summary>
    /// <param name="user">领域实体</param>
    /// <returns>ORM 实体</returns>
    public static UserEntity ToEntity(this User user)
    {
        var entity = new UserEntity
        {
            Id = user.Id,
            Username = user.Username,
            PasswordHash = user.PasswordHash,
            // CreatedAt 和 UpdatedAt 由 EF Core 自动处理
        };

        // 映射刷新令牌
        foreach (var refreshToken in user.RefreshTokens)
        {
            entity.RefreshTokens.Add(new RefreshTokenEntity
            {
                Token = refreshToken.Token,
                UserId = user.Id,
                ExpiresAt = refreshToken.ExpiresAt,
                IsRevoked = refreshToken.IsRevoked,
                CreatedAt = refreshToken.CreatedAt
            });
        }

        return entity;
    }
}

