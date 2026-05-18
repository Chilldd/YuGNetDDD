using System.Data;

namespace YuG.Application.Common.Interfaces;

/// <summary>
/// SQL 连接工厂接口，用于查询层直接使用 Dapper 进行数据读取
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// 创建并打开一个数据库连接
    /// </summary>
    IDbConnection CreateConnection();
}
