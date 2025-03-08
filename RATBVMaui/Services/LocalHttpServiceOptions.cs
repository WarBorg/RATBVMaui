namespace RATBVMaui.Services;

public class LocalHttpServiceOptions : IHttpServiceOptions
{
    #region Constants

    private const string iosLocalBaseAddress = "https://localhost:5001/api";
    private const string androidLocalBaseAddress = "https://10.0.2.2:5001/api";

    #endregion

    #region Properties

    public string BaseUrl =>
        DeviceInfo.Platform == DevicePlatform.Android
            ? androidLocalBaseAddress
            : iosLocalBaseAddress;

    public TimeSpan Timeout => TimeSpan.FromSeconds(5);

    #endregion
}
