using System.Reflection;
using System.Xml;
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
    private readonly IHostEnvironment _hostEnvironment;
    private XmlDocument? _xmlDoc;

    /// <summary>
    /// 初始化 API 端点扫描器
    /// </summary>
    /// <param name="actionDescriptorProvider">端点描述符提供者</param>
    /// <param name="hostEnvironment">主机环境</param>
    public ApiEndpointScanner(
        IActionDescriptorCollectionProvider actionDescriptorProvider,
        IHostEnvironment hostEnvironment)
    {
        _actionDescriptorProvider = actionDescriptorProvider;
        _hostEnvironment = hostEnvironment;
    }

    /// <inheritdoc />
    public DiscoveredApiScanResult Scan()
    {
        var controllerSet = new HashSet<string>();
        var controllers = new List<DiscoveredControllerInfo>();
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

        // 提前加载 XML 注释文件
        LoadXmlDocumentation();

        foreach (var action in actions)
        {
            var controllerName = action.ControllerName;
            var controllerType = action.ControllerTypeInfo;

            // 添加控制器（去重）
            if (controllerSet.Add(controllerName))
            {
                var controllerDescription = GetTypeSummary(controllerType);
                var displayName = controllerName;
                var generatedCode = GenerateControllerCode(controllerName);
                controllers.Add(new DiscoveredControllerInfo(controllerName, displayName, generatedCode, controllerDescription));
            }

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

            // 获取动作的总结描述
            var endpointDescription = GetMethodSummary(action.MethodInfo);

            // 如果动作没有注释，尝试获取控制器的注释
            if (string.IsNullOrWhiteSpace(endpointDescription))
            {
                endpointDescription = GetTypeSummary(action.ControllerTypeInfo);
            }

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

                    endpoints.Add(new DiscoveredEndpointInfo(
                        fullPath,
                        httpMethod,
                        displayName,
                        generatedCode,
                        endpointDescription ?? string.Empty,
                        controllerName));
                }
            }
        }

        return new DiscoveredApiScanResult(controllers, endpoints);
    }

    /// <summary>
    /// 加载 XML 文档注释文件
    /// </summary>
    private void LoadXmlDocumentation()
    {
        if (_xmlDoc != null)
        {
            return;
        }

        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyLocation = Path.GetDirectoryName(assembly.Location);
            if (string.IsNullOrEmpty(assemblyLocation))
            {
                return;
            }

            var assemblyName = assembly.GetName().Name;
            var xmlFilePath = Path.Combine(assemblyLocation, $"{assemblyName}.xml");

            if (File.Exists(xmlFilePath))
            {
                _xmlDoc = new XmlDocument();
                _xmlDoc.Load(xmlFilePath);
            }
        }
        catch
        {
            // 加载失败不影响功能，只是没有描述
            _xmlDoc = null;
        }
    }

    /// <summary>
    /// 获取方法的 summary 注释
    /// </summary>
    private string? GetMethodSummary(MethodInfo method)
    {
        if (_xmlDoc == null || method.DeclaringType == null)
        {
            return null;
        }

        var memberName = $"M:{method.DeclaringType.FullName}.{method.Name}";
        var node = _xmlDoc.SelectSingleNode($"//member[@name='{memberName}']/summary");
        if (node?.InnerText == null)
        {
            return null;
        }

        // 清理 XML 中的换行和空白字符
        return CleanXmlSummary(node.InnerText);
    }

    /// <summary>
    /// 获取类型的 summary 注释
    /// </summary>
    private string? GetTypeSummary(Type type)
    {
        if (_xmlDoc == null)
        {
            return null;
        }

        var memberName = $"T:{type.FullName}";
        var node = _xmlDoc.SelectSingleNode($"//member[@name='{memberName}']/summary");
        if (node?.InnerText == null)
        {
            return null;
        }

        // 清理 XML 中的换行和空白字符
        return CleanXmlSummary(node.InnerText);
    }

    /// <summary>
    /// 清理 summary 文本中的空白字符
    /// </summary>
    private static string CleanXmlSummary(string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
        {
            return string.Empty;
        }

        // 替换换行、制表符为单个空格，去除首尾空白
        var cleaned = System.Text.RegularExpressions.Regex
            .Replace(summary, @"\s+", " ")
            .Trim();

        return cleaned;
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
    /// 生成控制器资源编码
    /// </summary>
    private static string GenerateControllerCode(string controllerName)
    {
        // 移除 Controller 后缀
        var code = controllerName.Replace("Controller", string.Empty, StringComparison.Ordinal);
        // 只保留允许字符
        var chars = code
            .Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-')
            .ToArray();
        return new string(chars).ToLowerInvariant();
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
}
