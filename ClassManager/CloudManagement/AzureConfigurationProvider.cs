using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Security.Cryptography.X509Certificates;
using NLog;

namespace gll.ClassManager.CloudManagement
{
    /// <summary>
    /// Uses a managed identity or other available default credential to connect to an Azure Key Vault. 
    /// </summary>
    public class AzureConfigurationProvider : IConfigurationProvider    
    {
        private string? keyVaultName;
        SecretClient secretClient;

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public AzureConfigurationProvider(JObject cfg)
        {
            Logger.Info("Configuring AzureConfigurationProvider");

            if (cfg == null)
            {
                Logger.Error("Configuration JSON cannot be null");
                throw new ArgumentException("Configuration JSON cannot be null");
            }

            if (!cfg.ContainsKey("KeyVaultName") || cfg["KeyVaultName"] == null || cfg["KeyVaultName"].ToString().Length == 0)
            {
                Logger.Error("Configuration supplied to AzureConfigurationProvider must contain the value KeyVaultName.");
                throw new ArgumentException("Configuration supplied to AzureConfigurationProvider must contain the value KeyVaultName.");
            }

            keyVaultName = (string)cfg["KeyVaultName"];
            secretClient = new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net"), new DefaultAzureCredential());

            Logger.Info("AzureConfigurationProvider successfully configured");
        }

        public async Task<string> GetConfigurationValue(string name)
        {
            Logger.Info("Obtaining secret key of key {0} from key vault {1}", name, secretClient.VaultUri);

            if (name == null || name.Length == 0)
            {
                Logger.Error("Key name for secure Azure config values cannot be null or zero length.");
                throw new ArgumentException("Key name for secure Azure config values cannot be null or zero length.");
            }

            string result = "";
            try {
                var secret = await secretClient.GetSecretAsync(name);
                result = secret.Value.Value;
            } catch (Azure.RequestFailedException ex)
            {
                if (ex.Status == 404)
                {
                    Logger.Error(ex, "Secret with key of {0} could not be found in the Key Vault {1}", name, secretClient.VaultUri);
                    throw new ArgumentException($"Secret with key of {name} could not be found in the Key Vault {secretClient.VaultUri}");
                } else
                {
                    throw (ex);
                }
            }

            Logger.Info("Returning secret for {0}", name);
            return result;
        }

        public async Task<X509Certificate2> GetX509Certificate(string key)
        {
            Logger.Info("Obtaining certificate for {0} from key vault {1}", key, secretClient.VaultUri);

            X509Certificate2 cert = null;

            if (key == null || key.Length == 0)
            {
                throw new ArgumentException("Key name for secure Azure config values cannot be null or zero length.");
            }

            try
            {
                var response = await secretClient.GetSecretAsync(key);
                var keyVaultSecret = response.Value;
                if (keyVaultSecret != null)
                {
                    var privateKeyBytes = Convert.FromBase64String(keyVaultSecret.Value);
                    return new X509Certificate2(privateKeyBytes);
                }
            } catch (Azure.RequestFailedException ex)
            {
                if (ex.Status == 404)
                {
                    throw new ArgumentException($"Certificate with key of {key} could not be found in the Key Vault {secretClient.VaultUri}");
                }
                else
                {
                    throw (ex);
                }
            }

            Logger.Info("Returning certificate for {0}", key);
            return cert;
        }
    }
}
