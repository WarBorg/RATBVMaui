namespace RATBVMaui.Services;

public class DefaultHttpServiceOptions : IHttpServiceOptions
{
    #region Constants

    private const string remoteApiBaseAddress = "https://ratbvwebapi.azurewebsites.net/api";

    #endregion

    #region Properties

    public string BaseUrl => remoteApiBaseAddress;

    public TimeSpan Timeout => TimeSpan.FromSeconds(30);

    #endregion
}
