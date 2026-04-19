using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuG.Infrastructure.Persistence.Entities.Auth;

namespace YuG.Infrastructure.Persistence.Configurations;

/// <summary>
/// UserEntity 实体 EF Core 配置
/// </summary>
public class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity>
{
    /// <summary>
    /// 配置 UserEntity 实体
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("User");

        // 主键配置
        builder.HasKey(u => u.Id);

        // 审计属性配置（不使用 CURRENT_TIMESTAMP）
        builder.Property(u => u.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValue(DateTime.UtcNow);

        builder.Property(u => u.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValue(DateTime.UtcNow);

        // 用户名配置
        builder.Property(u => u.Username)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        // 密码哈希配置
        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        // 配置 Owned Type：RefreshToken
        builder.OwnsMany(u => u.RefreshTokens, rt =>
        {
            rt.ToTable("RefreshToken");

            rt.WithOwner()
                .HasForeignKey("UserId");

            rt.HasKey("Token");

            rt.Property(r => r.Token)
                .HasMaxLength(500)
                .IsRequired();

            rt.Property(r => r.ExpiresAt)
                .IsRequired();

            rt.Property(r => r.IsRevoked)
                .IsRequired()
                .HasDefaultValue(false);

            rt.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValue(DateTime.UtcNow);
        });
    }
}
