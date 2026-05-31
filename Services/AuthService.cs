using AutoMapper;
using SyncoraBackend.Utilities;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Auth;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Models.Entities;
using System.Security.Claims;
using Google.Apis.Auth;
using System.Net;


namespace SyncoraBackend.Services;
/// <summary>
///     Auth services used to manage authentication and token generation and verification
/// </summary>
/// <param name="mapper"></param>
/// <param name="dbContext"></param>
/// <param name="tokenService"></param>
/// <param name="emailService"></param>
/// <param name="configuration"></param>
/// <param name="clientSyncService"></param>
/// <param name="logger"></param>
public class AuthService(IMapper mapper, SyncoraDbContext dbContext, TokenService tokenService, EmailService emailService, IConfiguration configuration
, ClientSyncService clientSyncService, ILogger<AuthService> logger)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;
    private readonly TokenService _tokenService = tokenService;
    private readonly EmailService _emailService = emailService;
    private readonly IConfiguration _config = configuration;

    private readonly ClientSyncService _clientSyncService = clientSyncService;
    private readonly ILogger<AuthService> _logger = logger;


    public async Task<Result<AuthenticationResponseDTO>> LoginWithEmailAndPassword(string email, string password)
    {
        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email));

        if (user == null)
            return Result<AuthenticationResponseDTO>.Error("Invalid credentials.", ErrorCodes.INVALID_CREDENTIALS);

        string hash = Hashing.HashPassword(password, user.Salt);

        if (!string.Equals(hash, user.Hash))
            return Result<AuthenticationResponseDTO>.Error("Invalid credentials.", ErrorCodes.INVALID_CREDENTIALS);

        // Generate refresh token
        RefreshTokenEntity refreshTokenEntity = _tokenService.GenerateRefreshToken(user.Id, user.Salt, out string refreshToken);
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        // Generate access token
        string accessToken = _tokenService.GenerateAccessToken(user);

        AuthenticationResponseDTO authenticationResponse = new(new(AccessToken: accessToken, RefreshToken: refreshToken), _mapper.Map<UserDTO>(user), user.IsVerified, _mapper.Map<UserPreferencesDTO>(user.Preferences));
        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }
    /// <summary>
    ///     Registers a user with email and password if the email and username are available
    ///     And sends them a verification email
    /// </summary>
    /// <param name="registerRequest"></param>
    /// <param name="verifyUrl"></param>
    /// <returns></returns>
    public async Task<Result<AuthenticationResponseDTO>> RegisterWithEmailAndPassword(RegisterRequestDTO registerRequest, string verifyUrl)
    {
        // Validate availability of email and username
        UserEntity? userWithSameEmailOrUsername = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, registerRequest.Email) || EF.Functions.ILike(u.Username, registerRequest.Username));
        if (userWithSameEmailOrUsername != null)
        {
            if (userWithSameEmailOrUsername.Email.ToLower() == registerRequest.Email.ToLower())
                return Result<AuthenticationResponseDTO>.Error("Email is already in use.", ErrorCodes.EMAIL_ALREADY_IN_USE, StatusCodes.Status409Conflict);

            if (userWithSameEmailOrUsername.Username.ToLower() == registerRequest.Username.ToLower())
                return Result<AuthenticationResponseDTO>.Error("Username is already in use.", ErrorCodes.USERNAME_ALREADY_IN_USE, StatusCodes.Status409Conflict);

            return Result<AuthenticationResponseDTO>.Error("Credentials already in use.", ErrorCodes.CREDENTIALS_ALREADY_IN_USE);
        }

        // Generate salt and hash
        string salt = Hashing.GenerateSalt();
        string passwordHash = Hashing.HashPassword(registerRequest.Password, salt);

        // Create user without verified email
        UserEntity user = UserEntity.CreateUser(email: registerRequest.Email, username: registerRequest.Username, firstName: registerRequest.FirstName, lastName: registerRequest.LastName, hash: passwordHash, salt: salt, role: UserRoles.User, isVerified: false, userPreferences: registerRequest.UserPreferences);

        // Save user
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Generate refresh token, and store it hashed
        RefreshTokenEntity refreshTokenEntity = _tokenService.GenerateRefreshToken(user.Id, salt, out string refreshToken);
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        // Generate access token
        string accessToken = _tokenService.GenerateAccessToken(user);

        AuthenticationResponseDTO authenticationResponse = new(new(AccessToken: accessToken, RefreshToken: refreshToken), _mapper.Map<UserDTO>(user), user.IsVerified, _mapper.Map<UserPreferencesDTO>(user.Preferences));

        // Send verification email
        _ = await SendVerificationEmail(user.Id, verifyUrl);
        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }
    /// <summary>
    ///     Takes in a google id token and validates it using the google api and returns the user if it exists
    /// </summary>
    /// <param name="idToken"></param>
    /// <returns></returns>
    public async Task<Result<AuthenticationResponseDTO>> LoginWithGoogle(string idToken)
    {
        var jwtValidation = _config.GetSection("GoogleJWTValidation");
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = jwtValidation.GetSection("Audience").Get<List<string>>(),
        };
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Google token validation failed.");
            return Result<AuthenticationResponseDTO>.Error(e.Message, ErrorCodes.INVALID_GOOGLE_TOKEN);
        }

        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, payload.Email));

        if (user == null)
            return Result<AuthenticationResponseDTO>.Error("User does not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        // Generate refresh token
        RefreshTokenEntity refreshTokenEntity = _tokenService.GenerateRefreshToken(user.Id, user.Salt, out string refreshToken);
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        // Generate access token
        string accessToken = _tokenService.GenerateAccessToken(user);

        AuthenticationResponseDTO authenticationResponse = new(new(AccessToken: accessToken, RefreshToken: refreshToken), _mapper.Map<UserDTO>(user), user.IsVerified, _mapper.Map<UserPreferencesDTO>(user.Preferences));
        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }
    /// <summary>
    ///     Takes in a google id token and validates it using the google api
    ///     If the email and username are available creates a new user that is already verified
    /// </summary>
    /// <param name="registerWithGoogleRequest"></param>
    /// <returns></returns>
    public async Task<Result<AuthenticationResponseDTO>> RegisterWithGoogle(RegisterWithGoogleRequestDTO registerWithGoogleRequest)
    {
        var jwtValidation = _config.GetSection("GoogleJWTValidation");
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = jwtValidation.GetSection("Audience").Get<List<string>>(),
        };
        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(registerWithGoogleRequest.IdToken, validationSettings);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Google token validation failed.");

            return Result<AuthenticationResponseDTO>.Error(e.Message, ErrorCodes.INVALID_GOOGLE_TOKEN);
        }

        UserEntity? userWithSameEmailOrUsername = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, payload.Email) || EF.Functions.ILike(u.Username, registerWithGoogleRequest.Username));
        if (userWithSameEmailOrUsername != null)
        {
            if (userWithSameEmailOrUsername.Email.ToLower() == payload.Email.ToLower())
                return Result<AuthenticationResponseDTO>.Error("Email is already in use.", ErrorCodes.EMAIL_ALREADY_IN_USE);

            if (userWithSameEmailOrUsername.Username.ToLower() == registerWithGoogleRequest.Username.ToLower())
                return Result<AuthenticationResponseDTO>.Error("Username is already in use.", ErrorCodes.USERNAME_ALREADY_IN_USE);

            return Result<AuthenticationResponseDTO>.Error("Credentials already in use.", ErrorCodes.CREDENTIALS_ALREADY_IN_USE);
        }

        // Generate salt and hash
        string salt = Hashing.GenerateSalt();
        string hash = Hashing.HashPassword(registerWithGoogleRequest.Password, salt);

        // Create user with verified email
        UserEntity user = UserEntity.CreateUser(email: payload.Email, username: registerWithGoogleRequest.Username, firstName: registerWithGoogleRequest.FirstName, lastName: registerWithGoogleRequest.LastName, hash: hash, salt: salt, role: UserRoles.User, isVerified: true, userPreferences: registerWithGoogleRequest.UserPreferences, profilePictureURL: payload.Picture);

        // Save user
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Generate refresh token, ef core generates the id for the user after adding it
        RefreshTokenEntity refreshTokenEntity = _tokenService.GenerateRefreshToken(user.Id, salt, out string refreshToken);
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        // Generate access token
        string accessToken = _tokenService.GenerateAccessToken(user);

        AuthenticationResponseDTO authenticationResponse = new(new(AccessToken: accessToken, RefreshToken: refreshToken), _mapper.Map<UserDTO>(user), user.IsVerified, _mapper.Map<UserPreferencesDTO>(user.Preferences));

        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }

    /// <summary>
    ///     Takes in a user id and the base url of the website and
    ///     Sends a verification email containing a url with the raw verification token to the user if they're not already verified
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="verifyUrl"></param>
    /// <returns></returns>
    public async Task<Result<string>> SendVerificationEmail(int userId, string verifyUrl)
    {
        UserEntity? user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            return Result<string>.Error("User does not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        if (user.IsVerified)
            return Result<string>.Error("User is already verified.", ErrorCodes.USER_ALREADY_VERIFIED, StatusCodes.Status400BadRequest);

        // Generate verification token and add it to the database
        await _dbContext.VerificationTokens.Where(t => t.UserId == user.Id && !t.IsConsumed).ExecuteUpdateAsync(setters => setters.SetProperty(b => b.IsConsumed, true));
        VerificationTokenEntity verificationTokenEntity = _tokenService.GenerateVerificationToken(user.Id, out string verificationToken);
        await _dbContext.VerificationTokens.AddAsync(verificationTokenEntity);
        await _dbContext.SaveChangesAsync();

        // Encode verification token which will be automatically decoded by the browser
        string webEncodedToken = WebUtility.UrlEncode(verificationToken);

        // Send verification email with the raw verification token
        Result<string> emailResult = await _emailService.SendVerificationEmail(user.Username, user.Email, verifyUrl + $"?token={webEncodedToken}");

        if (!emailResult.IsSuccess)
            return Result<string>.Error(emailResult.ErrorMessage!, ErrorCodes.EMAIL_SEND_FAILED, StatusCodes.Status500InternalServerError);

        return Result<string>.Success("Verification email sent.");
    }

    /// <summary>
    ///     Confirms the verification token and marks the user as verified
    /// </summary>
    /// <param name="verificationToken"></param>
    /// <returns></returns>
    public async Task<Result<string>> ConfirmVerificationEmail(string verificationToken)
    {
        string verificationTokenHash = Hashing.HashToken(verificationToken, null);
        VerificationTokenEntity? tokenEntity = await _dbContext.VerificationTokens.AsTracking().Where(t => t.HashedToken == verificationTokenHash && !t.IsConsumed && t.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();

        if (tokenEntity == null)
            return Result<string>.Error("Invalid verification token. Expired or already consumed.", ErrorCodes.INVALID_TOKEN, StatusCodes.Status400BadRequest);

        UserEntity? user = await _dbContext.Users.FindAsync(tokenEntity.UserId);
        if (user == null)
            return Result<string>.Error("User does not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        if (user.IsVerified)
            return Result<string>.Error("User is already verified.", ErrorCodes.USER_ALREADY_VERIFIED, StatusCodes.Status400BadRequest);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            user.IsVerified = true;
            await _dbContext.SaveChangesAsync();

            await _dbContext.VerificationTokens.Where(t => t.UserId == user.Id && !t.IsConsumed).ExecuteUpdateAsync(setters => setters.SetProperty(b => b.IsConsumed, true));

            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Failed to verify user.");

            return Result<string>.Error("Failed to verify user.", ErrorCodes.INTERNAL_ERROR, StatusCodes.Status500InternalServerError);
        }

        await _clientSyncService.NotifyUserVerification(user.Id);
        return Result<string>.Success("User verified.");
    }

    /// <summary>
    ///     Refreshes the access token using the refresh token
    /// </summary>
    /// <param name="expiredAccessToken"></param>
    /// <param name="refreshToken"></param>
    /// <returns> A result containing the new access token </returns>
    public async Task<Result<TokensDTO>> RefreshToken(string expiredAccessToken, string refreshToken)
    {
        ClaimsPrincipal claimsFromExpiredToken;
        try
        {
            claimsFromExpiredToken = _tokenService.ExtractPrincipalFromToken(expiredAccessToken, false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Token validation failed.");
            return Result<TokensDTO>.Error(e.Message, ErrorCodes.INVALID_TOKEN, StatusCodes.Status401Unauthorized);
        }

        int userId = int.Parse(claimsFromExpiredToken.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        string? salt = await _dbContext.Users.Where(u => u.Id == userId).Select(u => u.Salt).FirstOrDefaultAsync();

        if (salt == null)
            return Result<TokensDTO>.Error("User does not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        // Check if refresh token is valid
        string refreshTokenHash = Hashing.HashToken(refreshToken, salt);

        RefreshTokenEntity? tokenEntity = await _dbContext.RefreshTokens.AsTracking().Where(t => t.UserId == userId && t.HashedToken == refreshTokenHash && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();

        if (tokenEntity == null)
            return Result<TokensDTO>.Error("Invalid refresh token.", ErrorCodes.INVALID_TOKEN, StatusCodes.Status401Unauthorized);

        tokenEntity.IsRevoked = true;

        // Generate refresh token, and store it hashed
        RefreshTokenEntity refreshTokenEntity = _tokenService.GenerateRefreshToken(userId, salt, out string newRefreshToken);
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        // Generate access token
        UserEntity user = await _dbContext.Users.FirstAsync(u => u.Id == userId);
        string accessToken = _tokenService.GenerateAccessToken(user);

        return Result<TokensDTO>.Success(new TokensDTO(accessToken, newRefreshToken));
    }

    public async Task<Result<bool>> CheckUserVerification(int userId)
    {
        bool? verified = await _dbContext.Users.Where(u => u.Id == userId).Select(u => u.IsVerified).FirstOrDefaultAsync();
        if (verified == null)
            return Result<bool>.Error("User does not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        return Result<bool>.Success(verified ?? false);
    }

    /// <summary>
    ///     Takes in a email and the url of the password reset page and
    ///     Sends a password reset email containing the url with the raw password reset token to the user
    /// </summary>
    /// <param name="email"></param>
    /// <param name="passwordResetPageUrl"></param>
    /// <returns></returns>
    public async Task<Result<string>> SendPasswordResetEmail(string email, string passwordResetPageUrl)
    {
        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email));
        if (user == null)
            return Result<string>.Error("User does not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        // Generate password reset token and add it to the database
        await _dbContext.PasswordResetTokens.Where(t => t.UserId == user.Id && !t.IsConsumed).ExecuteUpdateAsync(setters => setters.SetProperty(b => b.IsConsumed, true));
        PasswordResetTokenEntity passwordResetTokenEntity = _tokenService.GeneratePasswordResetToken(user.Id, out string passwordResetToken);
        await _dbContext.PasswordResetTokens.AddAsync(passwordResetTokenEntity);
        await _dbContext.SaveChangesAsync();

        // Encode password reset token which will be automatically decoded by the browser
        string webEncodedToken = WebUtility.UrlEncode(passwordResetToken);

        // Send password reset email with the raw password reset token
        Result<string> emailResult = await _emailService.SendResetPasswordEmail(user.Username, user.Email, passwordResetPageUrl + $"?token={webEncodedToken}");

        if (!emailResult.IsSuccess)
            return Result<string>.Error(emailResult.ErrorMessage!, ErrorCodes.EMAIL_SEND_FAILED, StatusCodes.Status500InternalServerError);

        return Result<string>.Success("Password reset email sent.");
    }
    /// <summary>
    ///     Checks if the password reset token is valid
    /// </summary>
    /// <param name="passwordResetToken"></param>
    /// <returns></returns>
    public async Task<Result<string>> ValidatePasswordResetToken(string passwordResetToken)
    {
        // Get hashed token
        string passwordResetTokenHash = Hashing.HashToken(passwordResetToken, null);

        // Verify token
        PasswordResetTokenEntity? tokenEntity = await _dbContext.PasswordResetTokens
        .AsNoTracking()
        .Where(t => t.HashedToken == passwordResetTokenHash && !t.IsConsumed && t.ExpiresAt > DateTime.UtcNow)
        .FirstOrDefaultAsync();

        if (tokenEntity == null)
            return Result<string>.Error("Invalid password reset token. Expired or already consumed.", ErrorCodes.INVALID_TOKEN, StatusCodes.Status400BadRequest);

        return Result<string>.Success("Password reset token is valid.");
    }
    /// <summary>
    ///     Updates the password of the user using the password reset token
    /// </summary>
    /// <param name="passwordResetToken"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    public async Task<Result<string>> UpdateUserPassword(string passwordResetToken, string newPassword)
    {
        // Get hashed token
        string passwordResetTokenHash = Hashing.HashToken(passwordResetToken, null);

        // Verify token again
        PasswordResetTokenEntity? tokenEntity = await _dbContext.PasswordResetTokens.AsTracking().Where(t => t.HashedToken == passwordResetTokenHash && !t.IsConsumed && t.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();

        if (tokenEntity == null)
            return Result<string>.Error("Invalid password reset token. Expired or already consumed.", ErrorCodes.INVALID_TOKEN, StatusCodes.Status400BadRequest);

        UserEntity? user = await _dbContext.Users.AsTracking().Where(u => u.Id == tokenEntity.UserId).FirstOrDefaultAsync();
        if (user == null)
            return Result<string>.Error("User does not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        // Generate salt and hash
        string salt = Hashing.GenerateSalt();
        string passwordHash = Hashing.HashPassword(newPassword, salt);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Mark token as consumed
            tokenEntity.IsConsumed = true;

            // Revoke refresh tokens to force user to log out from all sessions (they will become invalid anyway because we are updating the salt)
            await _dbContext.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked)
            .ExecuteUpdateAsync(setters => setters.SetProperty(b => b.IsRevoked, true));

            // Update user password and salt
            user.Salt = salt;
            user.Hash = passwordHash;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to update user password.");
            await transaction.RollbackAsync();
            return Result<string>.Error("Failed to update user password.", ErrorCodes.INTERNAL_ERROR, StatusCodes.Status500InternalServerError);
        }

        await _clientSyncService.NotifyPasswordChangedToUser(user.Id);

        return Result<string>.Success("Password updated.");
    }
}