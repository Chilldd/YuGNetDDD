---
paths:
  - "YuG.Application/**/*Query.cs"
---

# Query（查询）开发规范

## 必须事项

- 查询类必须继承 `QueryBase<TResponse>`，其中 `TResponse` 为返回的 DTO 类型
- 查询类必须以 `Query` 后缀结尾，放在 `Queries/` 目录下
- 查询应尽量简单，只包含必要的查询参数
- 查询必须有对应的 Handler（继承 `IRequestHandler<TQuery, TResponse>`）
- 查询命名使用动词开头：`GetXxx`、`SearchXxx`、`ListXxx`
- 查询必须是不可变的，使用不可变类型规范（见 code-style.md）
- 查询与其 Handler 必须放在同一目录下

## 禁止事项

- 禁止在 Query 中修改数据库状态
- 禁止在 Query 中包含业务逻辑
- 禁止在 Query 中注入 `DbContext` 或 `Repository`，这些应在 Handler 中注入
- 禁止在 Query 中执行耗时操作（如复杂计算），这些应在应用服务或领域层完成
- 禁止在 Query 中包含业务状态属性，只应包含查询参数
