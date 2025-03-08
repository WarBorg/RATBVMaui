namespace RATBVMaui.Services;

public interface ISQLiteService
{
    [Obsolete]
    ISQLiteConnection GetConnection(string databaseName);
    ISQLiteAsyncConnection GetAsyncConnection(string databaseName);
}