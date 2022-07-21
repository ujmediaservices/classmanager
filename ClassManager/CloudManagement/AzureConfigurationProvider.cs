using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace gll.ClassManager.CloudManagement
{
    /// <summary>
    /// Uses a managed identity or other available default credential to connect to an Azure Key Vault. 
    /// </summary>
    public class AzureConfigurationProvider : IConfigurationProvider    
    {
        private string keyVaultName; 
        public AzureConfigurationProvider(JObject cfg)
        {
            if (cfg == null)
            {
                throw new ArgumentException("Configuration JSON cannot be null");
            }


        }
    }
}
