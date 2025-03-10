namespace RATBVMaui.Services;

public interface ISQLiteConnectionFactory
{
    public ISQLiteAsyncConnection GetAsyncSqlConnection();
}
