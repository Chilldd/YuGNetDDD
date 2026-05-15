namespace YuG.Application.Common;

/// <summary>
/// 缓存键常量
/// </summary>
public static class CacheKeys
{
    private const string TokenGenPrefix = "token_gen";

    /// <summary>
    /// 用户令牌世代版本缓存键
    /// </summary>
    /// <param name="userId">用户标识</param>
    /// <returns>缓存键</returns>
    public static string TokenGeneration(long userId) => $"{TokenGenPrefix}:{userId}";
}
