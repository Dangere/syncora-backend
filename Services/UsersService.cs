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

    public async Task<Result<List<UserDTO>>> GetUsersInGroup(int userId, int groupId, DateTime? since = null)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().Include(g => g.Members).Include(g => g.OwnerUser).SingleOrDefaultAsync(g => g.Id == groupId);

        if (groupEntity == null)
            return Result<List<UserDTO>>.Error("Group does not exist.", StatusCodes.Status404NotFound);


        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.Members.Any(u => u.Id == userId);
        if (!isOwner && !isShared)
            return Result<List<UserDTO>>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);

        HashSet<UserEntity> users = [.. groupEntity.Members, groupEntity.OwnerUser];

        List<UserDTO> usersDTO = users.Where(u => since == null || u.LastModifiedDate > since).Select(u => _mapper.Map<UserDTO>(u)).ToList();

        return Result<List<UserDTO>>.Success(usersDTO);
    }


}