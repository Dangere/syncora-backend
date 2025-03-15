using AutoMapper;
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
    public async Task<AuthenticationResponseDTO?> LoginWithEmailAndPassword(string email, string password)
    {
        // Validate email's and password's formats (TODO)
        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            return null;

        byte[] salt = Convert.FromBase64String(user.Salt);
        string hash = Hashing.HashPassword(password, salt);

        if (!string.Equals(hash, user.Hash))
            return null;

        return new AuthenticationResponseDTO(_tokenService.GenerateAccessToken(user), _mapper.Map<UserDTO>(user));
    }

    public async Task<AuthenticationResponseDTO?> RegisterWithEmailAndPassword(string email, string password, string userName)
    {
        // Validate email's and password's formats (TODO)
        // Check for used email (TODO)
        // Check for used username (TODO)

        byte[] salt = Hashing.GenerateSalt();
        string hash = Hashing.HashPassword(password, salt);

        UserEntity user = new() { Email = email, Hash = hash, Salt = Convert.ToBase64String(salt), CreationDate = DateTime.UtcNow, Role = UserRole.User, UserName = userName };

        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();

        return new AuthenticationResponseDTO(_tokenService.GenerateAccessToken(user), _mapper.Map<UserDTO>(user));

    }

}