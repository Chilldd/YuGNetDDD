# 认证授权功能设计文档

**日期**: 2026-04-13
**作者**: Claude
**状态**: 已批准

## 1. 概述

本文档描述了 YuG API 项目的认证授权功能设计，采用 JWT (JSON Web Token) 方式实现用户认证，支持登录、登出和令牌刷新功能。

## 2. 背景

当前项目是一个采用 DDD + CQRS 架构的 .NET 10 Web API 项目，需要添加用户认证功能以确保 API 安全性。设计遵循现有架构模式，保持代码风格一致性。

## 3. 需求

### 3.1 功能需求
- 用户登录：验证用户名密码，返回 JWT Token 和 RefreshToken
- 令牌刷新：使用 RefreshToken 获取新的访问令牌
- 用户登出：撤销 RefreshToken
- 数据库初始化时创建默认管理员账户

### 3.2 非功能需求
- Token 过期时间：5小时（可配置）
- 密码安全：使用 BCrypt 哈希存储
- API 保护：使用声明式授权（[Authorize]），暂时默认允许所有请求

## 4. 架构设计

```
┌─────────────────────────────────────────────────────────────┐
│                         YuG.Api                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       │
│  │ AuthController│  │ JWT Middleware│ │  Swagger UI   │       │
│  └───────┬──────┘  └──────┬───────┘  └──────────────┘       │
└──────────┼──────────────────┼───────────────────────────────┘
           │              MediatR                               
           │                  │                                  
┌──────────┼──────────────────┼───────────────────────────────┐
│          │         YuG.Application                           │
│  ┌───────▼──────┐  ┌────────▼────────┐  ┌────────────────┐  │
│  │ LoginCommand │  │RefreshTokenCommand│  │ LogoutCommand  │  │
│  └──────────────┘  └─────────────────┘  └────────────────┘  │
└──────────────────────┬──────────────────────────────────────┘
                       │                  
┌──────────────────────┼──────────────────────────────────────┐
│         YuG.Domain   │                                       │
│  ┌───────────────────▼───────┐                              │
│  │       User (Entity)        │                              │
│  │  - Id, Username, Password  │                              │
│  │  - RefreshTokens           │                              │
│  └───────────────────────────┘                              │
└──────────────────────┬──────────────────────────────────────┘
                       │                  
┌──────────────────────┼──────────────────────────────────────┐
│   YuG.Infrastructure │                                       │
│  ┌───────────────────▼───────┐  ┌────────────────────────┐  │
│  │     JwtTokenService       │  │ UserRepository         │  │
│  │  - GenerateToken          │  │  - EF Core implementation│  │
│  │  - ValidateToken          │  └────────────────────────┘  │
│  │  - GenerateRefreshToken   │                              │
│  └───────────────────────────┘                              │
└──────────────────────────────────────────────────────────────┘
```

## 5. 组件设计

### 5.1 Domain 层

#### User 实体
```csharp
public class User : BaseEntity
{
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public List<RefreshToken> RefreshTokens { get; private set; }
}
```

#### RefreshToken 值对象
```csharp
public record RefreshToken
{
    public string Token { get; init; }
    public DateTime ExpiresAt { get; init; }
    public bool IsRevoked { get; private set; }
}
```

#### 接口定义
- `IUserRepository`：用户仓储接口
- `IJwtTokenService`：JWT 令牌服务接口
- `IPasswordHasher`：密码哈希服务接口

### 5.2 Application 层

#### 命令定义
- `LoginCommand`：登录命令
  - Request: `{ username, password }`
  - Response: `{ accessToken, refreshToken, expiresAt }`
- `RefreshTokenCommand`：刷新令牌命令
  - Request: `{ refreshToken }`
  - Response: `{ accessToken, refreshToken, expiresAt }`
- `LogoutCommand`：登出命令
  - Request: `{ refreshToken }`
  - Response: `Success`

### 5.3 Infrastructure 层

#### JwtTokenService 实现
- 生成 JWT AccessToken
- 验证令牌
- 生成 RefreshToken（GUID 格式）

#### UserRepository 实现
- 基于 EF Core 的仓储实现
- 继承 `IRepository<User>`

#### 数据库迁移
- 创建 Users 表
- 创建 RefreshTokens 表

### 5.4 Api 层

#### AuthController 端点
- `POST /api/auth/login` - 用户登录
- `POST /api/auth/refresh` - 刷新令牌
- `POST /api/auth/logout` - 用户登出

#### 配置
```json
"Jwt": {
  "SecretKey": "your-256-bit-secret-key",
  "Issuer": "YuG.Api",
  "Audience": "YuG.Client",
  "ExpirationMinutes": 300
}
```

#### Swagger 配置
- 启用 "Authorize" 按钮
- 支持 Bearer Token 认证

## 6. 数据流

