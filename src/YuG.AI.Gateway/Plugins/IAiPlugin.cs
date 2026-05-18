namespace YuG.AI.Gateway.Plugins;

/// <summary>AI 插件标记接口。实现此接口的类会被自动注册为 Semantic Kernel 插件。</summary>
public interface IAiPlugin
{
    /// <summary>插件分组名称，用于 Kernel 插件集合中的唯一标识。</summary>
    string Name { get; }
}
