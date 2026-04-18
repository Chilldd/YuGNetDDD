# 认证授权功能实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-step. Steps use checkbox (`- [ ]`) syntax for tracking.

**目标:** 为 YuG API 实现 JWT 认证授权功能，包括登录、登出、令牌刷新和默认管理员账户创建

**架构:** 遵循 DDD + CQRS 架构，Domain 层定义实体和接口，Application 层处理命令和查询，Infrastructure 层实现具体服务，Api 层提供端点

**技术栈:** .NET 10, EF Core, MediatR, FluentValidation, BCrypt.Net, Microsoft.AspNetCore.Authentication.JwtBearer

---

## 文件结构

```
src/
├── YuG.Domain/
│   ├── Entities/
│   │   └── User.cs                           (新建) 用户实体
│   ├── ValueObjects/
│   │   └── RefreshToken.cs                   (新建) 刷新令牌值对象
│   ├── Interfaces/
│   │   ├── IUserRepository.cs                (新建) 用户仓储接口
│   │   ├── IJwtTokenService.cs               (新建) JWT令牌服务接口
│   │   └── IPasswordHasher.cs                (新建) 密码哈希服务接口
│   └── Exceptions/
│       └── InvalidCredentialsException.cs    (新建) 无效凭据异常
│
├── YuG.Application/
│   ├── Commands/
│   │   └── Auth/
│   │       ├── Login/
│   │       │   ├── LoginCommand.cs           (新建) 登录命令
│   │       │   ├── LoginCommandHandler.cs    (新建) 登录命令处理器
│   │       │   └── LoginCommandValidator.cs  (新建) 登录命令验证器
│   │       ├── RefreshToken/
│   │       │   ├── RefreshTokenCommand.cs    (新建) 刷新令牌命令
│   │       │   ├── RefreshTokenCommandHandler.cs (新建) 刷新令牌处理器
│   │       │   └── RefreshTokenCommandValidator.cs (新建) 刷新令牌验证器
│   │       └── Logout/
│   │           ├── LogoutCommand.cs          (新建) 登出命令
│   │           ├── LogoutCommandHandler.cs   (新建) 登出命令处理器
│   │           └── LogoutCommandValidator.cs (新建) 登出命令验证器
│   └── DTOs/
│       └── Responses/
│           └── LoginResponse.cs              (新建) 登录响应DTO
│
├── YuG.Infrastructure/
│   ├── Services/
│   │   ├── JwtTokenService.cs                (新建) JWT令牌服务实现
│   │   └── PasswordHasher.cs                 (新建) 密码哈希服务实现
│   ├── Repositories/
│   │   └── UserRepository.cs                 (新建) 用户仓储实现
│   ├── Persistence/
│   │   ├── Configurations/
│   │   │   ├── UserConfiguration.cs          (新建) 用户实体配置
│   │   │   └── RefreshTokenConfiguration.cs  (新建) 刷新令牌配置
│   │   └── ApplicationDbContext.cs           (修改) 添加DbSet<User>
│   └── DependencyInjection.cs                (修改) 注册新服务
│
└── YuG.Api/
    ├── Controllers/
    │   └── AuthController.cs                 (修改) 实现登录/登出端点
    ├── appsettings.json                      (修改) 添加JWT配置
    └── Program.cs                            (修改) 添加JWT认证
```

---

## Task 1: 添加 NuGet 包依赖

**Files:**
- Modify: `src/YuG.Infrastructure/YuG.Infrastructure.csproj`
- Modify: `YuG.Api/YuG.Api.csproj`

- [ ] **Step 1: 添加 Infrastructure 层所需的 NuGet 包**

编辑 `src/YuG.Infrastructure/YuG.Infrastructure.csproj`，在 `<ItemGroup>` 中添加：

```xml
<PackageReference Include="BCrypt.Net-Next" Version="3.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="10.0.5" />
```

- [ ] **Step 2: 还原 NuGet 包**

运行: `dotnet restore`
预期输出: 还原成功，无错误

- [ ] **Step 3: 编译验证**

运行: `dotnet build`
预期输出: 编译成功

---

## Task 2: 创建 Domain 层值对象 - RefreshToken

**Files:**
- Create: `src/YuG.Domain/ValueObjects/RefreshToken.cs`

- [ ] **Step 1: 创建 RefreshToken 值对象**

创建 `src/YuG.Domain/ValueObjects/RefreshToken.cs`：

```csharp
namespace YuG.Domain.ValueObjects;

/// <summary>
/// 刷新令牌值对象
/// </summary>
public record RefreshToken
{
    /// <summary>
    /// 令牌值
    /// </summary>
    public string Token { get; init; } = string.Empty;

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; init; }

    /// <summary>
    /// 是否已撤销
    /// </summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// 撤销令牌
    /// </summary>
    public void Revoke()
    {
        IsRevoked = true;
    }

    /// <summary>
    /// 检查令牌是否有效
    /// </summary>
    public bool IsValid()
    {
        return !IsRevoked && DateTime.UtcNow < ExpiresAt;
    }
}
```

- [ ] **Step 2: 编译验证**

