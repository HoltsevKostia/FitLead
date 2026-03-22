using FitLead.Infrastructure.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace FitLead.Api.Identity;

public static class JwtSigningKeyResolver
{
    public static void Validate(JwtOptions options)
    {
        var hasRsaPrivate = !string.IsNullOrWhiteSpace(options.RsaPrivateKeyPem);
        var hasRsaPublic = !string.IsNullOrWhiteSpace(options.RsaPublicKeyPem);

        if (hasRsaPrivate ^ hasRsaPublic)
        {
            throw new InvalidOperationException(
                "Both Jwt:RsaPrivateKeyPem and Jwt:RsaPublicKeyPem must be provided together.");
        }

        if (!(hasRsaPrivate && hasRsaPublic))
        {
            throw new InvalidOperationException(
                "Jwt signing configuration is missing. Provide RSA key pair.");
        }
    }

    public static SecurityKey CreateValidationKey(JwtOptions options)
    {
        Validate(options);
        var rsa = RSA.Create();
        rsa.ImportFromPem(NormalizePem(options.RsaPublicKeyPem!).AsSpan());
        return new RsaSecurityKey(rsa);
    }

    public static SigningCredentials CreateSigningCredentials(JwtOptions options)
    {
        Validate(options);
        var rsa = RSA.Create();
        rsa.ImportFromPem(NormalizePem(options.RsaPrivateKeyPem!).AsSpan());
        return new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
    }

    public static IReadOnlyCollection<string> GetValidAlgorithms(JwtOptions options)
    {
        Validate(options);
        return [SecurityAlgorithms.RsaSha256];
    }

    private static string NormalizePem(string pem)
        => pem.Replace("\\n", "\n", StringComparison.Ordinal);
}
