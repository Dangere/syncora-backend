using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class UsersService(SyncoraDbContext dbContext, IMapper mapper)
{
    private readonly SyncoraDbContext _dbContext = dbContext;
    private readonly IMapper _mapper = mapper;

    public async Task<UserEntity?> GetUserEntity(int id)
    {
        return await _dbContext.Users.FindAsync(id);
    }

    public async Task<Result<List<UserDTO>>> GetUsersInGroup(int userId, int groupId, DateTime? sinceUtc = null)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().Include(g => g.GroupMembers).ThenInclude(m => m.User).Include(g => g.OwnerUser).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedDate == null);

        if (groupEntity == null)
            return Result<List<UserDTO>>.Error("Group does not exist.", StatusCodes.Status404NotFound);


        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == userId);
        if (!isOwner && !isShared)
            return Result<List<UserDTO>>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);

        HashSet<UserEntity> users = [.. groupEntity.GroupMembers.Select(m => m.User), groupEntity.OwnerUser];
        List<UserDTO> usersDTO;

        if (sinceUtc == null)
        {
            usersDTO = users.Select(u => _mapper.Map<UserDTO>(u)).ToList();
        }
        else
        {
            usersDTO = users.Where(u => u.LastModifiedDate > sinceUtc).Select(u => _mapper.Map<UserDTO>(u)).ToList();
            // usersDTO = users.Where(u => u.LastModifiedDate > sinceUtc).Select(u => _mapper.Map<UserDTO>(u)).ToList();

        }
        return Result<List<UserDTO>>.Success(usersDTO);
    }

    // Adding functions to update/delete user profile information and make sure to update all groups the user is in 


}