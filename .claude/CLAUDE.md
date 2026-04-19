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
