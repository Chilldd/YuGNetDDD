---
paths:
  - "YuG.Application/DTOs/**/*.cs"
---

# DTO（数据传输对象）开发规范

## 核心设计原则

| 特性 | 说明 |
|-----|------|
| **仅用于传输** | 不包含业务逻辑，纯数据容器 |
| **简化视图** | 只包含调用方需要的字段，隐藏不必要的细节 |
| **解耦表现和领域** | API 需求不同时，不需要修改 Domain 实体 |
| **类型安全** | 强类型，比 dynamic 或 JSON 更安全 |
| **API 文档** | DTO 属性作为 API 契约 |

### DTO 的三种主要类型

| 类型 | 用途 | 来源 | 去向 |
|------|------|------|------|
| **Request DTO** | 接收客户端输入 | Client → Api | Api → Handler |
| **Response DTO** | 返回数据给客户端 | Handler → Api | Api → Client |
| **Internal DTO** | 层间通信 | Handler → Handler | Application 内部 |

## 必须事项

### Request DTO（请求数据）

- 用于接收客户端输入
- 定义在 Application 层 `DTOs/` 目录

### Response DTO（响应数据）

- 用于返回数据给客户端
- 属性支持反序列化
- 不返回密码、密钥等敏感数据

### Internal DTO（内部传输）

- 用于 Application 层内部通信
- Handler 之间的数据传递

## 禁止事项

- **禁止在 DTO 中包含业务逻辑**（验证、计算等）
- **禁止 DTO 继承 Domain 实体**，两者完全独立
- **禁止在 API 直接返回 Domain 实体**，必须映射为 DTO
- **禁止在 Request DTO 中使用过度复杂的嵌套结构**，应该平面化
- **禁止在 DTO 中暴露敏感数据**（密码、内部ID、密钥等）
- **禁止 DTO 持有对 Domain 对象的引用**，只传输数据
- **禁止在不同层中共享同一个 DTO 类**（如在 Infrastructure 中引用 Application DTO）
- **禁止创建过度通用的 DTO**（如 `dynamic` 或 `object`），应该使用强类型
- **禁止 DTO 的属性冗长复杂**（超过 20 个属性），应考虑拆分
- **禁止 DTO 映射时丢失错误处理**，应处理为 null 或无效的数据
