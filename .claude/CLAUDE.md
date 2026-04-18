# CLAUDE.md

此文件为 Claude Code (claude.ai/code) 在此代码库中工作时提供指导。

## 项目概述

.NET 10 Web API 项目，采用 DDD + CQRS 架构。

### 项目结构
```
src/
├── YuG.Domain/           ← 领域层（实体、值对象、领域事件、仓储接口）
├── YuG.Application/      ← 应用层（CQRS 命令/查询、管道行为、DTO）
├── YuG.Infrastructure/   ← 基础设施层（EF Core、仓储实现、外部服务）
└── YuG.Api/              ← 表现层（控制器、中间件、配置）
```

### 依赖方向
Api → Infrastructure → Application → Domain（Domain 无外部依赖）

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

## 工作模式（必须严格遵守）

- 当接收到任务时，不要立刻开始工作，先思考需求是否了解清楚，需求模糊不确定时需要像用户提问获取完整需求。如果出现用户没有考虑到的情况，必须立刻指出。

## 开发规范（必须严格遵守）

- 必须遵守单一职责原则
- 任务完成后必须编译通过没有报错才允许结束任务

## 详细开发规则

项目详细开发规则定义在 `.claude/rules/` 目录下，请严格按照这些规则进行开发：

### 核心架构规则
- **[DTO 规范](.claude/rules/dto.md)** - 数据传输对象设计规范
- **[API 控制器规范](.claude/rules/api-controller.md)** - 表现层控制器开发规范
- **[仓储规范](.claude/rules/repository.md)** - 仓储接口与实现规范
- **[ORM 实体规范](.claude/rules/orm-entity.md)** - EF Core 实体定义规范

### CQRS 模式规则
- **[Command 规范](.claude/rules/cqrs/cqrs-command.md)** - 命令类开发规范
- **[Query 规范](.claude/rules/cqrs/cqrs-query.md)** - 查询类开发规范
- **[Handler 规范](.claude/rules/cqrs/cqrs-handler.md)** - 处理器开发规范
- **[Validator 规范](.claude/rules/cqrs/cqrs-validator.md)** - 验证器开发规范

### 领域层规则
- **[领域实体规范](.claude/rules/domain/domain-entity.md)** - 聚合根与实体开发规范
- **[值对象规范](.claude/rules/domain/domain-value-object.md)** - 值对象设计规范
- **[领域事件规范](.claude/rules/domain/domain-event.md)** - 领域事件开发规范
- **[领域服务规范](.claude/rules/domain/domain-service.md)** - 领域服务开发规范

### 通用代码规则
- **[C# 代码风格](.claude/rules/code-style.md)** - 命名、格式、异步编程等规范