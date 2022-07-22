namespace gll.ClassManager.Security
{
    /// <summary>
    /// A class for generating and verifying JWT tokens. 
    /// 
    /// Tokens are signed using an X.509 certificate. The certificate should be retrieved from a secure location 
    /// (e.g., Key Vault using AzureConfigurationProvider). The class uses a cert in memory in order to allow for caching of credentials
    /// and fast signing operations without making expensive round trips to Key Vault for signing. 
    /// </summary>
    public class JwtToken
    {

    }
}