运行: `dotnet build src/YuG.Domain/YuG.Domain.csproj`
预期输出: 编译成功

- [ ] **Step 3: 签入**

```bash
git add src/YuG.Domain/ValueObjects/RefreshToken.cs
git commit -m "feat(domain): 添加 RefreshToken 值对象"
```

---

## Task 3: 创建 Domain 层异常 - InvalidCredentialsException

**Files:**
- Create: `src/YuG.Domain/Exceptions/InvalidCredentialsException.cs`

- [ ] **Step 1: 创建无效凭据异常类**

创建 `src/YuG.Domain/Exceptions/InvalidCredentialsException.cs`：

```csharp
namespace YuG.Domain.Exceptions;

/// <summary>
/// 无效凭据异常，当用户名或密码不正确时抛出
/// </summary>
public class InvalidCredentialsException : DomainException
{
    /// <summary>
    /// 初始化无效凭据异常
    /// </summary>
    public InvalidCredentialsException()
        : base("用户名或密码不正确")
    {
    }

    /// <summary>
    /// 初始化无效凭据异常
    /// </summary>
    /// <param name="message">错误消息</param>
    public InvalidCredentialsException(string message)
        : base(message)
    {
    }
}
```

- [ ] **Step 2: 编译验证**

运行: `dotnet build src/YuG.Domain/YuG.Domain.csproj`
预期输出: 编译成功

- [ ] **Step 3: 签入**

```bash
git add src/YuG.Domain/Exceptions/InvalidCredentialsException.cs
git commit -m "feat(domain): 添加 InvalidCredentialsException"
```

---

## Task 4: 创建 Domain 层实体 - User

**Files:**
- Create: `src/YuG.Domain/Entities/User.cs`

- [ ] **Step 1: 创建 User 实体**

创建 `src/YuG.Domain/Entities/User.cs`：

```csharp
using YuG.Domain.Common;
using YuG.Domain.ValueObjects;

namespace YuG.Domain.Entities;

/// <summary>
/// 用户实体
/// </summary>
public class User : BaseEntity
{
    private readonly List<RefreshToken> _refreshTokens = [];

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; private set; } = string.Empty;

    /// <summary>
    /// 密码哈希
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// 刷新令牌集合（只读）
    /// </summary>
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    /// <summary>
    /// 创建用户（用于ORM）
    /// </summary>
    private User()
    {
    }

    /// <summary>
    /// 创建新用户
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="passwordHash">密码哈希</param>
    public User(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="passwordHasher">密码哈希服务</param>
    /// <param name="password">明文密码</param>
    /// <returns>密码是否正确</returns>
    public bool VerifyPassword(IPasswordHasher passwordHasher, string password)
    {
        return passwordHasher.Verify(password, PasswordHash);
    }

    /// <summary>
    /// 添加刷新令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    public void AddRefreshToken(RefreshToken refreshToken)
    {
        // 清理过期的令牌
        _refreshTokens.RemoveAll(t => t.ExpiresAt < DateTime.UtcNow || t.IsRevoked);

        // 添加新令牌
        _refreshTokens.Add(refreshToken);
    }

    /// <summary>
    /// 撤销刷新令牌
    /// </summary>
    /// <param name="token">令牌值</param>
    /// <returns>是否成功撤销</returns>
    public bool RevokeRefreshToken(string token)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
        if (refreshToken == null)
        {
            return false;
        }

        refreshToken.Revoke();
        return true;
    }

    /// <summary>
    /// 获取有效的刷新令牌
    /// </summary>
    /// <param name="token">令牌值</param>
    /// <returns>刷新令牌，不存在或无效则返回 null</returns>
    public RefreshToken? GetValidRefreshToken(string token)
    {
        return _refreshTokens.FirstOrDefault(t => t.Token == token && t.IsValid());
    }
}
```

- [ ] **Step 2: 创建密码哈希服务接口**

创建 `src/YuG.Domain/Interfaces/IPasswordHasher.cs`：

```csharp
namespace YuG.Domain.Interfaces;

/// <summary>
/// 密码哈希服务接口
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// 对密码进行哈希
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>密码哈希</returns>
    string Hash(string password);

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="hash">密码哈希</param>
    /// <returns>密码是否正确</returns>
    bool Verify(string password, string hash);
}
```

- [ ] **Step 3: 编译验证**

运行: `dotnet build src/YuG.Domain/YuG.Domain.csproj`
预期输出: 编译成功

- [ ] **Step 4: 签入**

```bash
git add src/YuG.Domain/Entities/User.cs src/YuG.Domain/Interfaces/IPasswordHasher.cs
git commit -m "feat(domain): 添加 User 实体和 IPasswordHasher 接口"
```

---

## Task 5: 创建 Domain 层接口 - IUserRepository 和 IJwtTokenService

**Files:**
- Create: `src/YuG.Domain/Interfaces/IUserRepository.cs`
- Create: `src/YuG.Domain/Interfaces/IJwtTokenService.cs`

- [ ] **Step 1: 创建用户仓储接口**

创建 `src/YuG.Domain/Interfaces/IUserRepository.cs`：

