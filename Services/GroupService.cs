using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;
public class GroupService(IMapper mapper, SyncoraDbContext dbContext)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;

    public async Task<Result<GroupDTO>> GetGroup(int userId, int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().Include(g => g.Members).SingleOrDefaultAsync(g => g.Id == groupId);

        if (groupEntity == null)
            return Result<GroupDTO>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        if (groupEntity.OwnerUserId == userId || groupEntity.Members.Any(u => u.Id == userId))
            return new Result<GroupDTO>(_mapper.Map<GroupDTO>(groupEntity));

        return Result<GroupDTO>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);

    }

    // TODO: Add pagination
    // Returns groups owned by the user or shared with the user
    public async Task<Result<GroupDTO[]>> GetGroups(int userId)
    {
        GroupDTO[] groups = await _dbContext.Groups.AsNoTracking().Where(g => g.OwnerUserId == userId || g.Members.Any(u => u.Id == userId)).OrderBy(t => t.CreationDate).ProjectTo<GroupDTO>(_mapper.ConfigurationProvider).ToArrayAsync();

        return new Result<GroupDTO[]>(groups);
    }

    public async Task<Result<GroupDTO>> CreateGroup(CreateGroupDTO createGroupDTO, int userId)
    {
        // Make sure the user exists
        if (await _dbContext.Users.FindAsync(userId) == null)
            return Result<GroupDTO>.Error("User does not exist.", StatusCodes.Status404NotFound);

        GroupEntity createdGroup = new() { Title = createGroupDTO.Title, Description = createGroupDTO.Description, CreationDate = DateTime.UtcNow, OwnerUserId = userId };

        await _dbContext.Groups.AddAsync(createdGroup);
        await _dbContext.SaveChangesAsync();

        return Result<GroupDTO>.Success(_mapper.Map<GroupDTO>(createdGroup));
    }


    public async Task<Result<string>> UpdateGroup(UpdateGroupDTO updateGroupDTO, int userId, int groupId)
    {
        // Make sure the user exists
        if (await _dbContext.Users.FindAsync(userId) == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);

        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.Members).SingleOrDefaultAsync(g => g.Id == groupId);

        // Make sure the group exists
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.Members.Any(u => u.Id == userId);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't update the details of a group they don't own", StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);

        return await UpdateGroupEntity(groupEntity, updateGroupDTO);
    }

    public async Task<Result<string>> DeleteGroup(int userId, int groupId)
    {
        // Make sure the user exists
        if (await _dbContext.Users.FindAsync(userId) == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);

        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
        // Make sure the group exists
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.Members.Any(u => u.Id == userId);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't delete a group they don't own", StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);


        _dbContext.Groups.Remove(groupEntity);
        await _dbContext.SaveChangesAsync();

        if (groupEntity.Tasks.Count == 0)
            return Result<string>.Success("Group deleted.");
        else
            return Result<string>.Success("Group deleted along with all of its tasks.");
    }

    public async Task<Result<string>> AllowAccessToGroup(int groupId, int userId, string userNameToGrant, bool allowAccess)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.Members).SingleOrDefaultAsync(g => g.Id == groupId);

        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        UserEntity? userToGrant = await _dbContext.Users.SingleOrDefaultAsync(u => EF.Functions.ILike(u.Username, userNameToGrant));

        if (userToGrant == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);


        if (allowAccess == groupEntity.Members.Any(u => u.Id == userToGrant.Id))
            return Result<string>.Error($"The user has already been " + (allowAccess ? "granted" : "revoked") + " access.", 400);


        if (groupEntity.OwnerUserId == userToGrant.Id)
            return Result<string>.Error("You can't grant or revoke access to yourself as the group owner.", 400);


        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.Members.Any(u => u.Id == userId);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't " + (allowAccess ? "grant" : "revoke") + " access to a group they don't own", StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);

        if (allowAccess)
            groupEntity.Members.Add(userToGrant);
        else
            groupEntity.Members.Remove(userToGrant);

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