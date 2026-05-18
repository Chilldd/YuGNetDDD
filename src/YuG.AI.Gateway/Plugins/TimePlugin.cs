using System.ComponentModel;
using Microsoft.SemanticKernel;

namespace YuG.AI.Gateway.Plugins;

/// <summary>时间日期工具插件。提供获取当前服务器时间、日期等函数供 LLM 调用。</summary>
public class TimePlugin : IAiPlugin
{
    /// <inheritdoc />
    public string Name => "time";

    /// <summary>获取当前服务器日期和时间。</summary>
    /// <returns>格式为 yyyy-MM-dd HH:mm:ss 的日期时间字符串</returns>
    [KernelFunction("get_current_datetime")]
    [Description("获取当前服务器日期和时间")]
    public string GetCurrentDateTime() => DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    /// <summary>获取当前日期（不含时间）。</summary>
    /// <returns>格式为 yyyy-MM-dd 的日期字符串</returns>
    [KernelFunction("get_current_date")]
    [Description("获取当前日期（不含时间）")]
    public string GetCurrentDate() => DateTime.Now.ToString("yyyy-MM-dd");

    /// <summary>获取当前时间（不含日期）。</summary>
    /// <returns>格式为 HH:mm:ss 的时间字符串</returns>
    [KernelFunction("get_current_time")]
    [Description("获取当前时间（不含日期）")]
    public string GetCurrentTime() => DateTime.Now.ToString("HH:mm:ss");
}
