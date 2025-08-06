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


namespace SyncoraBackend.Services;

public class AuthService(IMapper mapper, SyncoraDbContext dbContext, TokenService tokenService)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;
    private readonly TokenService _tokenService = tokenService;

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

        byte[] salt = Convert.FromBase64String(user.Salt);
        string hash = Hashing.HashPassword(password, salt);

        if (!string.Equals(hash, user.Hash))
            return Result<AuthenticationResponseDTO>.Error("Invalid credentials.");

        // Generate refresh token
        RefreshTokenEntity refreshToken = _tokenService.GenerateRefreshToken(user.Id);
        await _dbContext.RefreshTokens.AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Generate access token
        string accessToken = _tokenService.GenerateAccessToken(user);

        AuthenticationResponseDTO authenticationResponse = new(new(AccessToken: accessToken, RefreshToken: refreshToken.RefreshToken), _mapper.Map<UserDTO>(user));
        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }

    public async Task<Result<AuthenticationResponseDTO>> RegisterWithEmailAndPassword(string email, string password, string userName)
    {
        // Validate email's and password's formats
        if (!Validators.ValidateEmail(email))
        {
            return Result<AuthenticationResponseDTO>.Error("Email is not in valid format.");

        }
        if (!Validators.ValidatePassword(password))
        {
            return Result<AuthenticationResponseDTO>.Error("Password is not in valid format.");
        }

        // Validate availability of email and username
        UserEntity? userWithSameEmailOrUserName = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Email, email) || EF.Functions.ILike(u.Username, userName));
        if (userWithSameEmailOrUserName != null)
        {
            if (userWithSameEmailOrUserName.Email == email)
                return Result<AuthenticationResponseDTO>.Error("Email is already in use.");

            if (userWithSameEmailOrUserName.Username == userName)
                return Result<AuthenticationResponseDTO>.Error("Username is already in use.");
        }

        // Generate salt and hash
        byte[] salt = Hashing.GenerateSalt();
        string hash = Hashing.HashPassword(password, salt);


        UserEntity user = new() { Email = email, Hash = hash, Salt = Convert.ToBase64String(salt), CreationDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow, Role = UserRole.User, Username = userName };

        // Save user
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        // Generate refresh token, we assume ef core generates the id for the user after adding it
        RefreshTokenEntity refreshToken = _tokenService.GenerateRefreshToken(user.Id);
        await _dbContext.RefreshTokens.AddAsync(refreshToken);
        await _dbContext.SaveChangesAsync();

        // Generate access token
        string accessToken = _tokenService.GenerateAccessToken(user);

        AuthenticationResponseDTO authenticationResponse = new(new(AccessToken: accessToken, RefreshToken: refreshToken.RefreshToken), _mapper.Map<UserDTO>(user));
        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }
    public async Task<Result<TokensDTO>> RefreshToken(string expiredAccessToken, string refreshToken)
    {
        ClaimsPrincipal claimsFromExpiredToken;
        try
        {
            claimsFromExpiredToken = _tokenService.ExtractPrincipalFromExpiredToken(expiredAccessToken);
        }
        catch (Exception e)
        {
            return Result<TokensDTO>.Error(e.Message);
        }

        int userId = int.Parse(claimsFromExpiredToken.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        RefreshTokenEntity? tokenEntity = await _dbContext.RefreshTokens.AsTracking().Where(t => t.UserId == userId && t.RefreshToken == refreshToken && !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow).FirstOrDefaultAsync();

        if (tokenEntity == null)
            return Result<TokensDTO>.Error("Invalid refresh token.");

        tokenEntity.IsRevoked = true;

        // Generate refresh token
        RefreshTokenEntity newRefreshToken = _tokenService.GenerateRefreshToken(userId);
        await _dbContext.RefreshTokens.AddAsync(newRefreshToken);
        await _dbContext.SaveChangesAsync();


        // Generate access token
        UserEntity user = await _dbContext.Users.FirstAsync(u => u.Id == userId);
        string accessToken = _tokenService.GenerateAccessToken(user);

        return Result<TokensDTO>.Success(new TokensDTO(accessToken, newRefreshToken.RefreshToken));


    }

}