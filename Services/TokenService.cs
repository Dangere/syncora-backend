
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;
/// <summary>
///     Token service used to generate tokens for authentication and verification
/// </summary>
/// <param name="configuration"></param>
public class TokenService(IConfiguration configuration)
{
    private readonly IConfiguration _config = configuration;

    /// <summary>
    ///     Generates a JWT access token for the user holding the claims below
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public string GenerateAccessToken(UserEntity user)
    {
        var jwtConfig = _config.GetSection("Jwt");
        DateTime expiration = DateTime.UtcNow.AddMinutes(int.Parse(jwtConfig["TokenExpiryMinutes"]!));

        // JwtRegisteredClaimNames are standardized JWT claims per the JWT spec
        // Whereas ClaimTypes are Microsoft-defined claims used within ASP.NET Core authentication and identity.

        // Creating claims. we include a role claim based on the user's properties.
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

    /// <summary>
    ///     Extracts the principal from a jwt token (even if it's expired) and returns it
    ///     Used for refreshing tokens
    /// </summary>
    /// <param name="jwtToken"></param>
    /// <param name="validateLifetime"></param>
    /// <returns></returns>
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

    /// <summary>
    ///     Generates a hashed salted refresh token
    ///     Returns an entity to be stored in the database and the raw generated token
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="salt"></param>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    public RefreshTokenEntity GenerateRefreshToken(int userId, string salt, out string refreshToken)
    {
        string generatedToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        string refreshTokenHash = Hashing.HashToken(generatedToken, salt);
        int expiryDays = int.Parse(_config.GetSection("RefreshToken")["TokenExpiryDays"]!);
        RefreshTokenEntity refreshTokenEntity = RefreshTokenEntity.CreateToken(userId, refreshTokenHash, expiryDays);

        refreshToken = generatedToken;

        return refreshTokenEntity;

    }

    /// <summary>
    ///     Generates a hashed unsalted verification token
    ///     Returns an entity to be stored in the database and the raw generated token
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="verificationTokens"></param>
    /// <returns></returns>
    public VerificationTokenEntity GenerateVerificationToken(int userId, out string verificationTokens)
    {
        string generatedToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        string verificationTokenHash = Hashing.HashToken(generatedToken, null);
        int expiryMinutes = int.Parse(_config.GetSection("VerificationToken")["TokenExpiryMinutes"]!);
        VerificationTokenEntity verificationTokenEntity = VerificationTokenEntity.CreateToken(userId, verificationTokenHash, expiryMinutes);

        verificationTokens = generatedToken;

        return verificationTokenEntity;

    }

    /// <summary>
    ///     Generates a hashed unsalted password reset token
    ///     Returns an entity to be stored in the database and the raw generated token 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="passwordResetToken"></param>
    /// <returns></returns>
    public PasswordResetTokenEntity GeneratePasswordResetToken(int userId, out string passwordResetToken)
    {
        string generatedToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        string passwordResetTokenHash = Hashing.HashToken(generatedToken, null);
        int expiryMinutes = int.Parse(_config.GetSection("PasswordResetToken")["TokenExpiryMinutes"]!);
        PasswordResetTokenEntity passwordResetTokenEntity = PasswordResetTokenEntity.CreateToken(userId, passwordResetTokenHash, expiryMinutes);

        passwordResetToken = generatedToken;

        return passwordResetTokenEntity;

    }

}
