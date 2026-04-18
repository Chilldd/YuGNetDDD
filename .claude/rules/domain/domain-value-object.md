---
paths:
  - "YuG.Domain/**/ValueObjects/*.cs"
---

# 值对象（Value Object）开发规范

## 必须事项
- 放在 `YuG.Domain/ValueObjects/` 目录下
- 值对象必须在构造时验证自身有效性
- 无效数据应抛出 `DomainException`
- 不应由外部代码负责验证
- 值对象可包含业务方法，但不修改自身
- 方法返回新的值对象（函数式设计）


## 禁止事项

- **禁止值对象有 Guid Id**，值对象无唯一标识
- **禁止可变属性**（`{get; set;}`），必须用 `init`
- **禁止值对象修改操作返回 void**，必须返回新实例
- **禁止依赖外部注入**（如仓储），值对象完全自包含
- **禁止在值对象中存储实体引用**，只能存ID或值
- **禁止使用 class 定义值对象**，必须 `record`
- **禁止跨聚合共享值对象实例**（复制值，不共享引用）


## ORM 映射

### Owned Type（推荐）

```csharp
// Domain 值对象
public record Address(string Street, string City, string Country);

// EF 配置
modelBuilder.Entity<OrderEntity>()
    .OwnsOne(e => e.ShippingAddress, addressBuilder =>
    {
        addressBuilder.Property(a => a.Street).HasColumnName("shipping_street");
        addressBuilder.Property(a => a.City).HasColumnName("shipping_city");
        addressBuilder.Property(a => a.Country).HasColumnName("shipping_country");
    });
```