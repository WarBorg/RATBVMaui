namespace RATBVMaui.Services;

public class ConnectivityService() : IConnectivityService
{
    #region IConnectivityService Properties

    public bool IsInternetAvailable => Connectivity.NetworkAccess is NetworkAccess.Internet;

    #endregion
}
