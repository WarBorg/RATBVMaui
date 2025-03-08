namespace RATBVMaui.Services;

public class DefaultHttpMessageHandler : DelegatingHandler, ICustomHttpMessageHandler
{
    #region Constructors

    public DefaultHttpMessageHandler()
    {
        InnerHandler = new HttpClientHandler();
    }

    #endregion
}