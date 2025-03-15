using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.Models.DTOs.Auth;
using TaskManagementWebAPI.Models.DTOs.Users;
using TaskManagementWebAPI.Models.Entities;

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
    public async Task<LoginResponseDTO?> LoginWithEmailAndPassword(string email, string password)
    {
        // Validate email's and password's formats (TODO)


        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.Trim() == email && u.Hash == password);

        if (user == null)
            return null;


        return new LoginResponseDTO(_tokenService.GenerateAccessToken(user), _mapper.Map<UserDTO>(user));
    }

    public Task<bool> RegisterWithEmailAndPassword(string email, string password)
    {
        return Task.FromResult(true);
    }

    public Task<bool> Logout()
    {
        return Task.FromResult(false);
    }


}