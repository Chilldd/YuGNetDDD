using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuG.Infrastructure.Data.Entities.Auth;

namespace YuG.Infrastructure.Data.Configurations;

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
        builder.ToTable("Users");

        // 主键配置
        builder.HasKey(u => u.Id);

        // 审计属性配置
        builder.Property(u => u.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

        builder.Property(u => u.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");

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
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
