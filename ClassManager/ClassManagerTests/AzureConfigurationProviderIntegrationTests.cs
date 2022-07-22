namespace gll.ClassManager.Tests;

public class JSONConfigurationFixture : IDisposable
{
    public JSONConfigurationFixture()
    {
        config = JObject.Parse(File.ReadAllText(@".\AzureConfigurationProvider.json"));
    }

    public void Dispose()
    {
        config = null;
    }

    public JObject config { get; private set; }
}

/// <summary>
/// Test that Azure config provider (Key Vault) works against the dev env. 
/// </summary>
public class AzureConfigurationProviderIntegrationTests
{
    JSONConfigurationFixture jcf;

    public AzureConfigurationProviderIntegrationTests()
    {
        // Set scope properties for logging.
        NLog.LogManager.GetCurrentClassLogger().PushScopeProperty("userid", 0);

        jcf = new JSONConfigurationFixture();
    }

    [Fact]
    public void TestCreateNullReferenceException()
    {
        Assert.Throws<NullReferenceException>(() => SecureConfigurationProvider.Create(null));
    }

    [Fact]
    public void TestInvalidSecureConfigurationProvider()
    {
        JObject cfgCopy = (JObject)jcf.config.DeepClone();
        cfgCopy["SecureConfigurationProvider"] = "foo";
        Assert.Throws<ArgumentException>(() => SecureConfigurationProvider.Create(cfgCopy));
    }

    [Fact]
    public void TestNoKeyVaultName()
    {
        JObject cfgCopy = (JObject)jcf.config.DeepClone();
        cfgCopy.Remove("KeyVaultName");
        Assert.Throws<ArgumentException>(() => SecureConfigurationProvider.Create(cfgCopy));
    }

    [Fact]
    public void TestNullKeyVaultName()
    {
        JObject cfgCopy = (JObject)jcf.config.DeepClone();
        cfgCopy["KeyVaultName"] = null;
        Assert.Throws<ArgumentException>(() => SecureConfigurationProvider.Create(cfgCopy));
    }

    [Fact]
    public void TestZeroLengthKeyVaultName()
    {
        JObject cfgCopy = (JObject)jcf.config.DeepClone();
        cfgCopy["KeyVaultName"] = "";
        Assert.Throws<ArgumentException>(() => SecureConfigurationProvider.Create(cfgCopy));
    }

    [Fact]
    public void TestValidKeyVaultAndValue()
    {
        IConfigurationProvider scf = SecureConfigurationProvider.Create(jcf.config);
        Assert.NotNull(scf);
        string val = scf.GetConfigurationValue("db-username").Result;
        Assert.True(val == "dbd0192");
    }

    [Fact]
    public void TestNonexistentKeyVaultKey()
    {
        IConfigurationProvider scf = SecureConfigurationProvider.Create(jcf.config);
        Assert.NotNull(scf);
        try
        {
            var val = scf.GetConfigurationValue("ajsljs").Result;
        } catch (System.AggregateException ex)
        {
            Assert.False(ex.InnerException == null); 
            Assert.True(ex.InnerException.GetType() == typeof(System.ArgumentException));
        }
    }

    [Fact]
    public void TestX509CertRetrieval()
    {
        IConfigurationProvider scf = SecureConfigurationProvider.Create(jcf.config);
        Assert.NotNull(scf);
        try
        {
            X509Certificate2 cert = scf.GetX509Certificate("ajsljs").Result;
        } catch (System.AggregateException ex)
        {
            Assert.False(ex.InnerException == null); 
            Assert.True(ex.InnerException.GetType() == typeof(System.ArgumentException));
        }
    }
}