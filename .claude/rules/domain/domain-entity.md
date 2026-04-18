---
paths:
  - "YuG.Domain/**/*.cs"
---

# 领域实体（Entity）开发规范

## 核心设计原则

- **Domain 层纯净性**：零外部依赖（无 NuGet 包、无 Infrastructure/Application 引用）
- **Domain 与 Infrastructure 分离**：Domain 层只含业务逻辑，ORM 实体由 Infrastructure 独立管理
- **聚合根职责**：封装业务规则、管理状态、产生领域事件
- **充血模型**：实体必须封装业务逻辑，不允许贫血模型

## 必须事项

- 聚合根必须继承 `AggregateRoot` 基类，获得 `Id` 和领域事件能力
- 业务规则验证必须在实体方法中完成（如 `Activate()`、`ChangeEmail()`），禁止外部直接修改状态
- 属性 setter 应为 `private` 或 `init`，状态变更必须通过行为方法
- 相等性比较基于 `Id` 属性（`AggregateRoot` 已实现）
- 业务规则违反必须抛出 `DomainException` 及其子类
- 每个聚合根对应一个仓储接口（见 [repository.md](repository.md)）

## 禁止事项

- 禁止引用任何 NuGet 包
- 禁止引用上层项目（Application、Infrastructure、Api）
- 禁止创建贫血模型（只有属性没有行为）
- 禁止在属性中添加业务逻辑，使用专用的行为方法
- 禁止跨聚合直接修改状态，使用领域事件通信
- 禁止依赖注入或配置代码
- 禁止 Domain 对象和 ORM 实体共用相同的类
