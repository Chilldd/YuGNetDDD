---
paths:
  - "**/*.cs"
---

# C# 代码开发规范

## 必须事项

### 命名规范
- 命名空间使用 PascalCase，格式为 `YuG.{层名}.{子模块}`
- 类名使用 PascalCase，名词形式
- 接口名使用 PascalCase，以 `I` 前缀开头
- 方法名使用 PascalCase，动词或动词短语开头
- 属性名使用 PascalCase
- 私有字段使用 _camelCase，以 `_` 前缀开头
- 局部变量使用 camelCase
- 常量使用 PascalCase
- 异步方法以 `Async` 后缀结尾
- 异常类以 `Exception` 后缀结尾
- 特性/Attributes 以 `Attribute` 后缀结尾

### 代码格式
- 使用文件作用域命名空间声明（不使用 namespace 嵌套花括号）
- 使用大括号初始化器语法进行集合初始化（`[]`）
- 使用模式匹配和 switch 表达式替代传统的 if-else 和 switch 语句
- 使用 null 合并运算符（`??`）和 null 条件运算符（`?.`）
- 使用字符串插值（`$"{variable}"`）替代字符串拼接
- 使用 `var` 进行类型推断，当类型显而易见时
- 每行代码最大长度建议 120 字符
- 每个方法建议不超过 50 行，超出则拆分

### 异步编程
- 所有 I/O 操作（数据库、文件、网络）必须使用异步方法（`*Async`）
- 异步方法返回类型必须是 `Task`、`Task<T>` 或 `ValueTask<T>`

### 依赖注入
- 依赖项通过构造函数注入，使用 `private readonly` 字段存储
- 构造函数参数必须使用 XML 文档注释标注
- 优先使用 `IEnumerable<T>` 注入多个实现（如验证器、管道行为）

### 返回类型
- 查询方法返回集合时，优先使用 `IReadOnlyList<T>` 或 `IReadOnlyCollection<T>` 而非 `List<T>` 或 `Array`
- 返回可能为 null 的引用类型时，使用 `T?` 标注可空性

### 异常处理
- 捕获异常后必须记录日志，使用适当的日志级别（`LogError`、`LogWarning`）
- 禁止在 Handler 中捕获 `DomainException`，由全局异常中间件统一处理

### 注释规范
- 所有 `public` 和 `protected` 类型、方法、属性必须有 XML 文档注释（`/// <summary>`）
- XML 注释必须使用中文描述
- 方法参数使用 `<param name="...">` 标注
- 返回值使用 `<returns>` 标注
- 异常使用 `<exception cref="...">` 标注

### 现代 C# 特性（.NET 10）
- 使用模式匹配（`is not null`、`is` 模式、属性模式）
- 使用集合表达式（`int[] numbers = [1, 2, 3];`）
- 使用 nullable 引用类型（启用 `Nullable`），显式标注可空性（`T?`）

### 可变/不可变类型设计规范

#### 不可变类型（使用 record）
- 值对象、DTO、响应模型、领域事件使用 `record` 定义
- 属性使用 `init` 关键字或位置式 record
- 使用 `required` 属性标记必需的构造参数
- 修改操作返回新实例而非修改当前实例

#### 可变类型（使用 class）
- ORM 实体、需要追踪变化的实体使用 `class` 定义
- 属性使用 `{ get; set; }` 支持修改
- 禁止在 ORM 实体中使用 `record` 或 `init-only` 属性

### 代码组织
- 每个文件只包含一个公开类型，私有嵌套类型除外
- 类型成员按以下顺序排列：常量和静态字段 → 字段 → 构造函数 → 公共属性 → 公共方法 → 私有方法 → 嵌套类型
- 同一类型的成员按访问修饰符分组：`public` → `protected` → `internal` → `private`

### HTTP 上下文访问规范
- 禁止在 Command、Query、Handler、Domain 层中访问 `HttpContext`
- 表现层关注点不应泄露到应用层、领域层
- 用户信息通过 Claim 传递给 Command/Query，在 Handler 中处理

### LINQ 规范
- 优先使用方法语法（链式调用）而非查询语法
- 使用 `Where`、`Select` 等 LINQ 方法时，优先使用 lambda 表达式而非匿名方法
- 使用 `.Any()` 检查是否存在元素，而非 `.Count() > 0`
- 使用 `.FirstOrDefault()` 获取单个元素，处理 null 情况

### 字符串处理
- 字符串比较使用 `string.Equals(a, b, StringComparison.OrdinalIgnoreCase)` 而非 `==` 或 `.ToLower()`
- 复杂字符串拼接使用 `StringBuilder`，而非 `+` 或 `string.Format`
- 路径拼接使用 `Path.Combine()`，确保跨平台兼容性

## 禁止事项

- 禁止使用 `async void`（除事件处理器外）
- 禁止使用 `.Result` 或 `.Wait()` 阻塞异步方法
- 禁止在 using 语句中使用异步方法而不用 `await`
- 禁止吞掉异常（空 catch 块），至少要记录日志
- 禁止在构造函数中执行耗时操作或 I/O 操作
- 禁止使用静态可变状态（静态可变字段），这会导致线程安全问题
- 禁止使用 `Task.Run` 包装已有的异步方法
- 禁止在 finally 块中抛出异常
- 禁止使用魔数，应定义常量或配置项
- 禁止在业务逻辑中硬编码字符串，应使用常量或资源文件