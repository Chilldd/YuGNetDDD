---
paths:
  - "src/YuG.Infrastructure/**/*.cs"
---

# Infrastructure 层开发规范

Infrastructure 层负责领域层的持久化实现与外部服务调用，处于最底层，只被 Application 层（及更上层）引用。

---

## 一 仓储核心设计
Repository = 聚合根的持久化抽象（Persistence Boundary）

- 职责
  - 持久化与重建聚合根
  - 提供最小必要的加载与保存能力
  - 隐藏底层数据访问实现
- 不负责
  - 业务规则与领域行为（属于 Domain）
  - 复杂查询与报表（属于 Query 层）
  - 跨聚合业务编排（属于 Application）
- 按聚合根划分仓储: 一个聚合根 = 一个 Repository, Repository 只操作聚合根
- 聚合内部对象必须通过聚合根访问
- 事务由 Application 层控制, Repository 不负责事务开启与提交, 只参与事务
- 基类 Repository 方法标记为 virtual，子仓储可按需重写查询行为

---

## 二 目录结构

```
Infrastructure
├── Persistence        （持久化）
│   ├── 子域
│   │   ├── Configurations  （EF实体配置）
│   │   └── Repositories    （仓储）
├── HttpClients        （外部服务接口声明）
│   └── 外部服务名称
│       ├── Requests  （请求参数模型）
│       └── Responses （响应结果模型）
├── Services           （外部服务实现）
├── DomainEvents       （领域事件发布）
└── Migrations         （EF同步文件）
```

---

## 三 仓储规范

### 必须事项

- 每个聚合根必须有独立仓储
- 只提供最小必要方法
- 以聚合生命周期为核心（加载 / 保存 / 删除）
- 方法表达"数据访问"，而非"业务行为"
- 只返回聚合根，保证返回对象的完整性
- 加载的聚合必须处于一致状态
- 避免懒加载破坏领域模型
- 修改必须通过聚合方法完成
- 所有状态变更必须通过领域方法，Repository 不直接修改数据结构
- 多表 Join、统计、分页查询必须走 Query 层

### 禁止事项

- 禁止按数据表或模块划分仓储
- 禁止万能仓储
- 禁止返回 DTO / ViewModel / 基础数据结构
- 禁止返回"部分加载"的聚合
- 禁止出现业务规则，状态判断，业务流程
- 禁止在一个仓储中修改多个聚合，跨聚合逻辑必须在 Application 层完成
- 禁止开启或提交事务
- 禁止嵌套事务逻辑

---

## 四 HttpClients 规范

### 目录组织
- DTO 与服务端 API 的定义保持一致，与服务端各自的 DTO 解耦
- Refit 接口只包含方法签名和 HTTP 属性（`[Post]`、`[Get]`、`[Body]` 等）
- 不含任何业务逻辑
- 异步方法以 `Async` 后缀结尾
- Refit 客户端在调用方项目中通过 `AddRefitClient<T>()` 注册，不在此层注册
