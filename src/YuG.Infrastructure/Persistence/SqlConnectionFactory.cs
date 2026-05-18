using System.Data;
using Microsoft.Data.Sqlite;
using YuG.Application.Common.Interfaces;
using YuG.Infrastructure.Services;

namespace YuG.Infrastructure.Persistence;

/// <summary>
/// SQL 连接工厂实现，基于多租户连接字符串创建 SQLite 连接
/// </summary>
internal sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _connectionString;

    public SqlConnectionFactory(HttpTenantProvider tenantProvider)
    {
        var tenant = tenantProvider.GetTenant();
        _connectionString = tenant.ConnectionString;
    }

    /// <inheritdoc />
    public IDbConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
