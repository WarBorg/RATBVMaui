using SQLite;

namespace RATBVMaui.Services;

public class DefaultSQLiteConnectionFactory : ISQLiteConnectionFactory
{
    public ISQLiteAsyncConnection GetAsyncSqlConnection()
    {
        const string sqliteFilename = "ratbvPrism.sql";

        return new CustomSQLiteAsyncConnection(sqliteFilename);
    }
}
