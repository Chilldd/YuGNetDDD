---
paths:
  - "src/YuG.Api/**/*.cs"
---

# API 层设计规范

## 一、核心设计原则

API 层定位：接口适配层（Interface Adapter Layer）

核心原则：只负责接收请求和返回响应，不参与任何业务逻辑。
需要根据业务逻辑，判断是否增加授权策略，授权策略使用RBAC的权限编码模式，权限编码格式为：{控制器名小写}:{方法名小写}

### 职责

- 接收 HTTP 请求
- 参数绑定（Body / Query / Route）
- 调用 Application 层（Command / Query）
- 返回统一响应结构
- 处理协议相关内容（状态码、鉴权等）

### 不负责

- 不负责业务规则
- 不负责任务编排
- 不负责数据访问
- 不负责事务控制
- 不负责复杂数据转换

## 必须事项

- Controller 只能调用 Application 层
- 所有输入必须使用 Command / Query
- 所有输出必须使用统一响应结构
- 必须使用特性进行声明式控制（如鉴权、路由）
- 参数来源必须明确（Body / Query / Route）
- 异常必须统一处理（中间件或过滤器）
- Controller 必须按子域拆分
- 接口必须保持单一职责
- 遵循 RESTful 风格设计接口
- Controller 保持精简（避免过大）
- 必须集成 Swagger 用于接口文档

---

## 禁止事项

- 禁止在 Controller 中编写业务逻辑
- 禁止直接访问 Repository 或数据库
- 禁止跨层调用（如直接调用 Domain 或 Infrastructure）
- 禁止返回 Domain 实体
- 禁止在 Controller 中进行复杂映射
- 禁止接口承担多个职责
- 禁止绕过 Application 层