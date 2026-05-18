namespace YuG.Api.Filters;

/// <summary>
/// 标记此特性后，接口响应将不会被 <see cref="ApiResponseFilter"/> 自动包装
/// </summary>
/// <remarks>用于健康检查、SSE 流式响应等不需要统一包装的接口</remarks>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class IgnoreApiResponseAttribute : Attribute;
