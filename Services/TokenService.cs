
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class TokenService(IConfiguration configuration)
{
    private readonly IConfiguration _config = configuration;

    // You should NOT create an access token from a username/password request.
    // Username/password requests aren't authenticated and are vulnerable to impersonation and phishing attacks.
    // Access tokens should only be created using an OpenID Connect flow or an OAuth standard flow.
    // Deviating from these standards can result in an insecure app.

    // We are ignoring this for the sake of simplicity until we implement the OpenID Connect flow or OAuth standard flow.
    public string GenerateAccessToken(UserEntity user)
    {
        var jwtConfig = _config.GetSection("Jwt");
        DateTime expiration = DateTime.UtcNow.AddMinutes(int.Parse(jwtConfig["TokenExpiryMinutes"]!));

        // Create claims. Notice how we include a role claim based on the user's properties.
        // JwtRegisteredClaimNames are standardized JWT claims per the JWT spec
        // Whereas ClaimTypes are Microsoft-defined claims used within ASP.NET Core authentication and identity.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Expiration, expiration.ToString())
        };

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtConfig["SecretKey"]!));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal ExtractPrincipalFromToken(string jwtToken, bool validateLifetime = true)
    {
        var jwtConfig = _config.GetSection("Jwt");

        IdentityModelEventSource.ShowPII = true;

        TokenValidationParameters validationParameters = new TokenValidationParameters();
        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(jwtConfig["SecretKey"]!));


        validationParameters.ValidateLifetime = validateLifetime;

        validationParameters.ValidAudience = jwtConfig["Audience"];
        validationParameters.ValidIssuer = jwtConfig["Issuer"];
        validationParameters.IssuerSigningKey = key;

        ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(jwtToken, validationParameters, out SecurityToken validatedToken);


        return principal;
    }


    public RefreshTokenEntity GenerateRefreshToken(int userId, string salt, out string refreshToken)
    {
        string generatedToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        string refreshTokenHash = Hashing.HashString(generatedToken, salt);
        int expiryDays = int.Parse(_config.GetSection("RefreshToken")["TokenExpiryDays"]!);
        RefreshTokenEntity refreshTokenEntity = RefreshTokenEntity.CreateToken(userId, refreshTokenHash, expiryDays);

        refreshToken = generatedToken;

        return refreshTokenEntity;

    }


    public VerificationTokenEntity GenerateVerificationToken(int userId, out string verificationTokens)
    {
        string generatedToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        string verificationTokenHash = Hashing.HashString(generatedToken, null);
        int expiryMinutes = int.Parse(_config.GetSection("VerificationToken")["TokenExpiryMinutes"]!);
        VerificationTokenEntity verificationTokenEntity = VerificationTokenEntity.CreateToken(userId, verificationTokenHash, expiryMinutes);

        verificationTokens = generatedToken;

        return verificationTokenEntity;

    }


    //     public VerificationTokenEntity GeneratePasswordRestToken(int userId, out string passwordRestToken)
    // {
    //     string generatedToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    //     string passwordRestTokenHash = Hashing.HashString(generatedToken, null);
    //     int expiryMinutes = int.Parse(_config.GetSection("VerificationToken")["TokenExpiryMinutes"]!);
    //     VerificationTokenEntity verificationTokenEntity = VerificationTokenEntity.CreateToken(userId, verificationTokenHash, expiryMinutes);

    //     passwordRestToken = generatedToken;

    //     return passwordRestToken;

    // }

}
