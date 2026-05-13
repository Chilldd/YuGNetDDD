using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using YuG.Application.Permission.Resource.SyncApiEndpoints;
using YuG.Domain.Permission.Enums;

namespace YuG.Api.Helpers;

/// <summary>
/// API 端点扫描器
/// </summary>
public class ApiEndpointScanner : IApiEndpointScanner
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorProvider;

    /// <summary>
    /// 初始化 API 端点扫描器
    /// </summary>
    /// <param name="actionDescriptorProvider">端点描述符提供者</param>
    public ApiEndpointScanner(
        IActionDescriptorCollectionProvider actionDescriptorProvider)
    {
        _actionDescriptorProvider = actionDescriptorProvider;
    }

    /// <inheritdoc />
    public DiscoveredApiScanResult Scan()
    {
        var endpointSet = new HashSet<(string NormalizedPath, ResourceHttpMethod Method)>();
        var endpoints = new List<DiscoveredEndpointInfo>();

        var actions = _actionDescriptorProvider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Where(ad =>
                ad.ControllerTypeInfo.Namespace != null &&
                ad.ControllerTypeInfo.Namespace.StartsWith("YuG.Api", StringComparison.Ordinal) &&
                !ad.MethodInfo.IsDefined(typeof(Microsoft.AspNetCore.Mvc.NonActionAttribute), false) &&
                !IsApiIgnored(ad))
            .ToList();

        foreach (var action in actions)
        {
            var controllerName = action.ControllerName;

            // 获取 HTTP 方法
            var httpMethodNames = action.EndpointMetadata
                .OfType<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>()
                .SelectMany(attr => attr.HttpMethods)
                .ToList();

            if (!httpMethodNames.Any())
            {
                // 如果没有标记 HTTP 方法，默认为 GET
                httpMethodNames.Add("GET");
            }

            // 获取路由模板
            var routeTemplates = GetRouteTemplates(action);

            // 从 ApiDescription 特性读取描述
            var apiDescAttr = action.MethodInfo.GetCustomAttribute<ApiDescriptionAttribute>(false);
            var endpointDescription = apiDescAttr?.Description;

            foreach (var httpMethodName in httpMethodNames)
            {
                if (!TryParseHttpMethod(httpMethodName, out var httpMethod))
                {
                    continue;
                }

                foreach (var routeTemplate in routeTemplates)
                {
                    var fullPath = NormalizePath(routeTemplate);
                    var normalizedPath = fullPath.ToLowerInvariant();

                    if (!endpointSet.Add((normalizedPath, httpMethod)))
                    {
                        continue;
                    }

                    var actionName = action.ActionName;
                    var displayName = $"{controllerName} {actionName}";
                    var generatedCode = GenerateEndpointCode(fullPath, httpMethod);
                    var permissionCode = GeneratePermissionCode(controllerName, actionName);
                    var hasAuthorize =
                        action.MethodInfo.IsDefined(typeof(AuthorizeAttribute), false) ||
                        action.ControllerTypeInfo.IsDefined(typeof(AuthorizeAttribute), false);
                    var hasAllowAnonymous =
                        action.MethodInfo.IsDefined(typeof(AllowAnonymousAttribute), false) ||
                        action.ControllerTypeInfo.IsDefined(typeof(AllowAnonymousAttribute), false);
                    var requirePermission = hasAuthorize && !hasAllowAnonymous;

                    endpoints.Add(new DiscoveredEndpointInfo(
                        fullPath,
                        httpMethod,
                        displayName,
                        generatedCode,
                        permissionCode,
                        requirePermission,
                        endpointDescription ?? string.Empty));
                }
            }
        }

        return new DiscoveredApiScanResult(endpoints);
    }

    /// <summary>
    /// 检查是否标记为忽略 API
    /// </summary>
    private static bool IsApiIgnored(ControllerActionDescriptor action)
    {
        var apiExplorerSettings = action.EndpointMetadata
            .OfType<Microsoft.AspNetCore.Mvc.ApiExplorerSettingsAttribute>()
            .FirstOrDefault();

        return apiExplorerSettings is { IgnoreApi: true };
    }

    /// <summary>
    /// 获取所有路由模板
    /// </summary>
    private static List<string> GetRouteTemplates(ControllerActionDescriptor action)
    {
        var templates = new List<string>();

        // 控制器级别的路由
        var controllerRoute = action.ControllerTypeInfo
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), false)
            .OfType<Microsoft.AspNetCore.Mvc.RouteAttribute>()
            .FirstOrDefault();

        // 动作级别的 HTTP 特性路由
        var actionHttpAttributes = action.MethodInfo
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute), false)
            .OfType<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>()
            .ToList();

        if (actionHttpAttributes.Any())
        {
            foreach (var attr in actionHttpAttributes)
            {
                var actionTemplate = attr.Template;
                var fullTemplate = CombineRoute(controllerRoute?.Template, actionTemplate);
                if (!string.IsNullOrEmpty(fullTemplate))
                {
                    templates.Add(fullTemplate);
                }
            }
        }
        else
        {
            // 动作级别的路由特性
            var actionRoute = action.MethodInfo
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), false)
                .OfType<Microsoft.AspNetCore.Mvc.RouteAttribute>()
                .FirstOrDefault();

            var fullTemplate = CombineRoute(controllerRoute?.Template, actionRoute?.Template);
            if (!string.IsNullOrEmpty(fullTemplate))
            {
                templates.Add(fullTemplate);
            }
        }

        // 如果没有任何路由模板，使用默认路由 convention
        if (!templates.Any())
        {
            var defaultTemplate = $"api/{action.ControllerName}/{action.ActionName}";
            templates.Add(defaultTemplate);
        }

        return templates;
    }

    /// <summary>
    /// 组合控制器路由和动作路由
    /// </summary>
    private static string CombineRoute(string? controllerRoute, string? actionRoute)
    {
        if (string.IsNullOrEmpty(controllerRoute) && string.IsNullOrEmpty(actionRoute))
        {
            return string.Empty;
        }

        if (string.IsNullOrEmpty(controllerRoute))
        {
            return NormalizePath(actionRoute!);
        }

        if (string.IsNullOrEmpty(actionRoute))
        {
            return NormalizePath(controllerRoute);
        }

        // 如果控制器路由以 / 开头，视为绝对路径
        if (controllerRoute.StartsWith('/'))
        {
            return NormalizePath(controllerRoute);
        }

        // 组合相对路径
        return NormalizePath($"{controllerRoute}/{actionRoute}");
    }

    /// <summary>
    /// 规范化路径格式
    /// </summary>
    private static string NormalizePath(string path)
    {
        // 确保以 / 开头
        if (!path.StartsWith('/'))
        {
            path = "/" + path;
        }

        // 移除末尾的 /
        path = path.TrimEnd('/');

        // 替换多个 / 为单个
        while (path.Contains("//"))
        {
            path = path.Replace("//", "/");
        }

        return path;
    }

    /// <summary>
    /// 尝试解析 HTTP 方法枚举
    /// </summary>
    private static bool TryParseHttpMethod(string httpMethodName, out ResourceHttpMethod method)
    {
        var normalized = httpMethodName.ToUpperInvariant();
        method = normalized switch
        {
            "GET" => ResourceHttpMethod.Get,
            "POST" => ResourceHttpMethod.Post,
            "PUT" => ResourceHttpMethod.Put,
            "DELETE" => ResourceHttpMethod.Delete,
            _ => ResourceHttpMethod.Get
        };
        return normalized is "GET" or "POST" or "PUT" or "DELETE";
    }

    /// <summary>
    /// 生成端点资源编码
    /// </summary>
    private static string GenerateEndpointCode(string path, ResourceHttpMethod httpMethod)
    {
        // 将 / 替换为 _，替换 - 为 _
        var code = path
            .Replace('/', '_')
            .Replace("-", "_");

        // 只保留允许的字符：字母、数字、下划线、短横线
        // 移除 { } ( ) : \ ^ $ + 等路由约束相关字符
        var chars = code
            .Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-')
            .ToArray();

        code = new string(chars);

        // 移除开头的 _（因为路径以 / 开头）
        code = code.TrimStart('_');

        // 如果为空，使用默认名称
        if (string.IsNullOrEmpty(code))
        {
            code = "endpoint";
        }

        // 添加 HTTP 方法后缀
        return $"{code}_{httpMethod}".ToLowerInvariant();
    }

    /// <summary>
    /// 生成权限编码（格式：{模块}:{操作}）
    /// </summary>
    /// <param name="controllerName">控制器名称</param>
    /// <param name="actionName">操作方法名称</param>
    /// <returns>权限编码</returns>
    private static string GeneratePermissionCode(string controllerName, string actionName)
    {
        var module = controllerName.ToLowerInvariant();
        var verb = actionName.ToLowerInvariant();
        return $"{module}:{verb}";
    }
}