```csharp
using YuG.Domain.Entities;
using YuG.Domain.Repositories;

namespace YuG.Domain.Interfaces;

/// <summary>
/// 用户仓储接口
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户实体，不存在则返回 null</returns>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户名是否存在</returns>
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 2: 创建 JWT 令牌服务接口**

创建 `src/YuG.Domain/Interfaces/IJwtTokenService.cs`：

```csharp
namespace YuG.Domain.Interfaces;

/// <summary>
/// JWT 令牌服务接口
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// 生成 JWT 访问令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="username">用户名</param>
    /// <returns>JWT 访问令牌</returns>
    string GenerateAccessToken(Guid userId, string username);

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    /// <param name="expirationDays">过期天数</param>
    /// <returns>刷新令牌值</returns>
    string GenerateRefreshToken(int expirationDays = 7);

    /// <summary>
    /// 从 JWT 令牌中提取用户ID
    /// </summary>
    /// <param name="token">JWT 令牌</param>
    /// <returns>用户ID</returns>
    Guid? GetUserIdFromToken(string token);

    /// <summary>
    /// 验证令牌是否有效
    /// </summary>
    /// <param name="token">JWT 令牌</param>
    /// <returns>令牌是否有效</returns>
    bool ValidateToken(string token);
}
```

- [ ] **Step 3: 编译验证**

运行: `dotnet build src/YuG.Domain/YuG.Domain.csproj`
预期输出: 编译成功

- [ ] **Step 4: 签入**

```bash
git add src/YuG.Domain/Interfaces/IUserRepository.cs src/YuG.Domain/Interfaces/IJwtTokenService.cs
git commit -m "feat(domain): 添加 IUserRepository 和 IJwtTokenService 接口"
```

---

## Task 6: 创建 Infrastructure 层服务 - PasswordHasher

**Files:**
- Create: `src/YuG.Infrastructure/Services/PasswordHasher.cs`

- [ ] **Step 1: 创建密码哈希服务实现**

创建 `src/YuG.Infrastructure/Services/PasswordHasher.cs`：

```csharp
using YuG.Domain.Interfaces;

namespace YuG.Infrastructure.Services;

/// <summary>
/// BCrypt 密码哈希服务实现
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    /// <summary>
    /// 对密码进行哈希
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <returns>密码哈希</returns>
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// 验证密码
    /// </summary>
    /// <param name="password">明文密码</param>
    /// <param name="hash">密码哈希</param>
    /// <returns>密码是否正确</returns>
    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
```

- [ ] **Step 2: 编译验证**

运行: `dotnet build src/YuG.Infrastructure/YuG.Infrastructure.csproj`
预期输出: 编译成功

- [ ] **Step 3: 签入**

```bash
git add src/YuG.Infrastructure/Services/PasswordHasher.cs
git commit -m "feat(infrastructure): 添加 PasswordHasher 服务"
```

---

## Task 7: 创建 Infrastructure 层服务 - JwtTokenService

**Files:**
- Create: `src/YuG.Infrastructure/Services/JwtTokenService.cs`

- [ ] **Step 1: 创建 JWT 令牌服务实现**

创建 `src/YuG.Infrastructure/Services/JwtTokenService.cs`：

```csharp
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using YuG.Domain.Interfaces;

namespace YuG.Infrastructure.Services;

/// <summary>
/// JWT 令牌配置选项
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// 密钥
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 签发者
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// 受众
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// 过期时间（分钟）
    /// </summary>
    public int ExpirationMinutes { get; set; } = 300;
}

