---
paths:
  - "YuG.Application/Queries/**/*.cs"
  - "YuG.Infrastructure/Read/**/*.cs"
---

# Query（查询）开发规范

查询采用 Service + Dapper 模式，不使用 MediatR。

## 必须事项

### Application 层（接口定义）

- 查询服务接口放在 `YuG.Application/Queries/` 目录下
- 接口命名使用 `IXxxReadService` 格式
- 查询方法命名使用动词开头：`GetXxxAsync`、`SearchXxxAsync`、`ListXxxAsync`
- 返回类型为 DTO（见 DTO 规范）

### Infrastructure 层（实现）

- 查询服务实现放在 `YuG.Infrastructure/Read/` 目录下
- 使用 Dapper 直接查询数据库，不使用 EF Core 或仓储
- 查询结果映射为 DTO 返回

## 禁止事项

- 禁止在查询服务中修改数据库状态（写操作）
- 禁止在查询服务中包含业务逻辑
- 禁止在查询服务中使用 Repository 或 DbContext
- 禁止在查询服务中执行耗时操作（如复杂计算）
- 禁止使用 MediatR Query/Handler 模式进行查询
