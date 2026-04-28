using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuG.Domain.Identity.Entities;
using YuG.Domain.Identity.Enums;
using YuG.Domain.Permission.Entities;

namespace YuG.Infrastructure.Persistence.Identity.Configurations;

/// <summary>
/// Role 领域实体 EF Core 配置
/// </summary>
public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    /// <summary>
    /// 配置 Role 实体
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Role");

        // 主键配置（雪花 ID 由应用层生成，非数据库自增）
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();

        // 审计属性配置（不使用 CURRENT_TIMESTAMP）
        builder.Property(r => r.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValue(DateTime.UtcNow);

        builder.Property(r => r.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValue(DateTime.UtcNow);

        // 角色名称配置
        builder.Property(r => r.Name)
            .HasMaxLength(100)
            .IsRequired();

        // 角色编码配置（唯一）
        builder.Property(r => r.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.Code)
            .IsUnique();

        // 角色描述配置
        builder.Property(r => r.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        // 角色状态配置（枚举转字符串）
        builder.Property(r => r.Status)
            .HasConversion(
                v => v.ToString(),
                s => Enum.Parse<RoleStatus>(s, ignoreCase: true))
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(RoleStatus.Active);

        // 配置 Role ↔ Resource 多对多关系
        builder.HasMany(r => r.Resources)
            .WithMany()
            .UsingEntity(j => j.ToTable("RoleResource"));
    }
}
