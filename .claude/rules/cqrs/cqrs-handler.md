---
paths:
  - "YuG.Application/**/*Handler.cs"
---

# Handler（处理器）开发规范

## 核心设计原则

- **单一职责**：一个 Handler 只处理一个 Command 或 Query
- **DTO 隔离**：Handler 必须返回 DTO，禁止暴露 Domain 实体
- **控制反转**：调用委托给 Domain 层和 Infrastructure 层，不编写业务规则

## 必须事项

- Handler 必须实现 `IRequestHandler<TRequest, TResponse>` 接口
- Handler 必须以 `CommandHandler` 后缀命名
- Handler 与对应的 Command 放在同一目录下（`{业务模块}/Commands/{操作}/`）
- **CommandHandler**：调用领域对象执行业务逻辑 → 通过仓储持久化 → 发布领域事件 → 返回 Response DTO
- 实体到 DTO 的映射必须在 Handler 中完成
- 复杂映射可使用 AutoMapper 或映射扩展方法

## 命名规范

- Handler 类名：`{操作名}{实体名}CommandHandler`
  - 例如：`LoginCommandHandler`、`CreateResourceCommandHandler`

## 目录结构示例

```
YuG.Application/
├── Auth/
│   └── Commands/
│       ├── Login/
│       │   ├── LoginCommand.cs
│       │   ├── LoginCommandHandler.cs
│       │   └── LoginCommandValidator.cs
└── Resource/
    └── Commands/
        ├── Create/
        │   ├── CreateResourceCommand.cs
        │   ├── CreateResourceCommandHandler.cs
        │   └── CreateResourceCommandValidator.cs
```

## 禁止事项

- 禁止在 Handler 中编写业务规则（应在 Domain 实体中实现）
- 禁止返回 Domain 实体，只返回 DTO
- 禁止 Handler 间直接调用（使用 MediatR.Send()）
- 禁止 CommandHandler 中执行查询返回复杂结果
- 禁止跳过 Domain 对象直接操作数据库
