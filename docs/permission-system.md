# 权限系统

基于 RBAC 的权限系统，资源采用 Menu → Page → Api 三层树结构，前后端共用同一套角色-资源关联数据。

---

## 核心概念

**User → Role → Resource：多对多关系**

Resource 三种类型，层级约束：根节点必须是 Menu → Menu 的子节点是 Page → Page 的子节点是 Page 或 Api → Api 不能有子节点。

资源核心字段：
- `PermissionCode` — Page/Api 类型的权限编码，格式如 `user:create`
- `Type` — Menu/Api/Page
- `ParentId` — 自引用，构建树结构

---

## 重要文件

```
Domain/
├── Identity/Entities/      Role.cs, User.cs
├── Identity/Repositories/  IRoleRepository.cs
│   └── GetByUserIdAsync()           // 查用户角色（无资源）
│   └── GetByUserIdWithResourcesAsync()  // 查用户角色+资源
└── Permission/Entities/    Resource.cs
    Permission/Enums/       ResourceType.cs, ResourceStatus.cs
    Permission/Repositories/IResourceRepository.cs
    Common/Interfaces/      IJwtTokenService.cs

Application/
├── Common/Interfaces/      IUserIdentity.cs    // UserId, Username, Roles
├── Identity/UserLogin/
│   ├── Login/              // 登录 Handler：查角色→写入 JWT claims
│   └── RefreshToken/       // 刷新令牌 Handler：同上
└── Permission/
    ├── Role/               // 角色 CRUD 命令/查询
    ├── Resource/           // 资源 CRUD 命令/查询
    └── UserPermission/
        ├── GetUserMenu/           // 查询当前用户的菜单树
        └── GetPageApiPermissions/ // 查询指定页面的 API 权限编码

Infrastructure/
├── Services/               JwtTokenService.cs
└── Persistence/
    ├── Identity/Repositories/  RoleRepository.cs
    └── Permission/Repositories/ResourceRepository.cs

Api/
├── Controllers/
│   ├── PermissionController.cs   // GET /api/permission/menus
│   │                            // GET /api/permission/pages/{id}/apis
│   ├── RoleController.cs         // 角色管理，标注了 [Authorize(Policy = "role:xxx")]
│   ├── ResourceController.cs     // 资源管理
│   └── AuthController.cs         // 登录/刷新/登出
├── Authorization/
│   ├── PermissionRequirement.cs           // 授权要求（持有 permissionCode）
│   ├── PermissionPolicyProvider.cs         // 策略翻译器
│   └── PermissionAuthorizationHandler.cs   // 授权处理器（DB 核验）
├── Services/
│   └── UserIdentity.cs           // IUserIdentity 实现
└── Extensions/
    ├── AuthenticationExtensions.cs   // 注册 JWT + 授权组件
    └── ServiceCollectionExtensions.cs    // 注册 IUserIdentity
```

---

## 关键实现说明

### 1. JWT 令牌

登录/刷新 token 时，通过 `IRoleRepository.GetByUserIdAsync()` 查询用户角色，将角色编码以多个 `ClaimTypes.Role` claims 写入 JWT。登录/刷新接口的响应体也返回 `roles` 字段，前端无需解码 JWT。

三个 claims：`sub`(userId) / `unique_name`(username) / `role`(多个)。

### 2. IUserIdentity

Scoped 服务，使用 `IHttpContextAccessor` 从当前请求的 JWT claims 中读取 UserId、Username、Roles。惰性缓存，每个属性首次访问时解析。控制器注入后直接使用，避免各控制器重复解析 claims。

### 3. 后端 API 鉴权

鉴权分三层：
- `[Authorize]` — 验证是否登录
- `[Authorize(Roles = "admin")]` — 基于角色（从 JWT claims 读取）
- `[Authorize(Policy = "role:create")]` — 基于权限编码（查 DB）

**为什么需要 `PermissionPolicyProvider`？**

正常情况下，`[Authorize(Policy = "role:create")]` 要求策略名必须在 `AddAuthorization` 中预注册：

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("role:create", ...);
    options.AddPolicy("role:delete", ...);
    // 几十个 API 权限策略，不可能逐个手写
});
```

但 API 级权限编码有几十个，不可能逐个静态注册。`PermissionPolicyProvider` 的作用就是拦截所有策略名，**查不到静态注册的，一律视为权限编码，统一走 `PermissionRequirement` 处理**：

```
[Authorize(Policy = "role:create")]  →  PolicyProvider 查静态注册表
  ├─ 有注册 → 走对应策略
  └─ 没注册 → 视为权限编码 → 创建 PermissionRequirement("role:create")
                                 → 派发给 PermissionAuthorizationHandler 做 DB 核验
```

权限编码鉴权的完整流程：
1. `[Authorize(Policy = "role:create")]` 触发授权管道
2. `PermissionPolicyProvider.GetPolicyAsync("role:create")` 被调用
3. 检查 `_options.GetPolicy("role:create")` — 未在 `AddAuthorization` 中静态注册，返回 null
4. 动态创建 `AuthorizationPolicyBuilder().AddRequirements(new PermissionRequirement("role:create"))`
5. ASP.NET Core 将 `PermissionRequirement` 派发给 `PermissionAuthorizationHandler`
6. Handler 通过 `IRoleRepository.GetByUserIdWithResourcesAsync()` 查出当前用户的权限编码列表，匹配则放行
7. Handler 是 Scoped 服务，权限列表在同一次请求内缓存，不重复查库

### 4. 前端权限接口

两个接口供前端查询当前用户权限：

- `GET /api/permission/menus` — 返回 Menu → Page 树，供渲染左侧菜单
- `GET /api/permission/pages/{pageId}/apis` — 返回该页面下的 API 权限编码列表，供控制按钮显隐

权限数据来自用户的角色-资源关联，与后端鉴权使用同一份数据，故前后端权限天然对齐。

### 5. 资源同步

`POST /api/tool/sync-api-resources` 扫描所有 Controller 的 Action，生成 API 类型的 Resource 记录用于初始化权限数据。扫描器根据 C# 方法名生成权限编码，写入 `Resource.PermissionCode` 字段。`Resource.Code` 是资源自身标识，跟鉴权无关。

**权限编码规则：`{控制器名小写}:{方法名小写}`**

```
例：RoleController.Get()  →  role:get
```

`[Authorize(Policy = "role:get")]` 的值必须与扫描器生成的 `PermissionCode` 一致。

### 生成示例

| 控制器 | C# 方法 | 路由 | 权限编码 |
|--------|---------|------|---------|
| RoleController | `Get()` | GET /api/role | `role:get` |
| RoleController | `GetById()` | GET /api/role/{id} | `role:getbyid` |
| RoleController | `Create()` | POST /api/role | `role:create` |
| ResourceController | `Get()` | GET /api/management/resources | `resource:get` |
| ResourceController | `GetTree()` | GET /api/management/resources/tree | `resource:gettree` |
| UserController | `SetRoles()` | PUT /api/user/{userId}/roles | `user:setroles` |

