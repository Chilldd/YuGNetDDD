using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuG.Domain.Identity.Entities;
using YuG.Domain.Identity.ValueObjects;

namespace YuG.Infrastructure.Persistence.Identity.Configurations;

/// <summary>
/// User 领域实体 EF Core 配置
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// 配置 User 实体
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("User");

        // 主键配置（雪花 ID 由应用层生成，非数据库自增）
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedNever();

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

        // 配置 Owned Type：RefreshToken（值对象）
        builder.OwnsMany(u => u.RefreshTokens, rt =>
        {
            rt.ToTable("RefreshToken");

            rt.WithOwner()
                .HasForeignKey("UserId");

            rt.HasKey(r => r.Token);

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

        // 配置 User ↔ Role 多对多关系
        builder.HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity(j => j.ToTable("UserRole"));
    }
}
