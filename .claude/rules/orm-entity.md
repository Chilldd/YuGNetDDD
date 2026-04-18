---
paths:
  - "YuG.Infrastructure/Data/Entities/**/*Entity.cs"
---

# ORM 实体规范

## 核心设计原则

- **ORM 实体与 Domain 聚合根完全独立**：两者有不同的职责和生命周期
  - Domain `AggregateRoot`：纯业务，包含业务逻辑和领域事件
  - Infrastructure `BaseEntity`：纯持久化，只包含数据映射和审计属性
- ORM 实体仅负责数据持久化映射，不包含任何业务逻辑
- 通过 Repository 层的映射进行两者转换：`MapToDomain()`/`MapToOrmEntity()`

## 必须事项

- 数据库实体必须以 `Entity` 结尾命名，如 `UserEntity`、`OrderEntity`
- 命名空间格式：`YuG.Infrastructure.Data.Entities.{BoundedContext}`
- 数据库实体必须继承 `BaseEntity` 基类，自动获得 `Id`、`CreatedAt`、`UpdatedAt` 等审计属性
- 所有属性必须为 `public` 访问级别，使用自动属性
- 属性必须初始化为默认值（推荐使用现代 C# 特性）：
  - 字符串: `string Name { get; set; } = string.Empty;`
  - 集合: `List<ItemEntity> Items { get; set; } = [];` （集合表达式）
  - 值类型: `int? Status { get; set; }`
  - 可空引用: `string? Description { get; set; }` （Nullable Reference Types）
- 不允许在属性访问器中添加业务逻辑
- 外键属性以大写 `Id` 结尾：`Guid UserId { get; set; }`
- 导航属性采用 PascalCase 命名：`public UserEntity Author { get; set; }`
- 必须使用 `class` 而非 `record`（EF Core 需要完全可变属性）
- 禁止 `init-only` 属性（`{get; init;}`），必须完整的 `{get; set;}`

### EF Core 配置

- 配置必须在 `Data/Configurations` 目录下的独立文件中实现 `IEntityTypeConfiguration<T>`
- 文件命名：`{实体名}Configuration.cs`
- 必须配置：表名、主键、列约束、外键关系、索引、级联删除策略
- 一对多关系禁止级联删除：`.OnDelete(DeleteBehavior.Restrict)`（除非是完全包含的聚合）
- 业务键必须配置唯一索引：`.HasIndex().IsUnique()`

### 聚合根映射

- 每个 ORM 实体对应 Domain 层的一个聚合根
- 聚合内部实体只能通过聚合根 ORM 实体访问，不能有独立仓储
- 使用 `OwnsOne`/`OwnsMany` 管理值对象和强包含关系

### 映射规范

- Repository 通过实现 `MapToDomain()`/`MapToOrmEntity()` 进行转换
- 可在 `Data/Mappings` 目录下创建扩展方法支持映射
- 查询场景可提供直接映射到 DTO 的扩展方法

## 可选特性

- **并发处理**：高并发场景配置乐观锁 `.Property(e => e.RowVersion).IsRowVersion().ValueGeneratedOnAddOrUpdate()`
- **软删除**：需要时添加 `public bool IsDeleted { get; set; } = false;` 和全局查询过滤器 `.HasQueryFilter(e => !e.IsDeleted)`

## 禁止事项

- **禁止在 ORM 实体中编写业务逻辑**，包括：
  - 业务规则验证、计算逻辑、方法调用
  - 领域事件机制（DomainEvents、AddDomainEvent 等）
- **禁止在 ORM 实体中引用 Domain 对象**，如 AggregateRoot、ValueObject 实例
- **禁止跨聚合根导航**，聚合间只通过 ID 引用
- **禁止一个文件中有多个实体**
- **禁止 ORM 实体继承 Domain 层的业务类**，两者完全独立设计
- **禁止使用 `record` 或 `init-only` 属性**（见属性定义部分）