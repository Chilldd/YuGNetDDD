---
paths:
  - "YuG.Api/Controllers/*Controller.cs"
---

# API 控制器开发规范

## 核心设计原则

- **单一职责**：控制器只负责接收请求、分发到 MediatR、返回响应
- **MediatR 分发**：所有业务逻辑通过 Command/Query 分发，禁止直接业务逻辑
- **DTO 隔离**：所有请求/响应使用 DTO，不返回 Domain 实体
- **无数据库直接访问**：禁止在控制器中访问 DbContext 或仓储实现

## 必须事项

- 所有控制器必须标注 `[ApiController]` 和 `[Route("api/[controller]")]`，继承 `ControllerBase`
- 通过构造函数注入 `IMediator`，所有业务逻辑通过 MediatR 分发
- 所有端点使用显式的 HTTP 方法特性（`[HttpGet]`、`[HttpPost]`、`[HttpPut]`、`[HttpDelete]`）
- 请求参数标注来源特性：`[FromBody]`、`[FromQuery]`、`[FromRoute]` 等
- 所有 DTO/Request/Response 必须定义在 Application 层
- 写操作返回 `ActionResult<T>`，使用正确的状态码（创建 `CreatedAtAction`、成功 `Ok`、无内容 `NoContent`）
- 读操作返回 `ActionResult<T>`，找不到返回 `NotFound()`
- 受保护端点使用 `[Authorize]` 和 `[Authorize(Roles = "...")]` 特性
- 从 `User.Claims` 中提取用户信息传递给 Command/Query

## 禁止事项

- 禁止在控制器中编写业务逻辑
- 禁止直接注入 DbContext、仓储实现、业务服务等基础设施类型
- 禁止直接访问 `HttpContext`（从 Claims 提取用户信息）
- 禁止在控制器文件中定义 DTO/Request/Response 类
- 禁止返回匿名对象和 Domain 实体
- 禁止在控制器中使用 `try-catch` 捕获业务异常（由全局异常中间件处理）
- 禁止非 RESTful 路由（`/doSomething`），使用名词路由（`POST /api/orders`）
- 禁止在控制器中注入超过 3 个依赖
- 禁止直接操作数据库或调用外部服务
- 禁止使用 `IActionResult` 而不指定泛型（使用 `ActionResult<T>`）