/// <summary>
/// JWT 令牌服务实现
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    /// <summary>
    /// 初始化 JWT 令牌服务
    /// </summary>
    /// <param name="options">JWT 配置选项</param>
    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// 生成 JWT 访问令牌
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="username">用户名</param>
    /// <returns>JWT 访问令牌</returns>
    public string GenerateAccessToken(Guid userId, string username)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_options.SecretKey);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = _options.Issuer,
            Audience = _options.Audience
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// 生成刷新令牌
    /// </summary>
    /// <param name="expirationDays">过期天数</param>
    /// <returns>刷新令牌值</returns>
    public string GenerateRefreshToken(int expirationDays = 7)
    {
        return Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// 从 JWT 令牌中提取用户ID
    /// </summary>
    /// <param name="token">JWT 令牌</param>
    /// <returns>用户ID</returns>
    public Guid? GetUserIdFromToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            var subClaim = jsonToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            if (subClaim != null && Guid.TryParse(subClaim.Value, out var userId))
            {
                return userId;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 验证令牌是否有效
    /// </summary>
    /// <param name="token">JWT 令牌</param>
    /// <returns>令牌是否有效</returns>
    public bool ValidateToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_options.SecretKey);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _options.Issuer,
                ValidateAudience = true,
                ValidAudience = _options.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
```

- [ ] **Step 2: 编译验证**

运行: `dotnet build src/YuG.Infrastructure/YuG.Infrastructure.csproj`
预期输出: 编译成功

- [ ] **Step 3: 签入**

```bash
git add src/YuG.Infrastructure/Services/JwtTokenService.cs
git commit -m "feat(infrastructure): 添加 JwtTokenService 服务"
```

---

## Task 8: 创建 Infrastructure 层仓储 - UserRepository

**Files:**
- Create: `src/YuG.Infrastructure/Repositories/UserRepository.cs`

- [ ] **Step 1: 创建用户仓储实现**

创建 `src/YuG.Infrastructure/Repositories/UserRepository.cs`：

```csharp
using Microsoft.EntityFrameworkCore;
using YuG.Application.Interfaces;
using YuG.Domain.Entities;
using YuG.Domain.Interfaces;
using YuG.Domain.Repositories;

namespace YuG.Infrastructure.Repositories;

/// <summary>
/// 用户仓储实现
/// </summary>
public class UserRepository : Repository<User>, IUserRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// 初始化用户仓储
    /// </summary>
    /// <param name="context">数据库上下文</param>
    public UserRepository(ApplicationDbContext context)
        : base(context)
    {
        _context = context;
    }

    /// <summary>
    /// 根据用户名获取用户
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户实体，不存在则返回 null</returns>
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .Include(u => ((User)u).RefreshTokens)
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    /// <summary>
    /// 检查用户名是否存在
    /// </summary>
    /// <param name="username">用户名</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>用户名是否存在</returns>
    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AnyAsync(u => u.Username == username, cancellationToken);
    }
}
```

- [ ] **Step 2: 编译验证**

运行: `dotnet build src/YuG.Infrastructure/YuG.Infrastructure.csproj`
预期输出: 编译成功（可能会有错误，因为 ApplicationDbContext 还没有 Users DbSet，在下一个任务中修复）

- [ ] **Step 3: 签入**

```bash
git add src/YuG.Infrastructure/Repositories/UserRepository.cs
git commit -m "feat(infrastructure): 添加 UserRepository 实现"
```

---

## Task 9: 配置 EF Core 实体映射和更新 ApplicationDbContext

**Files:**
- Create: `src/YuG.Infrastructure/Persistence/Configurations/UserConfiguration.cs`
- Create: `src/YuG.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`
- Modify: `src/YuG.Infrastructure/Persistence/ApplicationDbContext.cs`

- [ ] **Step 1: 创建 User 实体配置**

创建 `src/YuG.Infrastructure/Persistence/Configurations/UserConfiguration.cs`：

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using YuG.Domain.Entities;

namespace YuG.Infrastructure.Persistence.Configurations;

/// <summary>
/// User 实体 EF Core 配置
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    /// <summary>
    /// 配置 User 实体
    /// </summary>
    /// <param name="builder">实体类型构建器</param>
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Username)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(u => u.Username)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .HasMaxLength(500)
            .IsRequired();

        builder.OwnsMany(u => u.RefreshTokens, rt =>
        {
            rt.WithOwner()
                .HasForeignKey("UserId");

            rt.HasKey("Token");

            rt.Property(r => r.Token)
                .HasMaxLength(100)
                .IsRequired();

            rt.Property(r => r.ExpiresAt)
                .IsRequired();

            rt.Property(r => r.IsRevoked)
                .IsRequired();

            rt.Property(r => r.CreatedAt)
                .IsRequired();

            rt.Ignore(r => r.IsValid());
        });
    }
}
```

- [ ] **Step 2: 创建 RefreshToken 值对象配置**

创建 `src/YuG.Infrastructure/Persistence/Configurations/RefreshTokenConfiguration.cs`：

```csharp
using Microsoft.EntityFrameworkCore;
using YuG.Infrastructure.Persistence;

namespace YuG.Infrastructure.Persistence.Configurations;

/// <summary>
/// RefreshToken 值对象 EF Core 配置
/// </summary>
public class RefreshTokenConfiguration
{
    /// <summary>
    /// 配置 RefreshToken 映射（在 UserConfiguration 中调用）
    /// </summary>
    /// <param name="modelBuilder">模型构建器</param>
    public static void Configure(ModelBuilder modelBuilder)
    {
        // 配置在 UserConfiguration 中完成
        // 这里保留作为备用
    }
}
```

- [ ] **Step 3: 更新 ApplicationDbContext 添加 Users DbSet**

编辑 `src/YuG.Infrastructure/Persistence/ApplicationDbContext.cs`，在类中添加：

```csharp
/// <summary>
/// 用户数据集
/// </summary>
public DbSet<User> Users => Set<User>();
```

并更新 `OnModelCreating` 方法：

```csharp
/// <summary>
/// 配置实体映射
/// </summary>
/// <param name="modelBuilder">模型构建器</param>
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // 应用实体配置
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
}
```

- [ ] **Step 4: 编译验证**

运行: `dotnet build src/YuG.Infrastructure/YuG.Infrastructure.csproj`
预期输出: 编译成功

- [ ] **Step 5: 签入**

```bash
git add src/YuG.Infrastructure/Persistence/Configurations/ src/YuG.Infrastructure/Persistence/ApplicationDbContext.cs
git commit -m "feat(infrastructure): 添加 User 实体配置和 DbSet"
```

---

## Task 10: 更新 Infrastructure 层依赖注入

**Files:**
- Modify: `src/YuG.Infrastructure/DependencyInjection.cs`

- [ ] **Step 1: 更新 DependencyInjection.cs**

