using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Hubs;
using SyncoraBackend.Models;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Models.DTOs.Sync;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class GroupsService(IMapper mapper, ILogger<GroupsService> logger, SyncoraDbContext dbContext, ClientSyncService clientSyncService, UserRequestContext userRequestContext)
{
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<GroupsService> _logger = logger;
    private readonly SyncoraDbContext _dbContext = dbContext;

    private readonly ClientSyncService _clientSyncService = clientSyncService;
    private readonly UserRequestContext _userRequestContext = userRequestContext;
    public async Task<Result<GroupDTO>> GetGroup(int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().Include(g => g.GroupMembers).ThenInclude(m => m.User).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);

        if (groupEntity == null)
            return Result<GroupDTO>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);

        if (groupEntity.OwnerUserId == _userRequestContext.UserId || groupEntity.GroupMembers.Any(m => m.UserId == _userRequestContext.UserId && m.KickedAt == null))
            return new Result<GroupDTO>(_mapper.Map<GroupDTO>(groupEntity));

        return Result<GroupDTO>.Error("User has no access to this group.", ErrorCodes.ACCESS_DENIED, StatusCodes.Status403Forbidden);
    }

    // TODO: Add pagination
    // Returns groups owned by the user or shared with the user
    public async Task<List<GroupDTO>> GetGroups()
    {


        return await _dbContext.Groups.AsNoTracking().Where(g => (g.OwnerUserId == _userRequestContext.UserId || g.GroupMembers.Any(m => m.UserId == _userRequestContext.UserId && m.KickedAt == null)) && g.DeletedAt == null).OrderBy(t => t.CreationDate).ProjectTo<GroupDTO>(_mapper.ConfigurationProvider).ToListAsync();

    }
    /// <summary>
    /// Returns groups ids owned by the user or shared with the user
    /// </summary>
    /// <param name="sinceUtc"></param>
    /// <returns></returns>

    public async Task<List<int>> GetGroupIds()
    {

        return await _dbContext.Groups.AsNoTracking().Where(g => (g.OwnerUserId == _userRequestContext.UserId || g.GroupMembers.Any(m => m.UserId == _userRequestContext.UserId && m.KickedAt == null)) && g.DeletedAt == null).OrderBy(t => t.CreationDate).Select(g => g.Id).ToListAsync();

    }

    public async Task<Result<GroupDTO>> CreateGroup(CreateGroupDTO createGroupDTO)
    {
        // Make sure the user exists
        if (await _dbContext.Users.FindAsync(_userRequestContext.UserId) == null)
            return Result<GroupDTO>.Error("User does not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        GroupEntity createdGroup = new() { Title = createGroupDTO.Title, Description = createGroupDTO.Description, OwnerUserId = _userRequestContext.UserId };

        await _dbContext.Groups.AddAsync(createdGroup);
        await _dbContext.SaveChangesAsync();
        await _clientSyncService.AddUserToHubGroup(_userRequestContext.UserId, createdGroup.Id);

        return Result<GroupDTO>.Success(_mapper.Map<GroupDTO>(createdGroup));
    }

    public async Task<Result<string>> UpdateGroup(UpdateGroupDTO updateGroupDTO, int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers.Where(m => m.KickedAt == null)).ThenInclude(m => m.User).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);

        // Make sure the group exists
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == _userRequestContext.UserId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == _userRequestContext.UserId);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't update the details of a group they don't own", ErrorCodes.SHARED_USER_CANNOT_PERFORM_ACTION, StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", ErrorCodes.ACCESS_DENIED, StatusCodes.Status403Forbidden);

        if (updateGroupDTO.Title == groupEntity.Title && updateGroupDTO.Description == groupEntity.Description)
            return Result<string>.Error("Group details are the same.", ErrorCodes.GROUP_DETAILS_UNCHANGED);

        groupEntity.Title = updateGroupDTO.Title ?? groupEntity.Title;
        groupEntity.Description = updateGroupDTO.Description ?? groupEntity.Description;
        groupEntity.LastModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await _clientSyncService.PushPayloadToGroup(groupEntity.Id, SyncPayload.FromEntity(Groups: [groupEntity]));

        return Result<string>.Success("Group updated.");
    }

    public async Task<Result<string>> DeleteGroup(int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers.Where(m => m.KickedAt == null)).ThenInclude(m => m.User).FirstOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);
        // Make sure the group exists
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == _userRequestContext.UserId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == _userRequestContext.UserId && m.KickedAt == null);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't delete a group they don't own", ErrorCodes.SHARED_USER_CANNOT_PERFORM_ACTION, StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", ErrorCodes.ACCESS_DENIED, StatusCodes.Status403Forbidden);

        groupEntity.DeletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        await _clientSyncService.PushPayloadToGroup(groupEntity.Id, SyncPayload.FromEntity(DeletedGroupsIds: [groupEntity.Id]));

        return Result<string>.Success("Group deleted.");
    }

    // public async Task<Result<UserDTO>> AllowAccessToGroup(int groupId, , List<string> usernamesToGrant, bool allowAccess)
    // { ... }

    public async Task<Result<List<UserDTO>>> GrantAccessToGroup(int groupId, List<string> usernamesToGrant)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers).Include(g => g.OwnerUser).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);

        if (groupEntity == null)
            return Result<List<UserDTO>>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == _userRequestContext.UserId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == _userRequestContext.UserId && m.KickedAt == null);
        if (!isOwner && isShared)
        {
            return Result<List<UserDTO>>.Error("A shared user can't grant access to a group they don't own", ErrorCodes.SHARED_USER_CANNOT_PERFORM_ACTION, StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<List<UserDTO>>.Error("User has no access to this group.", ErrorCodes.ACCESS_DENIED, StatusCodes.Status403Forbidden);

        if (usernamesToGrant.Count == 0)
            return Result<List<UserDTO>>.Error("No usernames were provided.", ErrorCodes.NO_USERNAMES_PROVIDED, StatusCodes.Status400BadRequest);

        List<UserEntity> usersToGrant = await _dbContext.Users.Where(u => usernamesToGrant.Contains(u.Username)).ToListAsync();
        List<int> usersIdsToGrant = usersToGrant.Select(u => u.Id).ToList();
        var now = DateTime.UtcNow;

        if (usersToGrant.Count != usernamesToGrant.Count)
            return Result<List<UserDTO>>.Error((usernamesToGrant.Count - usersToGrant.Count) + " Users do not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        if (usersToGrant.Any(u => u.Id == _userRequestContext.UserId))
            return Result<List<UserDTO>>.Error("You can't grant access to yourself as the group owner.", ErrorCodes.OWNER_CANNOT_PERFORM_ACTION);

        foreach (UserEntity userToGrant in usersToGrant)
        {
            bool isUserAlreadyGranted = groupEntity.GroupMembers.Any(m => m.UserId == userToGrant.Id && m.GroupId == groupId && m.KickedAt == null);

            if (isUserAlreadyGranted)
                return Result<List<UserDTO>>.Error($"User {userToGrant.Username} has already been granted access.", ErrorCodes.USER_ALREADY_GRANTED);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            List<GroupMemberEntity> groupMemberships = groupEntity.GroupMembers.Where(m => usersIdsToGrant.Contains(m.UserId)).ToList();
            _logger.LogInformation("Current member count {Count}", groupEntity.GroupMembers.Count);

            foreach (int userIdToGrant in usersIdsToGrant)
            {
                if (!groupMemberships.Any(m => m.UserId == userIdToGrant))
                {
                    groupEntity.GroupMembers.Add(new GroupMemberEntity() { GroupId = groupId, UserId = userIdToGrant, RoleInGroup = "Member" });
                }
                else
                {
                    groupMemberships.Single(m => m.UserId == userIdToGrant).KickedAt = null;
                    groupMemberships.Single(m => m.UserId == userIdToGrant).JoinedAt = now;
                }
            }
            _logger.LogInformation("New member count {Count}", groupEntity.GroupMembers.Count);

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to grant access to group.");
            await transaction.RollbackAsync();
            return Result<List<UserDTO>>.Error("Failed to grant access to group.", ErrorCodes.INTERNAL_ERROR, StatusCodes.Status500InternalServerError);
        }

        //We get the stored data to send it to the clients
        List<UserEntity> usersData = await _dbContext.GroupMembers.Where(m => m.GroupId == groupId && m.KickedAt == null).Select(m => m.User).ToListAsync();
        usersData.Add(groupEntity.OwnerUser);
        List<TaskEntity> tasksData = await _dbContext.Tasks.Include(t => t.AssignedTo).Where(t => t.GroupId == groupId && t.DeletedAt == null).ToListAsync();

        // Sending the new users to the entire group
        await _clientSyncService.PushPayloadToGroup(groupEntity.Id, SyncPayload.FromEntity(Groups: [groupEntity.ExcludeKickedUsers()], Users: usersToGrant));

        foreach (UserEntity userToGrant in usersToGrant)
        {
            // Sending the group + users + tasks to the users that was just added
            await _clientSyncService.PushPayloadToPerson(userToGrant.Id, SyncPayload.FromEntity(Groups: [groupEntity.ExcludeKickedUsers()], Users: usersData, Tasks: tasksData));
            await _clientSyncService.AddUserToHubGroup(userToGrant.Id, groupEntity.Id);
        }
        return Result<List<UserDTO>>.Success([.. usersToGrant.Select(_mapper.Map<UserDTO>)]);
    }

    public async Task<Result<string>> RevokeAccessToGroup(int groupId, List<string> usernamesToRevoke)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);

        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == _userRequestContext.UserId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == _userRequestContext.UserId && m.KickedAt == null);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't revoke access to a group they don't own", ErrorCodes.SHARED_USER_CANNOT_PERFORM_ACTION, StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", ErrorCodes.ACCESS_DENIED, StatusCodes.Status403Forbidden);

        if (usernamesToRevoke.Count == 0)
            return Result<string>.Error("No usernames were provided.", ErrorCodes.NO_USERNAMES_PROVIDED, StatusCodes.Status400BadRequest);

        List<UserEntity> usersToRevoke = await _dbContext.Users.Where(u => usernamesToRevoke.Contains(u.Username)).ToListAsync();
        List<int> usersIdsToRevoke = [.. usersToRevoke.Select(u => u.Id)];
        List<TaskEntity> assignedTasks = [];
        var now = DateTime.UtcNow;

        if (usersToRevoke.Count != usernamesToRevoke.Count)
            return Result<string>.Error((usernamesToRevoke.Count - usersToRevoke.Count) + " Users do not exist.", ErrorCodes.USER_NOT_FOUND, StatusCodes.Status404NotFound);

        if (usersToRevoke.Any(u => u.Id == _userRequestContext.UserId))
            return Result<string>.Error("You can't revoke access to yourself as the group owner.", ErrorCodes.OWNER_CANNOT_PERFORM_ACTION);

        foreach (UserEntity userToRevoke in usersToRevoke)
        {
            bool isUserAlreadyRevoked = !groupEntity.GroupMembers.Any(m => m.UserId == userToRevoke.Id && m.GroupId == groupId && m.KickedAt == null);

            if (isUserAlreadyRevoked)
                return Result<string>.Error($"User {userToRevoke.Username} has already been revoked access.", ErrorCodes.USER_ALREADY_REVOKED);
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            List<GroupMemberEntity> groupMemberships = groupEntity.GroupMembers.Where(m => usersIdsToRevoke.Contains(m.UserId)).ToList();

            foreach (GroupMemberEntity groupMember in groupMemberships)
            {
                groupMember.KickedAt = now;
                groupEntity.LastModifiedDate = now;
            }

            assignedTasks = await _dbContext.Tasks.Include(t => t.AssignedTo).Where(t => t.GroupId == groupId && t.AssignedTo.Any(u => usersIdsToRevoke.Contains(u.Id)) && t.DeletedAt == null).ToListAsync();

            foreach (TaskEntity task in assignedTasks)
            {
                task.AssignedTo.RemoveWhere(u => usersIdsToRevoke.Contains(u.Id));
                if (usersIdsToRevoke.Any(id => id == task.CompletedById))
                    task.CompletedById = null;

                task.LastModifiedDate = now;
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke access to group.");
            await transaction.RollbackAsync();
            return Result<string>.Error("Failed to revoke access to group.", ErrorCodes.INTERNAL_ERROR, StatusCodes.Status500InternalServerError);
        }

        foreach (UserEntity userToGrant in usersToRevoke)
        {
            // Remove the user from the group hub
            await _clientSyncService.RemoveUserFromHubGroup(userToGrant.Id, groupEntity.Id);
            // Sending a payload to the user that they have been removed
            await _clientSyncService.PushPayloadToPerson(userToGrant.Id, SyncPayload.FromEntity(KickedGroupsIds: [groupEntity.Id]));
        }

        // Sending a payload to the entire group members without kicked users, and updated tasks without that user assigned
        await _clientSyncService.PushPayloadToGroup(groupEntity.Id, SyncPayload.FromEntity(Groups: [groupEntity.ExcludeKickedUsers()], Tasks: assignedTasks));
        return Result<string>.Success("Access revoked.");
    }

    public async Task<Result<string>> LeaveGroup(int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);

        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == _userRequestContext.UserId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == _userRequestContext.UserId && m.KickedAt == null);

        if (isOwner)
            return Result<string>.Error("Owners can't leave the group.", ErrorCodes.OWNER_CANNOT_PERFORM_ACTION, StatusCodes.Status403Forbidden);
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", ErrorCodes.ACCESS_DENIED, StatusCodes.Status403Forbidden);

        // Getting the user leaving
        UserEntity userLeaving = await _dbContext.Users.FirstAsync(u => u.Id == _userRequestContext.UserId);

        // Removing the user from the group
        groupEntity.GroupMembers.RemoveWhere(m => m.UserId == _userRequestContext.UserId && m.GroupId == groupId);

        // Marking the group as modified so it is queued to sync
        groupEntity.LastModifiedDate = DateTime.UtcNow;

        // Getting the assigned tasks for that user
        var assignedTasks = await _dbContext.Tasks.Include(t => t.AssignedTo).Where(t => t.GroupId == groupId && t.AssignedTo.Contains(userLeaving) && t.DeletedAt == null).ToListAsync();

        // Removing the user from the assigned tasks
        foreach (TaskEntity task in assignedTasks)
        {
            task.AssignedTo.Remove(userLeaving);
            if (task.CompletedById == userLeaving.Id)
                task.CompletedById = null;

            task.LastModifiedDate = DateTime.UtcNow;
        }

        // Saving the changes
        await _dbContext.SaveChangesAsync();

        // Removing the user from the hub group
        await _clientSyncService.RemoveUserFromHubGroup(_userRequestContext.UserId, groupEntity.Id);

        // Sending a payload to the entire group members without kicked users, and updated tasks without that user assigned
        await _clientSyncService.PushPayloadToGroup(groupEntity.Id, SyncPayload.FromEntity(Groups: [groupEntity.ExcludeKickedUsers()], Tasks: assignedTasks));

        // Sending to the user leaving the group
        await _clientSyncService.PushPayloadToPerson(_userRequestContext.UserId, SyncPayload.FromEntity(DeletedGroupsIds: [groupEntity.Id]));

        return Result<string>.Success("User left group.");
    }
}