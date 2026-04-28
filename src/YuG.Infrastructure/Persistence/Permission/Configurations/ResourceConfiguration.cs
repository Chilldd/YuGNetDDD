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

        // 资源类型配置（枚举转字符串）
        builder.Property(r => r.Type)
            .HasConversion(
                v => v.ToString(),
                s => Enum.Parse<ResourceType>(s, ignoreCase: true))
            .HasMaxLength(20)
            .IsRequired()
            .HasDefaultValue(ResourceType.Api);

        // HTTP 方法配置（枚举转字符串，仅 API 类型有效）
        builder.Property(r => r.HttpMethod)
            .HasConversion(
                v => v!.ToString(),
                s => string.IsNullOrEmpty(s) ? null : Enum.Parse<ResourceHttpMethod>(s, ignoreCase: true))
            .HasMaxLength(10)
            .IsRequired(false);

        // API 路径配置（仅 API 类型有效）
        builder.Property(r => r.Path)
            .HasMaxLength(500)
            .IsRequired(false);

        // 菜单图标配置（仅菜单类型有效）
        builder.Property(r => r.Icon)
            .HasMaxLength(100)
            .IsRequired(false);

        // 前端路由配置（仅菜单类型有效）
        builder.Property(r => r.Route)
            .HasMaxLength(500)
            .IsRequired(false);

        // 组件路径配置（仅菜单类型有效）
        builder.Property(r => r.Component)
            .HasMaxLength(500)
            .IsRequired(false);

        // 是否隐藏配置（仅菜单类型有效）
        builder.Property(r => r.IsHidden)
            .HasDefaultValue(false);

        // 菜单角标配置（仅菜单类型有效）
        builder.Property(r => r.Badge)
            .HasMaxLength(50)
            .IsRequired(false);

        // 权限编码配置（仅按钮类型有效）
        builder.Property(r => r.PermissionCode)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.HasIndex(r => r.PermissionCode)
            .IsUnique();

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