编辑 `src/YuG.Infrastructure/DependencyInjection.cs`，在 `AddInfrastructure` 方法中添加：

```csharp
// 注册认证服务
services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

// 注册 JWT 令牌服务
services.AddSingleton<IJwtTokenService, JwtTokenService>();

// 注册密码哈希服务
services.AddSingleton<IPasswordHasher, PasswordHasher>();

// 注册用户仓储
services.AddScoped<IUserRepository, UserRepository>();
```

完整文件内容：

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using YuG.Application.Interfaces;
using YuG.Domain.Interfaces;
using YuG.Domain.Repositories;
using YuG.Infrastructure.Persistence;
using YuG.Infrastructure.Repositories;
using YuG.Infrastructure.Services;

namespace YuG.Infrastructure;

/// <summary>
/// 基础设施层依赖注入配置
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 注册基础设施层所有服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合（支持链式调用）</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // 注册数据库上下文
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // 注册数据库上下文接口
        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        // 注册数据库初始化器
        services.AddScoped<ApplicationDbContextInitializer>();

        // 注册认证服务
        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

        // 注册 JWT 令牌服务
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        // 注册密码哈希服务
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // 注册用户仓储
        services.AddScoped<IUserRepository, UserRepository>();

        return services;
    }
}
```

- [ ] **Step 2: 编译验证**

运行: `dotnet build src/YuG.Infrastructure/YuG.Infrastructure.csproj`
预期输出: 编译成功

- [ ] **Step 3: 签入**

```bash
git add src/YuG.Infrastructure/DependencyInjection.cs
git commit -m "feat(infrastructure): 更新依赖注入配置"
```

---

## Task 11: 创建 Application 层 DTO - LoginResponse

**Files:**
- Create: `src/YuG.Application/DTOs/Responses/LoginResponse.cs`

- [ ] **Step 1: 创建登录响应 DTO**

创建 `src/YuG.Application/DTOs/Responses/LoginResponse.cs`：

```csharp
namespace YuG.Application.DTOs.Responses;

/// <summary>
/// 登录响应
/// </summary>
public record LoginResponse
{
    /// <summary>
    /// 访问令牌
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>
    /// 过期时间（UTC）
    /// </summary>
    public DateTime ExpiresAt { get; init; }
}
```

- [ ] **Step 2: 编译验证**

运行: `dotnet build src/YuG.Application/YuG.Application.csproj`
预期输出: 编译成功

- [ ] **Step 3: 签入**

```bash
git add src/YuG.Application/DTOs/Responses/LoginResponse.cs
git commit -m "feat(application): 添加 LoginResponse DTO"
```

---

## Task 12: 创建 Application 层登录命令

**Files:**
- Create: `src/YuG.Application/Commands/Auth/Login/LoginCommand.cs`
- Create: `src/YuG.Application/Commands/Auth/Login/LoginCommandHandler.cs`
- Create: `src/YuG.Application/Commands/Auth/Login/LoginCommandValidator.cs`

- [ ] **Step 1: 创建登录命令**

创建 `src/YuG.Application/Commands/Auth/Login/LoginCommand.cs`：

```csharp
using FluentValidation;
using YuG.Application.Common;
using YuG.Application.DTOs.Responses;

namespace YuG.Application.Commands.Auth.Login;

/// <summary>
/// 登录命令
/// </summary>
public record LoginCommand : CommandBase<LoginResponse>
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// 登录命令验证器
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    /// <summary>
    /// 初始化登录命令验证器
    /// </summary>
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("用户名不能为空")
            .MaximumLength(50).WithMessage("用户名长度不能超过50个字符");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("密码不能为空");
    }
}
```

- [ ] **Step 2: 创建登录命令处理器**

创建 `src/YuG.Application/Commands/Auth/Login/LoginCommandHandler.cs`：

```csharp
using MediatR;
using YuG.Application.DTOs.Responses;
using YuG.Domain.Exceptions;
using YuG.Domain.Interfaces;
using YuG.Domain.ValueObjects;

namespace YuG.Application.Commands.Auth.Login;

/// <summary>
/// 登录命令处理器
/// </summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>
    /// 初始化登录命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="passwordHasher">密码哈希服务</param>
    /// <param name="jwtTokenService">JWT令牌服务</param>
    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// 处理登录命令
    /// </summary>
    /// <param name="request">登录命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录响应</returns>
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 获取用户
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user == null)
        {
            throw new InvalidCredentialsException();
        }

        // 验证密码
        if (!user.VerifyPassword(_passwordHasher, request.Password))
        {
            throw new InvalidCredentialsException();
        }

        // 生成访问令牌
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Username);

        // 生成刷新令牌
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        // 保存刷新令牌
        user.AddRefreshToken(refreshToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 计算过期时间
        var expiresAt = DateTime.UtcNow.AddMinutes(300); // 5小时

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = expiresAt
        };
    }
}
```

- [ ] **Step 3: 编译验证**

运行: `dotnet build src/YuG.Application/YuG.Application.csproj`
预期输出: 编译成功

- [ ] **Step 4: 签入**

```bash
git add src/YuG.Application/Commands/Auth/Login/
git commit -m "feat(application): 添加登录命令和处理器"
```

---

## Task 13: 创建 Application 层刷新令牌命令

**Files:**
- Create: `src/YuG.Application/Commands/Auth/RefreshToken/RefreshTokenCommand.cs`
- Create: `src/YuG.Application/Commands/Auth/RefreshToken/RefreshTokenCommandHandler.cs`
- Create: `src/YuG.Application/Commands/Auth/RefreshToken/RefreshTokenCommandValidator.cs`

- [ ] **Step 1: 创建刷新令牌命令**

创建 `src/YuG.Application/Commands/Auth/RefreshToken/RefreshTokenCommand.cs`：

```csharp
using FluentValidation;
using YuG.Application.Common;
using YuG.Application.DTOs.Responses;

