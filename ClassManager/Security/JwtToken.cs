using System;
using System.IO;
using JWT;
using System.Security.Cryptography.X509Certificates;
using NLog;
using JWT.Builder;
using JWT.Algorithms;
using JWT.Exceptions;
using Newtonsoft.Json.Linq;

namespace gll.ClassManager.Security
{
    public enum JwtTokenValidationStatus
    {
        Valid,
        NotValid
    }

    public enum JwtTokenInvalidReason
    {
        NotYetValid,
        Expired,
        SignatureVerificationFailed,
        PayloadInvalid
    }

    public class JwtTokenValidationData
    {
        public JwtTokenValidationData(JwtTokenValidationStatus status, JwtTokenInvalidReason invalidReason)
        {
            InvalidReason = invalidReason;
            Status = status;
            Payload = null;
        }

        public JwtTokenValidationData(JwtTokenValidationStatus status, JwtTokenInvalidReason invalidReason, JObject payload)
        {
            InvalidReason = invalidReason;
            Status = status;
            Payload = payload;
        }

        public JwtTokenValidationData(JwtTokenValidationStatus status, JObject payload)
        {
            Status = status;
            Payload = payload;
        }

        public JwtTokenInvalidReason? InvalidReason { get; set; }

        public JwtTokenValidationStatus Status { get; set; }

        public JObject? Payload { get; set; }
    }

    /// <summary>
    /// A class for generating and verifying JWT tokens. 
    /// 
    /// Tokens are signed using an X.509 certificate. The certificate should be retrieved from a secure location 
    /// (e.g., Key Vault using AzureConfigurationProvider). The class uses a cert in memory in order to allow for caching of credentials
    /// and fast signing operations without making expensive round trips to Key Vault for signing. 
    /// </summary>
    public class JwtToken
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private X509Certificate2 cert;

        public JwtToken(X509Certificate2 _cert) {
            cert = _cert;
        }

        public string GenerateJwtToken(int id, int expMinutes, string securityLevel)
        {
            Logger.Info("Generating JWT token: User ID={0}, Expiration={1}, Scope={2}", id, expMinutes, securityLevel);

            var token = JwtBuilder.Create()
                      .WithAlgorithm(new RS256Algorithm(cert))
                      .AddClaim("Expiration", DateTimeOffset.UtcNow.AddMinutes(expMinutes).ToUnixTimeSeconds())
                      .AddClaim("UserId", id)
                      .AddClaim("Scope", securityLevel)
                      .Encode();

            Logger.Info("Successfully generated JWT token.");

            return token;
        }

        public JwtTokenValidationData Validate(string token)
        {
            try
            {
                var json = JwtBuilder.Create()
                     .WithAlgorithm(new RS256Algorithm(cert))
                     .MustVerifySignature()
                     .Decode(token);

                // Test the payload.
                JObject payload = JObject.Parse(json);
                if (!(payload.ContainsKey("UserId") && payload.ContainsKey("Scope"))) 
                {
                    return new JwtTokenValidationData(JwtTokenValidationStatus.NotValid, JwtTokenInvalidReason.PayloadInvalid);
                }

                // Success!
                return new JwtTokenValidationData(JwtTokenValidationStatus.Valid, payload);
            } catch (TokenNotYetValidException timeEx)
            {
                Logger.Error(timeEx, "Token is not yet valid.");
                return new JwtTokenValidationData(JwtTokenValidationStatus.NotValid, JwtTokenInvalidReason.NotYetValid);
            } catch (TokenExpiredException teex)
            {
                Logger.Info(teex, "Token is expired. User must refresh.");
                return new JwtTokenValidationData(JwtTokenValidationStatus.NotValid, JwtTokenInvalidReason.Expired);
            }
            catch (SignatureVerificationException svex)
            {
                Logger.Error(svex, "Token was not signed by the relevant X509 certificate with an issuer of {0}", cert.IssuerName);
                return new JwtTokenValidationData(JwtTokenValidationStatus.NotValid, JwtTokenInvalidReason.SignatureVerificationFailed);
            }
        }
    }
}