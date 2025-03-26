using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using TaskManagementWebAPI.Data;
using TaskManagementWebAPI.Models.DTOs.Groups;
using TaskManagementWebAPI.Models.Entities;
using TaskManagementWebAPI.Utilities;

namespace TaskManagementWebAPI.Services;
public class GroupService(IMapper mapper, SyncoraDbContext dbContext)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;

    public async Task<Result<GroupDTO>> GetGroup(int userId, int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().Include(g => g.SharedUsers).SingleOrDefaultAsync(g => g.Id == groupId);

        if (groupEntity == null)
            return Result<GroupDTO>.Error("Group does not exist.", 404);

        if (groupEntity.OwnerUserId == userId || groupEntity.SharedUsers.Any(u => u.Id == userId))
            return new Result<GroupDTO>(_mapper.Map<GroupDTO>(groupEntity));

        return Result<GroupDTO>.Error("User has no access to this group.", 403);

    }

    public async Task<Result<GroupDTO[]>> GetGroups(int userId)
    {
        GroupDTO[] groups = await _dbContext.Groups.AsNoTracking().Where(g => g.OwnerUserId == userId).OrderBy(t => t.CreationDate).ProjectTo<GroupDTO>(_mapper.ConfigurationProvider).ToArrayAsync();

        return new Result<GroupDTO[]>(groups);
    }

    public async Task<Result<GroupDTO>> CreateGroup(CreateGroupDTO createGroupDTO, int userId)
    {
        // Make sure the user exists
        if (await _dbContext.Users.FindAsync(userId) == null)
            return Result<GroupDTO>.Error("User does not exist.", 404);

        GroupEntity createdGroup = new() { Title = createGroupDTO.Title, Description = createGroupDTO.Description, CreationDate = DateTime.UtcNow, OwnerUserId = userId };

        await _dbContext.Groups.AddAsync(createdGroup);
        await _dbContext.SaveChangesAsync();

        return Result<GroupDTO>.Success(_mapper.Map<GroupDTO>(createdGroup));
    }


    public async Task<Result<string>> UpdateGroup(UpdateGroupDTO updateGroupDTO, int userId, int groupId)
    {
        // Make sure the user exists
        if (await _dbContext.Users.FindAsync(userId) == null)
            return Result<string>.Error("User does not exist.", 404);

        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.SharedUsers).SingleOrDefaultAsync(g => g.Id == groupId);

        // Make sure the group exists
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", 404);

        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.SharedUsers.Any(u => u.Id == userId);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't update the details of a group they don't own", 403);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", 403);

        return await UpdateGroupEntity(groupEntity, updateGroupDTO);
    }

    public async Task<Result<string>> DeleteGroup(int userId, int groupId)
    {
        // Make sure the user exists
        if (await _dbContext.Users.FindAsync(userId) == null)
            return Result<string>.Error("User does not exist.", 404);

        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.SharedUsers).FirstOrDefaultAsync(g => g.Id == groupId);
        // Make sure the group exists
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", 404);

        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.SharedUsers.Any(u => u.Id == userId);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't delete a group they don't own", 403);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", 403);


        _dbContext.Groups.Remove(groupEntity);
        await _dbContext.SaveChangesAsync();

        if (groupEntity.Tasks.Count == 0)
            return Result<string>.Success("Group deleted.");
        else
            return Result<string>.Success("Group deleted along with all of its tasks.");
    }

    public async Task<Result<string>> AllowAccessToGroup(int groupId, int userId, string userNameToGrant, bool allowAccess)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.SharedUsers).SingleOrDefaultAsync(g => g.Id == groupId);

        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", 404);

        UserEntity? userToGrant = await _dbContext.Users.SingleOrDefaultAsync(u => EF.Functions.ILike(u.UserName, userNameToGrant));

        if (userToGrant == null)
            return Result<string>.Error("User does not exist.", 404);


        if (allowAccess == groupEntity.SharedUsers.Any(u => u.Id == userToGrant.Id))
            return Result<string>.Error($"The user has already been " + (allowAccess ? "granted" : "revoked") + " access.", 400);


        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.SharedUsers.Any(u => u.Id == userId);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't " + (allowAccess ? "grant" : "revoke") + " access to a group they don't own", 403);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", 403);

        if (allowAccess)
            groupEntity.SharedUsers.Add(userToGrant);
        else
            groupEntity.SharedUsers.Remove(userToGrant);

        await _dbContext.SaveChangesAsync();

        return Result<string>.Success(allowAccess ? "Access granted." : "Access revoked.");
    }

    private async Task<Result<string>> UpdateGroupEntity(GroupEntity groupEntity, UpdateGroupDTO updateGroupDTO)
    {
        if (updateGroupDTO.Title == groupEntity.Title && updateGroupDTO.Description == groupEntity.Description)
            return Result<string>.Error("Group details are the same.", 400);

        groupEntity.Title = updateGroupDTO.Title ?? groupEntity.Title;
        groupEntity.Description = updateGroupDTO.Description ?? groupEntity.Description;

        await _dbContext.SaveChangesAsync();

        return Result<string>.Success("Group updated.");
    }
}