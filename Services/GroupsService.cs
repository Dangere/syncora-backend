using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Hubs;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class GroupsService(IMapper mapper, SyncoraDbContext dbContext, ClientSyncService clientSyncService)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;

    private readonly ClientSyncService _clientSyncService = clientSyncService;

    public async Task<Result<GroupDTO>> GetGroup(int userId, int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().Include(g => g.GroupMembers).ThenInclude(m => m.User).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedDate == null);

        if (groupEntity == null)
            return Result<GroupDTO>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        if (groupEntity.OwnerUserId == userId || groupEntity.GroupMembers.Any(m => m.UserId == userId))
            return new Result<GroupDTO>(_mapper.Map<GroupDTO>(groupEntity));

        return Result<GroupDTO>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);

    }

    // TODO: Add pagination
    // Returns groups owned by the user or shared with the user
    public async Task<List<GroupDTO>> GetGroups(int userId, DateTime? sinceUtc = null)
    {
        List<GroupDTO> groups;
        if (sinceUtc != null)
        {
            groups = await _dbContext.Groups.AsNoTracking().Where(g => (g.OwnerUserId == userId || g.GroupMembers.Any(m => m.UserId == userId)) && g.LastModifiedDate > sinceUtc && g.DeletedDate == null).OrderBy(t => t.CreationDate).ProjectTo<GroupDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }
        else
        {
            groups = await _dbContext.Groups.AsNoTracking().Where(g => (g.OwnerUserId == userId || g.GroupMembers.Any(m => m.UserId == userId)) && g.DeletedDate == null).OrderBy(t => t.CreationDate).ProjectTo<GroupDTO>(_mapper.ConfigurationProvider).ToListAsync();

        }

        return groups;
    }

    public async Task<Result<GroupDTO>> CreateGroup(CreateGroupDTO createGroupDTO, int userId)
    {
        // Make sure the user exists
        if (await _dbContext.Users.FindAsync(userId) == null)
            return Result<GroupDTO>.Error("User does not exist.", StatusCodes.Status404NotFound);

        GroupEntity createdGroup = new() { Title = createGroupDTO.Title, Description = createGroupDTO.Description, CreationDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow, OwnerUserId = userId };

        await _dbContext.Groups.AddAsync(createdGroup);
        await _dbContext.SaveChangesAsync();

        return Result<GroupDTO>.Success(_mapper.Map<GroupDTO>(createdGroup));
    }


    public async Task<Result<string>> UpdateGroup(UpdateGroupDTO updateGroupDTO, int userId, int groupId)
    {
        // Make sure the user exists
        if (await _dbContext.Users.FindAsync(userId) == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);

        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers).ThenInclude(m => m.User).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedDate == null);

        // Make sure the group exists
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == userId);
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

        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers).ThenInclude(m => m.User).FirstOrDefaultAsync(g => g.Id == groupId && g.DeletedDate == null);
        // Make sure the group exists
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == userId);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't delete a group they don't own", StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);


        groupEntity.DeletedDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        await _clientSyncService.NotifyGroupMembersToSync(groupEntity.Id);


        // TODO: Store deleted groups to return in the response for syncing with client
        if (groupEntity.Tasks.Count == 0)
            return Result<string>.Success("Group deleted.");
        else
            return Result<string>.Success("Group deleted along with all of its tasks.");
    }

    public async Task<Result<string>> AllowAccessToGroup(int groupId, int userId, string userNameToGrant, bool allowAccess)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers).ThenInclude(m => m.User).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedDate == null);

        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == userId);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't " + (allowAccess ? "grant" : "revoke") + " access to a group they don't own", StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);



        UserEntity? userToGrant = await _dbContext.Users.SingleOrDefaultAsync(u => EF.Functions.ILike(u.Username, userNameToGrant));

        if (userToGrant == null)
            return Result<string>.Error("User does not exist.", StatusCodes.Status404NotFound);

        if (groupEntity.OwnerUserId == userToGrant.Id)
            return Result<string>.Error("You can't grant or revoke access to yourself as the group owner.", 400);

        if (allowAccess == groupEntity.GroupMembers.Any(m => m.UserId == userToGrant.Id && m.GroupId == groupId))
            return Result<string>.Error($"The user has already been " + (allowAccess ? "granted" : "revoked") + " access.", 400);






        if (allowAccess)
            groupEntity.GroupMembers.Add(new GroupMemberEntity() { GroupId = groupId, UserId = userToGrant.Id, RoleInGroup = "Member" });
        else
        {

            groupEntity.GroupMembers.RemoveWhere(m => m.UserId == userToGrant.Id && m.GroupId == groupId);
        }
        groupEntity.LastModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        if (allowAccess)
            await _clientSyncService.AddUserToGroup(userToGrant.Id, groupEntity.Id);
        else
            await _clientSyncService.RemoveUserFromGroup(userToGrant.Id, groupEntity.Id);

        await _clientSyncService.NotifyGroupMembersToSync(groupEntity.Id);
        return Result<string>.Success(allowAccess ? "Access granted." : "Access revoked.");
    }

    private async Task<Result<string>> UpdateGroupEntity(GroupEntity groupEntity, UpdateGroupDTO updateGroupDTO)
    {
        if (updateGroupDTO.Title == groupEntity.Title && updateGroupDTO.Description == groupEntity.Description)
            return Result<string>.Error("Group details are the same.", 400);

        groupEntity.Title = updateGroupDTO.Title ?? groupEntity.Title;
        groupEntity.Description = updateGroupDTO.Description ?? groupEntity.Description;
        groupEntity.LastModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await _clientSyncService.NotifyGroupMembersToSync(groupEntity.Id);


        return Result<string>.Success("Group updated.");
    }
}