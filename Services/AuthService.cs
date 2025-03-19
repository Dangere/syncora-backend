using AutoMapper;
using TaskManagementWebAPI.Utilities;
using Microsoft.EntityFrameworkCore;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.Enums;
using TaskManagementWebAPI.Models.DTOs.Auth;
using TaskManagementWebAPI.Models.DTOs.Users;
using TaskManagementWebAPI.Models.Entities;
using TaskManagementWebAPI.Utilities;

namespace TaskManagementWebAPI.Services;
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
        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
            return Result<AuthenticationResponseDTO>.Error("Invalid credentials.");

        byte[] salt = Convert.FromBase64String(user.Salt);
        string hash = Hashing.HashPassword(password, salt);

        if (!string.Equals(hash, user.Hash))
            return Result<AuthenticationResponseDTO>.Error("Invalid credentials.");

        AuthenticationResponseDTO authenticationResponse = new(_tokenService.GenerateAccessToken(user), _mapper.Map<UserDTO>(user));
        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }

    public async Task<Result<AuthenticationResponseDTO>> RegisterWithEmailAndPassword(string email, string password, string userName)
    {
        // Validate email's and password's formats
        if (!Validators.ValidateEmail(email))
        {
            return Result<AuthenticationResponseDTO>.Error("Email is not valid format.");

        }
        if (!Validators.ValidatePassword(password))
        {
            return Result<AuthenticationResponseDTO>.Error("Password is not valid format.");
        }

        // Validate availability of email and username
        UserEntity? userWithSameEmailOrUserName = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() || u.UserName.ToLower() == userName.ToLower());
        if (userWithSameEmailOrUserName != null)
        {
            if (userWithSameEmailOrUserName.Email == email)
                return Result<AuthenticationResponseDTO>.Error("Email is already in use.");

            if (userWithSameEmailOrUserName.UserName == userName)
                return Result<AuthenticationResponseDTO>.Error("Username is already in use.");
        }

        // Generate salt and hash
        byte[] salt = Hashing.GenerateSalt();
        string hash = Hashing.HashPassword(password, salt);

        UserEntity user = new() { Email = email, Hash = hash, Salt = Convert.ToBase64String(salt), CreationDate = DateTime.UtcNow, Role = UserRole.User, UserName = userName };

        // Save user
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        AuthenticationResponseDTO authenticationResponse = new(_tokenService.GenerateAccessToken(user), _mapper.Map<UserDTO>(user));
        return Result<AuthenticationResponseDTO>.Success(authenticationResponse);
    }

}