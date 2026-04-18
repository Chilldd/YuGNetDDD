---
paths:
  - "YuG.Application/**/*Command.cs"
---

# Command（命令）开发规范

## 必须事项

- 命令类必须继承 `CommandBase<TResponse>`，其中 `TResponse` 为操作后的返回类型
- 命令类必须以 `Command` 后缀结尾，放在 `{业务模块}/Commands/{操作}/` 目录下
- 命令必须配置对应的 Validator（继承 `AbstractValidator<TCommand>`）
- 命令的属性必须是输入参数，不应包含业务状态
- 命令必须是不可变的，使用不可变类型规范（见 code-style.md）
- 命令必须有对应的 Handler（继承 `IRequestHandler<TCommand, TResponse>`）
- 命令命名使用动词开头：`CreateXxx`、`UpdateXxx`、`DeleteXxx`、`ActivateXxx`
- 命令与其 Handler、Validator 必须放在同一目录下

## 目录结构示例

```
YuG.Application/
├── Auth/
│   └── Commands/
│       ├── Login/
│       │   ├── LoginCommand.cs
│       │   ├── LoginCommandHandler.cs
│       │   └── LoginCommandValidator.cs
│       ├── Logout/
│       │   ├── LogoutCommand.cs
│       │   ├── LogoutCommandHandler.cs
│       │   └── LogoutCommandValidator.cs
│       └── RefreshToken/
│           ├── RefreshTokenCommand.cs
│           ├── RefreshTokenCommandHandler.cs
│           └── RefreshTokenCommandValidator.cs
└── Resource/
    └── Commands/
        ├── Create/
        │   ├── CreateResourceCommand.cs
        │   ├── CreateResourceCommandHandler.cs
        │   └── CreateResourceCommandValidator.cs
        └── Update/
            ├── UpdateResourceCommand.cs
            ├── UpdateResourceCommandHandler.cs
            └── UpdateResourceCommandValidator.cs
```

## 禁止事项

- 禁止在 Command 中包含业务逻辑
- 禁止在 Command 中注入 `DbContext` 或 `Repository`，这些应在 Handler 中注入
- 禁止在 Command 中返回复杂查询结果
- 禁止共享 Command（一个 Command 只对应一个场景）
- 禁止在 Command 中包含业务状态属性，只应包含输入参数
