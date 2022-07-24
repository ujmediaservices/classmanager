using gll.ClassManager.CloudManagement;
using gll.ClassManager.Security;

namespace gll.ClassManager.Tests;

public class JWTTokenFixture : IDisposable
{
    public JWTTokenFixture()
    {
        config = JObject.Parse(File.ReadAllText(@".\AzureConfigurationProvider.json"));
        IConfigurationProvider azureCP = SecureConfigurationProvider.Create(config);
        Cert = azureCP.GetX509Certificate("JWTSigningCert").Result;
    }

    public void Dispose()
    {
        Cert = null;
    }

    public X509Certificate2 Cert { get; private set; }
    public JObject config { get; private set; }
}


public class JWTTokenUnitTests
{
    JWTTokenFixture jtf;

    public JWTTokenUnitTests()
    {
        // Set scope properties for logging.
        NLog.LogManager.GetCurrentClassLogger().PushScopeProperty("userid", 0);

        jtf = new JWTTokenFixture();
    }

    // Generate a token and then validate it. 
    [Fact]
    public void TestJwtTokenCycle()
    {
        JwtToken token = new JwtToken(jtf.Cert);
        var tokenStr = token.GenerateJwtToken(0, 60, "User");
        JwtTokenValidationData tokenVal = token.Validate(tokenStr);
        Assert.True(tokenVal.Status == JwtTokenValidationStatus.Valid);
        Assert.True(tokenVal.Payload != null);
        Assert.True((int)tokenVal.Payload["UserId"] == 0);
    }


}