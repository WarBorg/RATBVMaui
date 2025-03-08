namespace RATBVMaui.Services;

public interface IConnectivityService
{
    bool IsInternetAvailable { get; }
}