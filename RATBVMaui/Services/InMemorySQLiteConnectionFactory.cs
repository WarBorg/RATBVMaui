namespace RATBVMaui.Services;

public class InMemorySQLiteConnectionFactory : ISQLiteConnectionFactory
{
    public ISQLiteAsyncConnection GetAsyncSqlConnection(ISQLiteService sqliteService) =>
        sqliteService.GetAsyncConnection(":memory:");

    [Obsolete("Please use the GetAsyncSqlConnection method")]
    public ISQLiteConnection GetSqlConnection(ISQLiteService sqliteService) =>
        throw new NotImplementedException();
}
