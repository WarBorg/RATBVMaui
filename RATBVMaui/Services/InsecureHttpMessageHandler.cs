using System.Net.Security;

namespace RATBVMaui.Services;

public class InsecureHttpMessageHandler : DelegatingHandler, ICustomHttpMessageHandler
{
    #region Constructors

    /// <summary>
    /// Implementation to bypass the SSL problem when debugging with local web api server
    /// </summary>
    public InsecureHttpMessageHandler()
    {
        InnerHandler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (_, cert, _, errors) =>
            {
                if (cert?.Issuer.Equals("CN=localhost") is true)
                {
                    return true;
                }

                return errors is SslPolicyErrors.None;
            }
        };
    }

    #endregion
}
