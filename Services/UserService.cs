using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.Models.Entities;

namespace TaskManagementWebAPI.Services;

public class UserService(SyncoraDbContext dbContext)
{
    private readonly SyncoraDbContext _dbContext = dbContext;

    public async Task<UserEntity?> GetUserEntity(int id)
    {
        return await _dbContext.Users.FindAsync(id);
    }


}