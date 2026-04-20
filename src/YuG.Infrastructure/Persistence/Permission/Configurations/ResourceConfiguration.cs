using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuG.Domain.Permission.Entities;
using YuG.Domain.Permission.Enums;

namespace YuG.Infrastructure.Persistence.Permission.Configurations;

/// <summary>
/// Resource 领域实体 EF Core 配置
/// </summary>
public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    /// <summary>
    /// 配置 Resource 实体
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.ToTable("Resource");

        // 主键配置
        builder.HasKey(r => r.Id);

        // 审计属性配置（不使用 CURRENT_TIMESTAMP）
        builder.Property(r => r.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValue(DateTime.UtcNow);

        builder.Property(r => r.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValue(DateTime.UtcNow);

        // 资源名称配置
        builder.Property(r => r.Name)
            .HasMaxLength(200)
            .IsRequired();

        // 资源编码配置（唯一）
        builder.Property(r => r.Code)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(r => r.Code)
            .IsUnique();

        // 资源描述配置
        builder.Property(r => r.Description)
            .HasMaxLength(500)
            .IsRequired(false);

        // HTTP 方法配置（枚举转字符串）
        builder.Property(r => r.HttpMethod)
            .HasConversion(
                v => v.ToString(),
                s => Enum.Parse<ResourceHttpMethod>(s, ignoreCase: true))
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue(ResourceHttpMethod.Get);

        // API 路径配置
        builder.Property(r => r.Path)
            .HasMaxLength(500)
            .IsRequired();

        // 父级资源标识配置
        builder.Property(r => r.ParentId);

        // 排序顺序配置
        builder.Property(r => r.SortOrder)
            .HasDefaultValue(0);

        // 资源状态配置（枚举转字符串）
        builder.Property(r => r.Status)
            .HasConversion(
                v => v.ToString(),
                s => Enum.Parse<ResourceStatus>(s, ignoreCase: true))
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(ResourceStatus.Active);

        // 配置自引用外键关系（父子关系）
        builder.HasOne<Resource>()
            .WithMany()
            .HasForeignKey(r => r.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
