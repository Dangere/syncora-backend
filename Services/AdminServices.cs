using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;


public class AdminServices(IMapper mapper, SyncoraDbContext dbContext, TokenService tokenService, EmailService emailService, IConfiguration configuration)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;
    private readonly TokenService _tokenService = tokenService;
    private readonly EmailService _emailService = emailService;
    private readonly IConfiguration _config = configuration;


    public async Task<Result<string>> UpdateUserPassword(string username, string newPassword)
    {

        UserEntity? user = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.Username, username));


        if (user == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);

        // Generate salt and hash
        string salt = Hashing.GenerateSalt();
        string passwordHash = Hashing.HashPassword(newPassword, salt);


        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
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
