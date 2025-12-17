using AutoMapper;
using SyncoraBackend.Utilities;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Auth;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Models.Entities;
using Microsoft.AspNetCore.Routing.Template;
using System.Security.Claims;
using Google.Apis.Auth;
using SyncoraBackend.Migrations;
using System.Security.Cryptography;
using System.Net;
using SyncoraBackend.Models.Common;


namespace SyncoraBackend.Services;

public class AuthService(IMapper mapper, SyncoraDbContext dbContext, TokenService tokenService, EmailService emailService, IConfiguration configuration
, ClientSyncService clientSyncService)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;
    private readonly TokenService _tokenService = tokenService;
    private readonly EmailService _emailService = emailService;
    private readonly IConfiguration _config = configuration;

    private readonly ClientSyncService _clientSyncService = clientSyncService;


    // You should NOT create an access token from a username/password request.
    // Username/password requests aren't authenticated and are vulnerable to impersonation and phishing attacks.
    // Access tokens should only be created using an OpenID Connect flow or an OAuth standard flow.
    // Deviating from these standards can result in an insecure app.

    // We are ignoring this for the sake of simplicity until we implement the OpenID Connect flow or OAuth standard flow.
    public async Task<Result<AuthenticationResponseDTO>> LoginWithEmailAndPassword(string email, string password)
    {
        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email));

        if (user == null)
            return Result<AuthenticationResponseDTO>.Error("Invalid credentials.");

        string hash = Hashing.HashPassword(password, user.Salt);

        if (!string.Equals(hash, user.Hash))
            return Result<AuthenticationResponseDTO>.Error("Invalid credentials.");

        // Generate refresh token
        RefreshTokenEntity refreshTokenEntity = _tokenService.GenerateRefreshToken(user.Id, user.Salt, out string refreshToken);
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        // Generate access token
        string accessToken = _tokenService.GenerateAccessToken(user);

        AuthenticationResponseDTO authenticationResponse = new(new(AccessToken: accessToken, RefreshToken: refreshToken), _mapper.Map<UserDTO>(user), user.IsVerified, user.Preferences);
        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }

    public async Task<Result<AuthenticationResponseDTO>> RegisterWithEmailAndPassword(RegisterRequestDTO registerRequest, string verifyUrl)
    {
        // Validate email's and password's formats
        if (!Validators.ValidateEmail(registerRequest.Email))
        {
            return Result<AuthenticationResponseDTO>.Error("Email is not in valid format.");

        }
        if (!Validators.ValidatePassword(registerRequest.Password))
        {
            return Result<AuthenticationResponseDTO>.Error("Password is not in valid format.");
        }

        // Validate availability of email and username
        UserEntity? userWithSameEmailOrUserName = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, registerRequest.Email) || EF.Functions.ILike(u.Username, registerRequest.Username));
        if (userWithSameEmailOrUserName != null)
        {
            if (userWithSameEmailOrUserName.Email.ToLower() == registerRequest.Email.ToLower())
                return Result<AuthenticationResponseDTO>.Error("Email is already in use.", StatusCodes.Status409Conflict);

            if (userWithSameEmailOrUserName.Username.ToLower() == registerRequest.Username.ToLower())
                return Result<AuthenticationResponseDTO>.Error("Username is already in use.", StatusCodes.Status409Conflict);
            return Result<AuthenticationResponseDTO>.Error("Credentials already in use.");
        }

        // Generate salt and hash
        string salt = Hashing.GenerateSalt();
        string passwordHash = Hashing.HashPassword(registerRequest.Password, salt);

        // Create user without verified email
        UserEntity user = UserEntity.CreateUser(email: registerRequest.Email, username: registerRequest.Username, hash: passwordHash, salt: salt, role: UserRole.User, isVerified: false, userPreferences: registerRequest.UserPreferences ?? new UserPreferences());


        // Save user
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Generate refresh token, and store it hashed
        RefreshTokenEntity refreshTokenEntity = _tokenService.GenerateRefreshToken(user.Id, salt, out string refreshToken);
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        // Generate access token
        string accessToken = _tokenService.GenerateAccessToken(user);

        AuthenticationResponseDTO authenticationResponse = new(new(AccessToken: accessToken, RefreshToken: refreshToken), _mapper.Map<UserDTO>(user), user.IsVerified, user.Preferences);

        // Send verification email
        _ = await SendVerificationEmail(user.Id, verifyUrl);
        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }


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

            return Result<AuthenticationResponseDTO>.Error(e.Message);
        }


        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, payload.Email));

        // TODO: First identify the user if he exists in the backend and if the account was first created by Google sign in
        // TODO: Make sure to provide a way for a user to merge account if they're the owner if the email

        if (user == null)
            return Result<AuthenticationResponseDTO>.Error("User does not exist.", StatusCodes.Status404NotFound);

        // Generate refresh token
        RefreshTokenEntity refreshTokenEntity = _tokenService.GenerateRefreshToken(user.Id, user.Salt, out string refreshToken);
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();

        // Generate access token
        string accessToken = _tokenService.GenerateAccessToken(user);

        AuthenticationResponseDTO authenticationResponse = new(new(AccessToken: accessToken, RefreshToken: refreshToken), _mapper.Map<UserDTO>(user), user.IsVerified, user.Preferences);
        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }

    public async Task<Result<AuthenticationResponseDTO>> RegisterWithGoogle(RegisterWithGoogleRequestDTO registerWithGoogleRequest, string verifyUrl)
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

            return Result<AuthenticationResponseDTO>.Error(e.Message);
        }

        // TODO: Make sure to provide a way for a user to restore their account if someone already took it or they created it using manual registration
        // Validate availability of email and username
        UserEntity? userWithSameEmailOrUserName = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, payload.Email) || EF.Functions.ILike(u.Username, registerWithGoogleRequest.Username));
        if (userWithSameEmailOrUserName != null)
        {
            if (userWithSameEmailOrUserName.Email.ToLower() == payload.Email.ToLower())
                return Result<AuthenticationResponseDTO>.Error("Email is already in use.");

            if (userWithSameEmailOrUserName.Username.ToLower() == registerWithGoogleRequest.Username.ToLower())
                return Result<AuthenticationResponseDTO>.Error("Username is already in use.");

            return Result<AuthenticationResponseDTO>.Error("Credentials already in use.");
        }


        // Generate salt and hash
        string salt = Hashing.GenerateSalt();
        string hash = Hashing.HashPassword(registerWithGoogleRequest.Password, salt);

        // Create user with verified email
        UserEntity user = UserEntity.CreateUser(email: payload.Email, username: registerWithGoogleRequest.Username, hash: hash, salt: salt, role: UserRole.User, isVerified: true, userPreferences: registerWithGoogleRequest.UserPreferences ?? new UserPreferences());

        // Save user
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Generate refresh token, ef core generates the id for the user after adding it
        RefreshTokenEntity refreshTokenEntity = _tokenService.GenerateRefreshToken(user.Id, salt, out string refreshToken);
        await _dbContext.RefreshTokens.AddAsync(refreshTokenEntity);
        await _dbContext.SaveChangesAsync();


        // Generate access token
        string accessToken = _tokenService.GenerateAccessToken(user);

        AuthenticationResponseDTO authenticationResponse = new(new(AccessToken: accessToken, RefreshToken: refreshToken), _mapper.Map<UserDTO>(user), user.IsVerified, user.Preferences);

        // Send verification email
        _ = await SendVerificationEmail(user.Id, verifyUrl);

        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }

    // Remember to use stricter rate limiting in controllers
    public async Task<Result<string>> SendVerificationEmail(int userId, string verifyUrl)
    {
        UserEntity? user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);

        if (user.IsVerified)
            return Result<string>.Error("User is already verified.", StatusCodes.Status400BadRequest);

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
            return Result<string>.Error(emailResult.ErrorMessage!, StatusCodes.Status500InternalServerError);

        return Result<string>.Success("Verification email sent.");
    }

    public async Task<Result<string>> ConfirmVerificationEmail(string verificationToken)
    {
        string verificationTokenHash = Hashing.HashToken(verificationToken, null);
        VerificationTokenEntity? tokenEntity = await _dbContext.VerificationTokens.AsTracking().Where(t => t.HashedToken == verificationTokenHash && !t.IsConsumed && t.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();

        if (tokenEntity == null)
            return Result<string>.Error("Invalid verification token. Expired or already consumed.", StatusCodes.Status400BadRequest);

        UserEntity? user = await _dbContext.Users.FindAsync(tokenEntity.UserId);
        if (user == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);

        if (user.IsVerified)
            return Result<string>.Error("User is already verified.", StatusCodes.Status400BadRequest);

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            user.IsVerified = true;
            await _dbContext.SaveChangesAsync();

            await _dbContext.VerificationTokens.Where(t => t.UserId == user.Id && !t.IsConsumed).ExecuteUpdateAsync(setters => setters.SetProperty(b => b.IsConsumed, true));


            // Commit transaction if all commands succeed, transaction will auto-rollback
            // when disposed if either commands fails
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Result<string>.Error("Failed to verify user.", StatusCodes.Status500InternalServerError);
        }
        await _clientSyncService.NotifyUserVerification(user.Id);
        return Result<string>.Success("User verified.");
    }
    public async Task<Result<TokensDTO>> RefreshToken(string expiredAccessToken, string refreshToken)
    {
        ClaimsPrincipal claimsFromExpiredToken;
        try
        {
            claimsFromExpiredToken = _tokenService.ExtractPrincipalFromToken(expiredAccessToken, false);
        }
        catch (Exception e)
        {
            return Result<TokensDTO>.Error(e.Message, StatusCodes.Status401Unauthorized);
        }

        int userId = int.Parse(claimsFromExpiredToken.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        string? salt = await _dbContext.Users.Where(u => u.Id == userId).Select(u => u.Salt).FirstOrDefaultAsync();

        if (salt == null)
            return Result<TokensDTO>.Error("User does not exist.", StatusCodes.Status404NotFound);

        // Check if refresh token is valid
        string refreshTokenHash = Hashing.HashToken(refreshToken, salt);

        RefreshTokenEntity? tokenEntity = await _dbContext.RefreshTokens.AsTracking().Where(t => t.UserId == userId && t.HashedToken == refreshTokenHash && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();

        if (tokenEntity == null)
            return Result<TokensDTO>.Error("Invalid refresh token.", StatusCodes.Status401Unauthorized);

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
            return Result<bool>.Error("User does not exist.", StatusCodes.Status404NotFound);

        return Result<bool>.Success(verified ?? false);
    }

    // Remember to use stricter rate limiting in controllers
    public async Task<Result<string>> SendPasswordResetEmail(string email, string passwordResetPageUrl)
    {
        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email));
        if (user == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);


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
            return Result<string>.Error(emailResult.ErrorMessage!, StatusCodes.Status500InternalServerError);

        return Result<string>.Success("Password reset email sent.");
    }

    public async Task<Result<string>> ValidatePasswordResetToken(string passwordResetToken)
    {
        // Get hashed token
        string passwordResetTokenHash = Hashing.HashToken(passwordResetToken, null);

        // Verify token
        PasswordResetTokenEntity? tokenEntity = await _dbContext.PasswordResetTokens.AsNoTracking().Where(t => t.HashedToken == passwordResetTokenHash && !t.IsConsumed && t.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();

        if (tokenEntity == null)
            return Result<string>.Error("Invalid password reset token. Expired or already consumed.", StatusCodes.Status400BadRequest);

        return Result<string>.Success("Password reset token is valid.");
    }


    public async Task<Result<string>> UpdateUserPassword(string passwordResetToken, string newPassword)
    {
        // Get hashed token
        string passwordResetTokenHash = Hashing.HashToken(passwordResetToken, null);

        // Verify token again
        PasswordResetTokenEntity? tokenEntity = await _dbContext.PasswordResetTokens.AsTracking().Where(t => t.HashedToken == passwordResetTokenHash && !t.IsConsumed && t.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();

        if (tokenEntity == null)
            return Result<string>.Error("Invalid password reset token. Expired or already consumed.", StatusCodes.Status400BadRequest);
        UserEntity? user = await _dbContext.Users.AsTracking().Where(u => u.Id == tokenEntity.UserId).FirstOrDefaultAsync();
        if (user == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);

        // Generate salt and hash
        string salt = Hashing.GenerateSalt();
        string passwordHash = Hashing.HashPassword(newPassword, salt);


        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Mark token as consumed
            tokenEntity.IsConsumed = true;

            // Revoke refresh tokens to force user to log out from all sessions (they will become invalid anyway because we are updating the salt)
            await _dbContext.RefreshTokens.Where(t => t.UserId == user.Id && !t.IsRevoked).ExecuteUpdateAsync(setters => setters.SetProperty(b => b.IsRevoked, true));

            // Update user password and salt
            user.Salt = salt;
            user.Hash = passwordHash;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return Result<string>.Error("Failed to update user password.", StatusCodes.Status500InternalServerError);
        }

        return Result<string>.Success("Password updated.");
    }

}