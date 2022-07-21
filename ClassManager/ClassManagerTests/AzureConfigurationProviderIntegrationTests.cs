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
        string val = scf.GetConfigurationValue("db-username");
        Assert.True(val == "dbd0192");
    }
}