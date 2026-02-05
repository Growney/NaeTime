using EventDb.Sqlite.Abstractions;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDb.Sqlite.Connections;
public class SqliteConnectionFactory : ISqliteConnectionFactory
{
    private const string _datasource = "eventdblite.db";
    public SqliteConnection CreateConnection(SqliteOpenMode mode)
    {
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = _datasource,
            Cache = SqliteCacheMode.Private,
            Pooling = false,
            Mode = mode
        };

        string connectionString = builder.ToString();

        return new SqliteConnection(connectionString);
    }
}
