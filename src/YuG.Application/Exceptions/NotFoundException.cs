namespace YuG.Application.Exceptions;

/// <summary>
/// 资源未找到异常
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// 初始化资源未找到异常
    /// </summary>
    /// <param name="entityName">实体名称</param>
    /// <param name="key">实体标识</param>
    public NotFoundException(string entityName, object key)
        : base($"\"{entityName}\" ({key}) 未找到。")
    {
    }
}