namespace YuG.Application.Commands.Auth.RefreshToken;

/// <summary>
/// 刷新令牌命令
/// </summary>
public record RefreshTokenCommand : CommandBase<LoginResponse>
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// 刷新令牌命令验证器
/// </summary>
public class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    /// <summary>
    /// 初始化刷新令牌命令验证器
    /// </summary>
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("刷新令牌不能为空");
    }
}
```

- [ ] **Step 2: 创建刷新令牌命令处理器**

创建 `src/YuG.Application/Commands/Auth/RefreshToken/RefreshTokenCommandHandler.cs`：

```csharp
using MediatR;
using YuG.Application.DTOs.Responses;
using YuG.Domain.Exceptions;
using YuG.Domain.Interfaces;
using YuG.Domain.ValueObjects;

namespace YuG.Application.Commands.Auth.RefreshToken;

/// <summary>
/// 刷新令牌命令处理器
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>
    /// 初始化刷新令牌命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    /// <param name="jwtTokenService">JWT令牌服务</param>
    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// 处理刷新令牌命令
    /// </summary>
    /// <param name="request">刷新令牌命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录响应</returns>
    public async Task<LoginResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // 获取所有用户并查找包含该刷新令牌的用户
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.RefreshTokens.Any(rt => rt.Token == request.RefreshToken));

        if (user == null)
        {
            throw new InvalidCredentialsException("无效的刷新令牌");
        }

        // 获取并验证刷新令牌
        var refreshToken = user.GetValidRefreshToken(request.RefreshToken);
        if (refreshToken == null)
        {
            throw new InvalidCredentialsException("刷新令牌已过期或已撤销");
        }

        // 撤销旧的刷新令牌
        user.RevokeRefreshToken(request.RefreshToken);

        // 生成新的访问令牌
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Username);

        // 生成新的刷新令牌
        var newRefreshTokenValue = _jwtTokenService.GenerateRefreshToken();
        var newRefreshToken = new RefreshToken
        {
            Token = newRefreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        // 保存新的刷新令牌
        user.AddRefreshToken(newRefreshToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        // 计算过期时间
        var expiresAt = DateTime.UtcNow.AddMinutes(300); // 5小时

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshTokenValue,
            ExpiresAt = expiresAt
        };
    }
}
```

- [ ] **Step 3: 编译验证**

运行: `dotnet build src/YuG.Application/YuG.Application.csproj`
预期输出: 编译成功

- [ ] **Step 4: 签入**

```bash
git add src/YuG.Application/Commands/Auth/RefreshToken/
git commit -m "feat(application): 添加刷新令牌命令和处理器"
```

---

## Task 14: 创建 Application 层登出命令

**Files:**
- Create: `src/YuG.Application/Commands/Auth/Logout/LogoutCommand.cs`
- Create: `src/YuG.Application/Commands/Auth/Logout/LogoutCommandHandler.cs`
- Create: `src/YuG.Application/Commands/Auth/Logout/LogoutCommandValidator.cs`

- [ ] **Step 1: 创建登出命令**

创建 `src/YuG.Application/Commands/Auth/Logout/LogoutCommand.cs`：

```csharp
using FluentValidation;
using YuG.Application.Common;

namespace YuG.Application.Commands.Auth.Logout;

/// <summary>
/// 登出命令
/// </summary>
public record LogoutCommand : CommandBase
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// 登出命令验证器
/// </summary>
public class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    /// <summary>
    /// 初始化登出命令验证器
    /// </summary>
    public LogoutCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("刷新令牌不能为空");
    }
}
```

- [ ] **Step 2: 创建登出命令处理器**

创建 `src/YuG.Application/Commands/Auth/Logout/LogoutCommandHandler.cs`：

```csharp
using MediatR;

namespace YuG.Application.Commands.Auth.Logout;

