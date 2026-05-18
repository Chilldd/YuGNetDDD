using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using YuG.Common.Models;

namespace YuG.Api.Filters;

/// <summary>
/// 统一 API 响应过滤器，将控制器返回值包装为 <see cref="ApiResponse"/>
/// </summary>
public class ApiResponseFilter : IActionFilter
{
    /// <summary>
    /// 请求执行前不做处理
    /// </summary>
    public void OnActionExecuting(ActionExecutingContext context)
    {
    }

    /// <summary>
    /// 请求执行后将 <see cref="ObjectResult"/> 或 <see cref="CreatedAtActionResult"/> 的返回值包装为 <see cref="ApiResponse"/>
    /// </summary>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // 跳过标记了 [IgnoreApiResponse] 的接口
        if (context.Controller is not ControllerBase controller)
            return;

        var hasIgnore = controller.ControllerContext.ActionDescriptor
            .EndpointMetadata
            .Any(em => em is IgnoreApiResponseAttribute);

        if (hasIgnore)
            return;

        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is ApiResponse)
                return;

            var statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;

            if (statusCode >= 400)
            {
                // 错误响应
                var message = objectResult.Value is string msg
                    ? msg
                    : "请求失败";

                objectResult.Value = ApiResponse.Fail(statusCode * 100, message);
            }
            else
            {
                // 成功响应
                objectResult.Value = ApiResponse.Ok(objectResult.Value);
            }
        }
        else if (context.Result is CreatedAtActionResult createdResult)
        {
            if (createdResult.Value is ApiResponse)
                return;

            createdResult.Value = ApiResponse.Ok(createdResult.Value);
        }
    }
}
