---
paths:
  - "src/YuG.Application/**/*.cs"
---

# DDD 领域建模核心规范

## 一、核心设计原则

Application核心定位是用例编排层（Use Case Orchestration Layer）
- 职责
  - 调度 Domain 层
  - 组织业务流程
  - 控制事务边界
  - 输入输出适配（Command/Query）
- 不负责
  - 业务规则，复杂计算逻辑（Domain 负责）
  - 数据访问实现（Infrastructure 负责）
  
### CQRS 输入输出模型约定
- Command / Query = 输入模型（DTO）
- Result = 输出模型（DTO）
- Query 必须有 Result
- Command 可选返回值（一般返回 ID 或 Void）

### 守卫机制
- `Common/Guards/` 统一封装业务操作前置校验
- 所有 Command Handler 必须显式调用 Guard
- Guard 用于阻断非法业务操作（权限 / 状态 / 规则校验）

---

## 二、目录结构设计

Application
├── {子域}  （比如Permission，按照具体业务创建）
│   ├── {Submodule}  （比如Role，Resource，按照具体业务创建）
│   │   ├── Commands  （命令）
│   │   │   ├── {命令名称} （比如CreateResource）
│   │   │   │   ├── Handler.cs  （处理程序）
│   │   │   │   ├── Command.cs  （命令参数，验证器。操作命令使用）
│   │   │   ├── DTOs （不同命令之间通用的参数）
│   │   ├── Queries   （查询）
│   │   │   ├── {查询名称} （比如GetRoleList）
│   │   │   │   ├── Handler.cs  （处理程序）
│   │   │   │   ├── Query.cs  （查询参数）
│   │   │   │   ├── Result.cs  （返回结果）
│   │   ├── EventHandlers   (领域事件处理程序)
│   ├── Queries   （跨模块查询）
│   │   ├── {查询名称} （比如GetRoleList）
│   │   │   ├── Handler.cs  （处理程序）
│   │   │   ├── Query.cs  （查询参数）
│   │   │   ├── Result.cs  （返回结果）
│   ├── EventHandlers   （跨模块领域事件处理程序）
├── Common （通用）
│   ├── Behaviors  （MediatR管道）
│   ├── Guards （守卫类，如 SystemRoleGuard）
│   ├── Exceptions （异常）
│   ├── Interfaces （数据库上下文接口）

---

## 三、必须事项

- Handler 只做编排，不写业务规则
  - 调用 Domain，调用 Repository，组合结果
- Query / Command 严格分离
  - Query（查询）
    - 只读
    - 可直接使用 SQL / Dapper / EF 投影
    - 可绕过 Domain 层
  - Command（命令）
    - 仅用于状态变更
    - 必须经过 Domain 层
    - 必须保证业务一致性
- 事务必须在 Application 层控制
- 必须按 Submodule/UseCase 分文件夹
- Handler 必须单一职责
- 所有输入必须通过 Command / Query，Controller 不直接调用 Domain
- 所有外部依赖必须抽象接口
- 业务变更时，代码修复完成后必须检查对应领域事件是否需要修改，防止代码逻辑不一致或缺少业务处理

---

## 四、禁止事项

- 禁止按技术分类 Application
- 禁止 Handler 写业务规则
  - 比如：
    - 状态流转判断
    - 金额计算
    - 库存扣减规则
    - 权限业务规则
    - 折扣/价格逻辑
- 禁止 Application 依赖 Infrastructure 实现
- 禁止 Command / Query 跨用例复用
- 禁止 DTO 全局共享
- 禁止 Application 返回 Domain Entity