/// <summary>
/// 登出命令处理器
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly IUserRepository _userRepository;

    /// <summary>
    /// 初始化登出命令处理器
    /// </summary>
    /// <param name="userRepository">用户仓储</param>
    public LogoutCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// 处理登出命令
    /// </summary>
    /// <param name="request">登出命令</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>任务完成</returns>
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        // 获取所有用户并查找包含该刷新令牌的用户
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var user = users.FirstOrDefault(u => u.RefreshTokens.Any(rt => rt.Token == request.RefreshToken));

        if (user != null)
        {
            // 撤销刷新令牌
            user.RevokeRefreshToken(request.RefreshToken);
            await _userRepository.SaveChangesAsync(cancellationToken);
        }

        // 即使找不到用户或令牌，也返回成功（幂等操作）
        await Task.CompletedTask;
    }
}
```

- [ ] **Step 3: 编译验证**

运行: `dotnet build src/YuG.Application/YuG.Application.csproj`
预期输出: 编译成功

- [ ] **Step 4: 签入**

```bash
git add src/YuG.Application/Commands/Auth/Logout/
git commit -m "feat(application): 添加登出命令和处理器"
```

---

## Task 15: 更新 ApplicationDbContextInitializer 添加默认管理员种子数据

**Files:**
- Modify: `src/YuG.Infrastructure/Persistence/ApplicationDbContextInitializer.cs`

- [ ] **Step 1: 更新数据库初始化器**

编辑 `src/YuG.Infrastructure/Persistence/ApplicationDbContextInitializer.cs`：

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YuG.Domain.Entities;
using YuG.Domain.Interfaces;
using YuG.Infrastructure.Services;

namespace YuG.Infrastructure.Persistence;

/// <summary>
/// 数据库初始化器，负责执行迁移和种子数据
/// </summary>
public class ApplicationDbContextInitializer
{
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    /// <summary>
    /// 初始化数据库初始化器
    /// </summary>
    /// <param name="logger">日志记录器</param>
    /// <param name="context">数据库上下文</param>
    /// <param name="passwordHasher">密码哈希服务</param>
    public ApplicationDbContextInitializer(
        ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context,
        IPasswordHasher passwordHasher)
    {
        _logger = logger;
        _context = context;
        _passwordHasher = passwordHasher;
    }

    /// <summary>
    /// 初始化数据库（执行迁移）
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始初始化数据库...");
            await _context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("数据库初始化完成");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据库初始化失败");
            throw;
        }
    }

    /// <summary>
    /// 填充种子数据
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始填充种子数据...");

            // 检查是否已有用户
            if (await _context.Users.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("数据库已包含用户数据，跳过种子数据填充");
                return;
            }

            // 创建默认管理员用户
            var adminPasswordHash = _passwordHasher.Hash("admin123");
            var admin = new User("admin", adminPasswordHash);

            await _context.Users.AddAsync(admin, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("种子数据填充完成：已创建默认管理员账户 (用户名: admin, 密码: admin123)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "种子数据填充失败");
            throw;
        }
    }
}
```

- [ ] **Step 2: 编译验证**

运行: `dotnet build src/YuG.Infrastructure/YuG.Infrastructure.csproj`
预期输出: 编译成功

- [ ] **Step 3: 签入**

```bash
git add src/YuG.Infrastructure/Persistence/ApplicationDbContextInitializer.cs
git commit -m "feat(infrastructure): 添加默认管理员种子数据"
```

---

## Task 16: 更新 Api 层配置

**Files:**
- Modify: `YuG.Api/appsettings.json`
- Modify: `YuG.Api/Program.cs`

- [ ] **Step 1: 更新 appsettings.json 添加 JWT 配置**

编辑 `YuG.Api/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=yug.db"
  },
  "Jwt": {
    "SecretKey": "your-256-bit-secret-key-change-this-in-production",
    "Issuer": "YuG.Api",
    "Audience": "YuG.Client",
    "ExpirationMinutes": 300
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Step 2: 更新 Program.cs 添加 JWT 认证**

编辑 `YuG.Api/Program.cs`，在注册控制器服务之前添加：

```csharp
// 注册 JWT 认证服务
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]!)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// 注册授权服务
builder.Services.AddAuthorization(options =>
{
    // 默认策略：允许所有请求（后续可修改）
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAssertion(_ => true)
        .Build();
});
```

在 Swagger 配置中添加：

```csharp
// 注册 Swagger 服务
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "YuG API",
        Version = "v1",
        Description = "YuG 项目 API 文档",
        Contact = new OpenApiContact
        {
            Name = "YuG"
        }
    });

    // 启用 XML 注释
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    // 添加 JWT 认证支持
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
```

在配置 HTTP 请求管道中添加：

```csharp
app.UseAuthentication();
app.UseAuthorization();
```

需要在文件顶部添加 using：
```csharp
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
```

- [ ] **Step 3: 编译验证**

运行: `dotnet build`
预期输出: 编译成功

- [ ] **Step 4: 签入**

```bash
git add YuG.Api/appsettings.json YuG.Api/Program.cs
git commit -m "feat(api): 添加 JWT 认证配置和 Swagger 支持"
```

---

## Task 17: 更新 AuthController 实现登录/登出端点

**Files:**
- Modify: `YuG.Api/Controllers/AuthController.cs`

- [ ] **Step 1: 更新 AuthController**

编辑 `YuG.Api/Controllers/AuthController.cs`：

```csharp
using MediatR;
using Microsoft.AspNetCore.Mvc;
using YuG.Application.Commands.Auth.Login;
using YuG.Application.Commands.Auth.Logout;
using YuG.Application.Commands.Auth.RefreshToken;

namespace YuG.Api.Controllers;