### 6.1 登录流程
```
Client                    AuthController           Command Handler            JwtTokenService
  │                              │                         │                         │
  ├─ POST /api/auth/login ──────>│                         │                         │
  │    {username, password}       │                         │                         │
  │                              │                         │                         │
  │                              ├─ LoginCommand ─────────>│                         │
  │                              │                         │                         │
  │                              │                         ├─ ValidateUser ─────────>│
  │                              │                         │                         │
  │                              │                         │<────────────────────────┤
  │                              │                         │                         │
  │                              │                         ├─ GenerateTokens ───────>│
  │                              │                         │                         │
  │                              │                         │<─ (accessToken,         │
  │                              │                         │    refreshToken) ───────┤
  │                              │                         │                         │
  │                              │<─ LoginResponse ─────────┤                         │
  │<─────────────────────────────┤                         │                         │
```

### 6.2 刷新令牌流程
```
Client                    AuthController           Command Handler
  │                              │                         │
  ├─ POST /api/auth/refresh ────>│                         │
  │    {refreshToken}             │                         │
  │                              │                         │
  │                              ├─ RefreshTokenCommand ──>│
  │                              │                         │
  │                              │                         ├─ Validate RefreshToken
  │                              │                         │
  │                              │                         ├─ Revoke old token
  │                              │                         │
  │                              │                         ├─ Generate new tokens
  │                              │                         │
  │                              │<─ LoginResponse ─────────┤
  │<─────────────────────────────┤                         │
```

### 6.3 登出流程
```
Client                    AuthController           Command Handler
  │                              │                         │
  ├─ POST /api/auth/logout ─────>│                         │
  │    {refreshToken}             │                         │
  │                              │                         │
  │                              ├─ LogoutCommand ─────────>│
  │                              │                         │
  │                              │                         ├─ Revoke RefreshToken
  │                              │                         │
  │                              │<─ Success ───────────────┤
  │<─────────────────────────────┤                         │
```

## 7. 安全考虑

### 7.1 密码存储
- 使用 BCrypt 算法进行密码哈希
- 自动处理盐值
- 工作因子设置为 12

### 7.2 令牌安全
- JWT 使用 HS256 算法签名
- Secret Key 至少 256 位
- RefreshToken 使用 GUID，存储时哈希

### 7.3 默认管理员
- 用户名：admin
- 密码：admin123
- 仅在数据库为空时创建

## 8. 文件结构

```
src/
├── YuG.Domain/
│   ├── Entities/
│   │   └── User.cs
│   ├── ValueObjects/
│   │   └── RefreshToken.cs
│   ├── Interfaces/
│   │   ├── IUserRepository.cs
│   │   ├── IJwtTokenService.cs
│   │   └── IPasswordHasher.cs
│   └── Exceptions/
│       └── InvalidCredentialsException.cs
│
├── YuG.Application/
│   ├── Commands/
│   │   ├── Auth/
│   │   │   ├── Login/
│   │   │   │   ├── LoginCommand.cs
│   │   │   │   ├── LoginCommandHandler.cs
│   │   │   │   └── LoginCommandValidator.cs
│   │   │   ├── RefreshToken/
│   │   │   │   ├── RefreshTokenCommand.cs
│   │   │   │   ├── RefreshTokenCommandHandler.cs
│   │   │   │   └── RefreshTokenCommandValidator.cs
│   │   │   └── Logout/
│   │   │       ├── LogoutCommand.cs
│   │   │       ├── LogoutCommandHandler.cs
│   │   │       └── LogoutCommandValidator.cs
│   │   └── DTOs/
│   │       └── LoginResponse.cs
│
├── YuG.Infrastructure/
│   ├── Services/
│   │   ├── JwtTokenService.cs
│   │   └── PasswordHasher.cs
│   ├── Repositories/
│   │   └── UserRepository.cs
│   ├── Persistence/
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs
│   │   │   └── RefreshTokenConfiguration.cs
│   │   └── ApplicationDbContext.cs (更新)
│
└── YuG.Api/
    ├── Controllers/
    │   └── AuthController.cs (更新)
    └── appsettings.json (更新)
```

## 9. 验收标准

1. 用户可以使用用户名和密码成功登录，获取有效的 JWT Token
2. JWT Token 可以用于认证保护的 API 端点
3. RefreshToken 可以用于获取新的 AccessToken
4. 登出后 RefreshToken 被撤销，无法继续使用
5. 数据库初始化时创建默认管理员账户
6. Swagger UI 支持 "Authorize" 按钮进行测试
7. 所有代码遵循项目代码规范
8. 编译通过无错误

## 10. 未来扩展

- 基于角色的授权 (RBAC)
- 用户注册功能
- 密码重置功能
- 多因素认证 (MFA)
- 令牌黑名单
- 登录历史记录
