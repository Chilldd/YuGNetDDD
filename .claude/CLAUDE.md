# CLAUDE.md

此文件为 Claude Code (claude.ai/code) 在此代码库中工作时提供指导。

## 项目概述

.NET 10 Web API 项目，采用 DDD + CQRS 架构。

### 项目结构
```
src/
├── YuG.Common/           ← 共享内核（Result 模式、值对象基类、Guard、扩展方法、通用异常、分页模型）
├── YuG.Domain/           ← 领域层（实体、值对象、领域事件、仓储接口）
├── YuG.Application/      ← 应用层（CQRS 命令/查询、管道行为、DTO）
├── YuG.Infrastructure/   ← 基础设施层（EF Core、仓储实现、外部服务）
└── YuG.Api/              ← 表现层（控制器、中间件、配置）
```

### 依赖方向
Api → Infrastructure → Application → Domain → Common（Common 无外部依赖）

### 参考文档
- [权限系统设计](docs/permission-system.md) — RBAC 权限模型、JWT 设计、后端鉴权、前端权限接口

## 常用命令

### 构建和运行
```bash
dotnet build                    # 构建解决方案
dotnet run --project YuG.Api    # 运行 API（默认端口可能变化）
dotnet watch --project YuG.Api  # 运行并支持开发时热重载
```

### 测试
```bash
dotnet test                     # 运行所有测试（如果添加了测试项目）
```

### 还原和清理
```bash
dotnet restore                  # 还原 NuGet 包
dotnet clean                    # 清理构建产物
```

## 工作模式

- 当接收到任务时，不要立刻开始工作，先思考需求是否了解清楚，需求模糊不确定时需要像用户提问获取完整需求。如果出现用户没有考虑到的情况，必须立刻指出。

## 开发规范

- 必须遵守单一职责原则
- 任务完成后必须编译通过没有报错才允许结束任务
- 如果涉及数据库实体字段变更，代码编写完成编译通过后，必须执行 `dotnet ef migrations add <迁移名称> --project src/YuG.Infrastructure --startup-project src/YuG.Api` 同步迁移
- 严禁删除本地 SQLite 数据库文件（`yug.db`、`cache.db` 等）
- 任务开发完成且编译通过后，必须提交代码到 git !!!
