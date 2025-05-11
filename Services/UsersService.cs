using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class UsersService(SyncoraDbContext dbContext)
{
    private readonly SyncoraDbContext _dbContext = dbContext;

    public async Task<UserEntity?> GetUserEntity(int id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task<Result<List<UserEntity>>> GetUsersInGroup(int userId, int groupId, DateTime? since = null)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().Include(g => g.Members).Include(g => g.OwnerUser).SingleOrDefaultAsync(g => g.Id == groupId);

        if (groupEntity == null)
            return Result<List<UserEntity>>.Error("Group does not exist.", StatusCodes.Status404NotFound);


        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.Members.Any(u => u.Id == userId);
        if (!isOwner && !isShared)
            return Result<List<UserEntity>>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);

        HashSet<UserEntity> users = [.. groupEntity.Members, groupEntity.OwnerUser];
        if (since != null)
            return Result<List<UserEntity>>.Success([.. users.Where(u => u.LastModifiedDate > since)]);
        else
            return Result<List<UserEntity>>.Success([.. users]);
    }


}