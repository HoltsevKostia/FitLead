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
        var hasSymmetric = !string.IsNullOrWhiteSpace(options.SigningKey);

        if (hasRsaPrivate ^ hasRsaPublic)
        {
            throw new InvalidOperationException(
                "Both Jwt:RsaPrivateKeyPem and Jwt:RsaPublicKeyPem must be provided together.");
        }

        if (!hasSymmetric && !(hasRsaPrivate && hasRsaPublic))
        {
            throw new InvalidOperationException(
                "Jwt signing configuration is missing. Provide SigningKey or RSA key pair.");
        }
    }

    public static SecurityKey CreateValidationKey(JwtOptions options)
    {
        if (HasRsaPair(options))
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(NormalizePem(options.RsaPublicKeyPem!).AsSpan());
            return new RsaSecurityKey(rsa);
        }

        return new SymmetricSecurityKey(GetSymmetricKeyBytes(options));
    }

    public static SigningCredentials CreateSigningCredentials(JwtOptions options)
    {
        if (HasRsaPair(options))
        {
            var rsa = RSA.Create();
            rsa.ImportFromPem(NormalizePem(options.RsaPrivateKeyPem!).AsSpan());
            return new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
        }

        return new SigningCredentials(
            new SymmetricSecurityKey(GetSymmetricKeyBytes(options)),
            SecurityAlgorithms.HmacSha256);
    }

    public static IReadOnlyCollection<string> GetValidAlgorithms(JwtOptions options)
    {
        return HasRsaPair(options)
            ? [SecurityAlgorithms.RsaSha256]
            : [SecurityAlgorithms.HmacSha256];
    }

    private static bool HasRsaPair(JwtOptions options)
        => !string.IsNullOrWhiteSpace(options.RsaPrivateKeyPem)
           && !string.IsNullOrWhiteSpace(options.RsaPublicKeyPem);

    private static byte[] GetSymmetricKeyBytes(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.SigningKey))
            throw new InvalidOperationException("Jwt SigningKey is not configured.");

        return System.Text.Encoding.UTF8.GetBytes(options.SigningKey);
    }

    private static string NormalizePem(string pem)
        => pem.Replace("\\n", "\n", StringComparison.Ordinal);
}
