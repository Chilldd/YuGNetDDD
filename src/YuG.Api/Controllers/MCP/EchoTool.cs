using System.ComponentModel;
using ModelContextProtocol.Server;

namespace YuG.Api.Controllers.MCP;

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool, Description("输出信息")]
    public static string Echo(string message) => $"hello {message}";
    
    
    [McpServerTool, Description("加法计算")]
    public static int AddCalc(int a, int b) => a + b;
}