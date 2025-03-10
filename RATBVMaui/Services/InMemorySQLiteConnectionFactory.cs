namespace RATBVMaui.Services;

public class InMemorySQLiteConnectionFactory : ISQLiteConnectionFactory
{
    public ISQLiteAsyncConnection GetAsyncSqlConnection() =>
        new  CustomSQLiteAsyncConnection(":memory:");
}
