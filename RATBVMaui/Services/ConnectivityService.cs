namespace RATBVMaui.Services;

public class ConnectivityService(IConnectivity connectivity) : IConnectivityService
{
    #region Dependencies

    private readonly IConnectivity _connectivity = connectivity ?? throw new ArgumentNullException(nameof(connectivity));

    #endregion

    #region IConnectivityService Properties

    public bool IsInternetAvailable => _connectivity.NetworkAccess is NetworkAccess.Internet;

    #endregion
}
