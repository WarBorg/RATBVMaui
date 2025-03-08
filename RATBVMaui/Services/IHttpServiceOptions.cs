namespace RATBVMaui.Services;

public interface IHttpServiceOptions
{
    string BaseUrl { get; }
    TimeSpan Timeout { get; }
}