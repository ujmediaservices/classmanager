using Newtonsoft.Json.Linq;

namespace gll.ClassManager.CloudManagement
{

    public interface IConfigurationProvider
    {
        string GetConfigurationValue(string key);
    }
    
    /// <summary>
    /// Provides an abstraction for obtaining configuration values in a secure manner from a cloud provider.
    /// </summary>
    public static class SecureConfigurationProvider
    {
        public static IConfigurationProvider Create(JObject jsonCfg)
        {
            IConfigurationProvider provider = null;

            if (!jsonCfg.ContainsKey("SecureConfigurationProvider")) {
                throw new ArgumentException("Configuration must specify the SecureConfigurationProvider parameter at the top level.");
            }

            var cfgProvider = jsonCfg["SecureConfigurationProvider"];
            if (cfgProvider == null)
            {
                throw new ArgumentException("SecureConfigurationProvider parameter cannot be null.");
            }
            if (cfgProvider?.ToString() == "Azure")
            {
                AzureConfigurationProvider azProv = new AzureConfigurationProvider(jsonCfg);
                provider = azProv; 
            } else
            {
                throw new ArgumentException("SecureCOnfigurationProvider must be one of the following values: Azure");
            }

            return provider; 
        }
    }
}