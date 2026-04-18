---
paths:
  - "YuG.Application/**/*Validator.cs"
---

# Validator（验证器）开发规范

## 必须事项

- 验证器必须继承 `AbstractValidator<TRequest>`
- 验证器必须以 `Validator` 后缀结尾
- 验证器与对应的 Command/Query 放在同一目录下
- 使用 FluentValidation 的规则语法定义验证规则
- 验证规则应包括：非空验证、长度限制、格式验证、业务规则前置验证
- 验证器自动由 `ValidationBehavior` 在管道中调用
- 验证错误消息必须使用中文

## 禁止事项

- 禁止在验证器中访问数据库（性能问题），业务规则验证应在 Handler 或 Domain 层完成
- 禁止在验证器中修改请求对象
- 禁止在验证器中执行耗时操作
- 禁止在验证器中抛出异常，应返回验证失败结果
- 禁止使用英文错误消息
- 禁止在验证器中进行异步数据库查询验证
