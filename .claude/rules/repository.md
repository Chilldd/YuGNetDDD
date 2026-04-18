---
paths:
  - "**/Domain/**/IRepository.cs"
  - "**/Infrastructure/**/Repository.cs"
---

# 仓储（Repository）开发规范

## 核心设计原则

- **接口与实现分离**：Domain 层定义业务接口，Infrastructure 层实现技术细节
- **映射转换职责**：Infrastructure 层负责 Domain 聚合根 ↔ ORM 实体的双向映射
- **隐藏 ORM 细节**：仓储接口不暴露 `IQueryable<T>`、`DbSet<T>` 等基础设施类型
- **只关心聚合根**：每个聚合根一个仓储，禁止为聚合内部实体创建仓储

## 必须事项

### Domain 层仓储接口
- 定义在 `YuG.Domain/Repositories/` 目录
- 命名规范：`I{聚合根名}Repository`（如 `IUserRepository`）
- 继承泛型仓储接口：`IRepository<TAggregate> where TAggregate : AggregateRoot`
- 可定义聚合根特定的查询方法（可选）

### Infrastructure 层仓储实现
- 定义在 `YuG.Infrastructure/Repositories/` 目录
- 继承 `Repository<TAggregate, TOrmEntity>` 基类
- 实现映射方法：`MapToDomain()` 和 `MapToOrmEntity()`
- 所有数据库访问必须通过异步方法（禁止同步调用）
- 返回类型为 `IReadOnlyList<T>` 或 `IReadOnlyCollection<T>`（不返回 `List<T>`）

## 禁止事项

- 禁止在仓储接口中暴露 ORM 类型（`IQueryable<T>`、`DbSet<T>` 等）
- 禁止在仓储中编写业务逻辑（应在 Domain 聚合根中实现）
- 禁止为聚合内部实体创建独立仓储
- 禁止在仓储实现中引入 Infrastructure 外的依赖（如日志）
- 禁止跨聚合直接通过仓储访问（在 Application 层处理）
- 禁止在仓储中发布领域事件（由 Application 层发布）
- 禁止返回 `List<T>` 或 `IEnumerable<T>`，只返回 `IReadOnlyList<T>`
- 禁止使用同步数据库方法，必须异步