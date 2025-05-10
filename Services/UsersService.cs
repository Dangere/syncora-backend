using SyncoraBackend.Data;
using SyncoraBackend.Models.Entities;

namespace SyncoraBackend.Services;

public class UsersService(SyncoraDbContext dbContext)
{
    private readonly SyncoraDbContext _dbContext = dbContext;

    public async Task<UserEntity?> GetUserEntity(int id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    // public async Task<List<UserEntity>> GetAccessableUsers(int userId)
    // {


    // }


}