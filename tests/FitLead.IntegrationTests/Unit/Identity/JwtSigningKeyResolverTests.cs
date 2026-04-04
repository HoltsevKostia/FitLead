using FitLead.Api.Identity;
using FitLead.Infrastructure.Identity;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace FitLead.IntegrationTests.Unit.Identity;

public sealed class JwtSigningKeyResolverTests
{
    [Fact]
    public void Validate_WhenOnlyOneRsaKeyProvided_ShouldThrow()
    {
        var options = new JwtOptions
        {
            RsaPrivateKeyPem = "private-only"
        };

        var act = () => JwtSigningKeyResolver.Validate(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*RsaPrivateKeyPem*RsaPublicKeyPem*");
    }

    [Fact]
    public void Validate_WhenNoSigningConfigurationProvided_ShouldThrow()
    {
        var options = new JwtOptions();

        var act = () => JwtSigningKeyResolver.Validate(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Jwt signing configuration is missing*");
    }

    [Fact]
    public void CreateSigningCredentials_WhenRsaPairProvided_ShouldUseRsaSha256()
    {
        using var rsa = RSA.Create(2048);
        var options = new JwtOptions
        {
            RsaPrivateKeyPem = EscapePem(rsa.ExportRSAPrivateKeyPem()),
            RsaPublicKeyPem = EscapePem(rsa.ExportRSAPublicKeyPem())
        };

        var credentials = JwtSigningKeyResolver.CreateSigningCredentials(options);

        credentials.Algorithm.Should().Be(SecurityAlgorithms.RsaSha256);
        credentials.Key.Should().BeOfType<RsaSecurityKey>();
    }

    [Fact]
    public void CreateValidationKey_WhenRsaPairProvided_ShouldReturnRsaSecurityKey()
    {
        using var rsa = RSA.Create(2048);
        var options = new JwtOptions
        {
            RsaPrivateKeyPem = EscapePem(rsa.ExportRSAPrivateKeyPem()),
            RsaPublicKeyPem = EscapePem(rsa.ExportRSAPublicKeyPem())
        };

        var key = JwtSigningKeyResolver.CreateValidationKey(options);

        key.Should().BeOfType<RsaSecurityKey>();
    }

    [Fact]
    public void CreateValidationKey_WhenRsaPairMissing_ShouldThrow()
    {
        var options = new JwtOptions();

        var act = () => JwtSigningKeyResolver.CreateValidationKey(options);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Provide RSA key pair*");
    }

    private static string EscapePem(string pem)
        => pem.Replace("\r\n", "\n", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);
}
