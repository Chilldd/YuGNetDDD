---
paths:
  - "YuG.Application/**/*Handler.cs"
---

# Handler（处理器）开发规范

## 核心设计原则

- **单一职责**：一个 Handler 只处理一个 Command 或 Query
- **DTO 隔离**：Handler
 必须返回 DTO，禁止暴露 Domain 实体
- **控制反转**：调用委托给 Domain 层和 Infrastructure 层，不编写业务规则

## 必须事项

- Handler 必须实现 `IRequestHandler<TRequest, TResponse>` 接口
- Handler 必须以 `CommandHandler` 或 `QueryHandler` 后缀命名
- Handler 与对应的 Command/Query 放在同一目录下
- **CommandHandler**：调用领域对象执行业务逻辑 → 通过仓储持久化 → 发布领域事件 → 返回 Response DTO
- **QueryHandler**：读取数据 → 检查"未找到"场景（抛出 `NotFoundException`）→ 映射为 DTO 返回
- 实体到 DTO 的映射必须在 Handler 中完成
- 复杂映射可使用 AutoMapper 或映射扩展方法

## 禁止事项

- 禁止在 Handler 中编写业务规则（应在 Domain 实体中实现）
- 禁止返回 Domain 实体，只返回 DTO
- 禁止 Handler 间直接调用（使用 MediatR.Send()）
- 禁止 CommandHandler 中执行查询返回复杂结果
- 禁止 QueryHandler 中修改数据库状态
- 禁止跳过 Domain 对象直接操作数据库
