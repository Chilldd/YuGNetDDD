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
- Command / Query = 输入模型（DTO）
- Result = 输出模型（DTO）
- Query 必须有 Result，Command 可选（一般返回 ID 或 Void）

---

## 二、目录规范
Application
├── 子域  （比如Permission）
│    ├── UseCase  （比如Role，Resource）
│    │    ├── 命令名称 （比如CreateResource）
│    │    │    ├── Command.cs  （命令参数，验证器。操作命令使用）
│    │    │    ├── Query.cs  （命令参数，验证器。查询命令使用）
│    │    │    ├── Result.cs  （查询命令返回对象）
│    │    │    ├── Handler.cs  （处理程序）
├── Common （通用）
│    ├── Behaviors  （MediatR管道）
│    ├── Exceptions （异常）
│    ├── Interfaces （数据库上下文接口）

---

## 三、必须事项

- Handler 只做编排，不写业务规则
  - 调用 Domain，调用 Repository，组合结果
- Query / Command 严格分离
  - Query（查询）
    - 只读
    - 使用 Dapper / SQL 查询
    - 可绕过 Domain
  - Command（命令）
    - 修改状态
    - 必须走 Domain
- 事务必须在 Application 层控制
- 必须按 UseCase 分文件夹
- Handler 必须单一职责
- 所有输入必须通过 Command / Query，Controller 不直接调用 Domain
- 所有外部依赖必须抽象接口

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