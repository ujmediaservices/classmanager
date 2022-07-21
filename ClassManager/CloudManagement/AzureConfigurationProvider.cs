using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace gll.ClassManager.CloudManagement
{
    /// <summary>
    /// Uses a managed identity or other available default credential to connect to an Azure Key Vault. 
    /// </summary>
    public class AzureConfigurationProvider : IConfigurationProvider    
    {
        private string? keyVaultName;
        SecretClient secretClient; 

        public AzureConfigurationProvider(JObject cfg)
        {
            if (cfg == null)
            {
                throw new ArgumentException("Configuration JSON cannot be null");
            }

            if (!cfg.ContainsKey("KeyVaultName") || cfg["KeyVaultName"] == null || cfg["KeyVaultName"].ToString().Length == 0)
            {
                throw new ArgumentException("Configuration supplied to AzureConfigurationProvider must contain the value KeyVaultName.");
            }

            keyVaultName = (string)cfg["KeyVaultName"];
            secretClient = new SecretClient(new Uri($"https://{keyVaultName}.vault.azure.net"), new DefaultAzureCredential());
        }

        public string GetConfigurationValue(string name)
        {
            if (name == null || name.Length == 0)
            {
                throw new ArgumentException("Key name for secure Azure config values cannot be null or zero length.");
            }

            var secret = secretClient.GetSecret(name);

            return secret.Value.Value;
        }
    }
}
