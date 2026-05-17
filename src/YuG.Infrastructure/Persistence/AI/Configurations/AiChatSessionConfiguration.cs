using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuG.Domain.AI.Entities;
using YuG.Domain.AI.ValueObjects;

namespace YuG.Infrastructure.Persistence.AI.Configurations;

/// <summary>AiChatSession 领域实体 EF Core 配置。</summary>
public class AiChatSessionConfiguration : IEntityTypeConfiguration<AiChatSession>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<AiChatSession> builder)
    {
        builder.ToTable("AiChatSession");

        // 主键（雪花 ID）
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        // 审计属性
        builder.Property(s => s.CreatedAt)
            .ValueGeneratedOnAdd()
            .HasDefaultValue(DateTime.UtcNow);

        builder.Property(s => s.UpdatedAt)
            .ValueGeneratedOnAddOrUpdate()
            .HasDefaultValue(DateTime.UtcNow);

        // SessionId（外部引用标识）
        builder.Property(s => s.SessionId)
            .HasMaxLength(64)
            .IsRequired();

        builder.HasIndex(s => s.SessionId)
            .IsUnique();

        // 用户标识
        builder.Property(s => s.UserId)
            .IsRequired();

        builder.HasIndex(s => s.UserId);

        // 会话标题
        builder.Property(s => s.Title)
            .HasMaxLength(200)
            .IsRequired()
            .HasDefaultValue("新对话");

        // 最后活动时间
        builder.Property(s => s.LastActiveAt)
            .IsRequired();

        // 消息列表（值对象集合）
        builder.OwnsMany(s => s.Messages, msg =>
        {
            msg.ToTable("AiChatMessage");
            msg.WithOwner().HasForeignKey("AiChatSessionId");

            msg.Property<long>("Id");
            msg.HasKey("Id");

            msg.Property(m => m.Role)
                .HasMaxLength(20)
                .IsRequired();

            msg.Property(m => m.Content)
                .IsRequired();

            msg.Property(m => m.SequenceNumber)
                .IsRequired();

            msg.Property(m => m.TokenCount);

            msg.Property(m => m.CreatedAt)
                .ValueGeneratedOnAdd()
                .HasDefaultValue(DateTime.UtcNow);
        });

        builder.Navigation(s => s.Messages)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