/// <summary>
/// 认证控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// 初始化认证控制器
    /// </summary>
    /// <param name="mediator">MediatR 发送器</param>
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">登录请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登录结果，包含访问令牌和刷新令牌</returns>
    /// <response code="200">登录成功</response>
    /// <response code="401">用户名或密码错误</response>
    /// <response code="400">请求参数无效</response>
    [HttpPost]
    [Route("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand
        {
            Username = request.Username,
            Password = request.Password
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// 刷新令牌
    /// </summary>
    /// <param name="request">刷新令牌请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>新的访问令牌和刷新令牌</returns>
    /// <response code="200">刷新成功</response>
    /// <response code="401">刷新令牌无效或已过期</response>
    /// <response code="400">请求参数无效</response>
    [HttpPost]
    [Route("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// 用户登出
    /// </summary>
    /// <param name="request">登出请求</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>登出结果</returns>
    /// <response code="204">登出成功</response>
    /// <response code="400">请求参数无效</response>
    [HttpPost]
    [Route("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand
        {
            RefreshToken = request.RefreshToken
        };

        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

/// <summary>
/// 登录请求
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// 刷新令牌请求
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// 登出请求
/// </summary>
public record LogoutRequest
{
    /// <summary>
    /// 刷新令牌
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
}
```

- [ ] **Step 2: 编译验证**

运行: `dotnet build`
预期输出: 编译成功

- [ ] **Step 3: 签入**

```bash
git add YuG.Api/Controllers/AuthController.cs
git commit -m "feat(api): 实现登录/刷新令牌/登出端点"
```

---

## Task 18: 创建数据库迁移

**Files:**
- Run migration command

- [ ] **Step 1: 创建数据库迁移**

运行: `dotnet ef migrations add AddAuthSchema --project src/YuG.Infrastructure/YuG.Infrastructure.csproj --startup-project YuG.Api/YuGApi.csproj`
预期输出: 创建迁移文件

- [ ] **Step 2: 检查生成的迁移文件**

检查 `src/YuG.Infrastructure/Migrations` 目录下是否有新的迁移文件
预期输出: 存在包含 Users 表定义的迁移文件

- [ ] **Step 3: 签入**

```bash
git add src/YuG.Infrastructure/Migrations/
git commit -m "feat(infrastructure): 添加认证相关数据库迁移"
```

---

## Task 19: 编译和运行测试

**Files:**
- Test the complete implementation

- [ ] **Step 1: 完整编译**

运行: `dotnet build`
预期输出: 编译成功，无错误

- [ ] **Step 2: 运行应用程序**

运行: `dotnet run --project YuG.Api`
预期输出: 应用程序启动成功，数据库初始化完成，显示默认管理员账户创建日志

- [ ] **Step 3: 测试登录端点**

使用 Swagger UI 或 curl 测试登录：

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}'
```

预期输出: 返回包含 AccessToken 和 RefreshToken 的 JSON 响应

- [ ] **Step 4: 测试刷新令牌端点**

使用上一步获取的 RefreshToken：

```bash
curl -X POST http://localhost:5000/api/auth/refresh \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"<your-refresh-token>"}'
```

预期输出: 返回新的 AccessToken 和 RefreshToken

- [ ] **Step 5: 测试登出端点**

```bash
curl -X POST http://localhost:5000/api/auth/logout \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"<your-refresh-token>"}'
```

预期输出: 返回 204 NoContent

- [ ] **Step 6: 测试错误场景**

测试错误的用户名密码：

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"wrong","password":"wrong"}'
```

预期输出: 返回 401 Unauthorized

---

## Task 20: 最终验证和清理

**Files:**
- Final verification

- [ ] **Step 1: 验证所有验收标准**

- ✅ 用户可以使用用户名和密码成功登录，获取有效的 JWT Token
- ✅ JWT Token 可以用于认证保护的 API 端点
- ✅ RefreshToken 可以用于获取新的 AccessToken
- ✅ 登出后 RefreshToken 被撤销，无法继续使用
- ✅ 数据库初始化时创建默认管理员账户
- ✅ Swagger UI 支持 "Authorize" 按钮进行测试
- ✅ 所有代码遵循项目代码规范
- ✅ 编译通过无错误

- [ ] **Step 2: 最终代码检查**

检查以下内容：
- 所有 XML 文档注释使用中文
- 所有异步方法包含 CancellationToken 参数
- 所有依赖项通过构造函数注入
- 所有控制器通过 MediatR 分发命令
- 所有 DTO 定义在 Application 层

- [ ] **Step 3: 最终签入**

```bash
git add .
git commit -m "feat: 完成认证授权功能实现"
```

---

## 实现完成标准

1. 所有任务完成
2. 编译通过无错误
3. 登录/刷新/登出端点测试通过
4. 默认管理员账户创建成功
5. Swagger UI 认证功能正常
6. 代码符合项目规范

## 注意事项

1. 生产环境部署前必须更改 JWT SecretKey
2. 建议将敏感配置移至环境变量或密钥管理服务
3. 默认管理员密码应在首次登录后修改
4. 日志记录已包含关键操作（登录、登出、令牌刷新）
