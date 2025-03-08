namespace RATBVMaui.Services;

public class HttpService(
    ICustomHttpMessageHandler customHttpMessageHandler,
    IHttpServiceOptions httpServiceOptions)
    : IHttpService
{
    #region Properties

    public HttpClient HttpClient { get; } = new((HttpMessageHandler)customHttpMessageHandler)
    {
        BaseAddress = new Uri(httpServiceOptions.BaseUrl),
        Timeout = httpServiceOptions.Timeout
    };

    #endregion
}
