using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Hubs;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Models.DTOs.Sync;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class GroupsService(IMapper mapper, ILogger<GroupsService> logger, SyncoraDbContext dbContext, ClientSyncService clientSyncService)
{
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<GroupsService> _logger = logger;
    private readonly SyncoraDbContext _dbContext = dbContext;

    private readonly ClientSyncService _clientSyncService = clientSyncService;

    public async Task<Result<GroupDTO>> GetGroup(int userId, int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().Include(g => g.GroupMembers).ThenInclude(m => m.User).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);

        if (groupEntity == null)
            return Result<GroupDTO>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        if (groupEntity.OwnerUserId == userId || groupEntity.GroupMembers.Any(m => m.UserId == userId && m.KickedAt == null))
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
            groups = await _dbContext.Groups.AsNoTracking().Where(g => (g.OwnerUserId == userId || g.GroupMembers.Any(m => m.UserId == userId && m.KickedAt == null)) && g.LastModifiedDate > sinceUtc && g.DeletedAt == null).OrderBy(t => t.CreationDate).ProjectTo<GroupDTO>(_mapper.ConfigurationProvider).ToListAsync();
        }
        else
        {
            groups = await _dbContext.Groups.AsNoTracking().Where(g => (g.OwnerUserId == userId || g.GroupMembers.Any(m => m.UserId == userId && m.KickedAt == null)) && g.DeletedAt == null).OrderBy(t => t.CreationDate).ProjectTo<GroupDTO>(_mapper.ConfigurationProvider).ToListAsync();

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
        await _clientSyncService.AddUserToHubGroup(userId, createdGroup.Id);


        return Result<GroupDTO>.Success(_mapper.Map<GroupDTO>(createdGroup));
    }


    public async Task<Result<string>> UpdateGroup(UpdateGroupDTO updateGroupDTO, int userId, int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers.Where(m => m.KickedAt == null)).ThenInclude(m => m.User).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);

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

        if (updateGroupDTO.Title != null)
            if (!Validators.ValidateTitle(updateGroupDTO.Title))
                return Result<string>.Error("Title is invalid.", StatusCodes.Status400BadRequest);

        if (updateGroupDTO.Description != null)
            if (!Validators.ValidateTitle(updateGroupDTO.Description))
                return Result<string>.Error("Description is invalid.", StatusCodes.Status400BadRequest);


        if (updateGroupDTO.Title == groupEntity.Title && updateGroupDTO.Description == groupEntity.Description)
            return Result<string>.Error("Group details are the same.", 400);

        groupEntity.Title = updateGroupDTO.Title ?? groupEntity.Title;
        groupEntity.Description = updateGroupDTO.Description ?? groupEntity.Description;
        groupEntity.LastModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await _clientSyncService.PushPayloadToGroup(groupEntity.Id, SyncPayload.FromEntity(Groups: [groupEntity]));


        return Result<string>.Success("Group updated.");
    }

    public async Task<Result<string>> DeleteGroup(int userId, int groupId)
    {

        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers.Where(m => m.KickedAt == null)).ThenInclude(m => m.User).FirstOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);
        // Make sure the group exists
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == userId && m.KickedAt == null);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't delete a group they don't own", StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);


        groupEntity.DeletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        await _clientSyncService.PushPayloadToGroup(groupEntity.Id, SyncPayload.FromEntity(DeletedGroupsIds: [groupEntity.Id]));


        return Result<string>.Success("Group deleted.");

    }

    // public async Task<Result<UserDTO>> AllowAccessToGroup(int groupId, int userId, List<string> usernamesToGrant, bool allowAccess)
    // {
    //     GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers).ThenInclude(m => m.User).Include(g => g.OwnerUser).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);

    //     if (groupEntity == null)
    //         return Result<UserDTO>.Error("Group does not exist.", StatusCodes.Status404NotFound);

    //     bool isOwner = groupEntity.OwnerUserId == userId;
    //     bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == userId);
    //     if (!isOwner && isShared)
    //     {
    //         return Result<UserDTO>.Error("A shared user can't " + (allowAccess ? "grant" : "revoke") + " access to a group they don't own", StatusCodes.Status403Forbidden);
    //     }
    //     else if (!isOwner && !isShared)
    //         return Result<UserDTO>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);



    //     List<UserEntity> users = await _dbContext.Users.Where(u => usernamesToGrant.Contains(u.Username)).ToListAsync();

    //     if (users.Count != usernamesToGrant.Count)
    //         return Result<UserDTO>.Error((usernamesToGrant.Count - users.Count) + " Users do not exist.", StatusCodes.Status404NotFound);

    //     if (users.Any(u => u.Id == userId))
    //         return Result<UserDTO>.Error("You can't grant or revoke access to yourself as the group owner.", 400);

    //     foreach (UserEntity userToGrant in users)
    //     {
    //         bool isUserAlreadyRevokedOrGranted = (allowAccess == groupEntity.GroupMembers.Any(m => m.UserId == userToGrant.Id && m.GroupId == groupId && m.KickedAt == null));

    //         if (isUserAlreadyRevokedOrGranted)
    //             return Result<UserDTO>.Error($"User {userToGrant.Username} has already been " + (allowAccess ? "granted" : "revoked") + " access.", 400);

    //     }


    //     List<TaskEntity> assignedTasks = [];
    //     if (allowAccess)
    //     {
    //         GroupMemberEntity? groupMember = groupEntity.GroupMembers.FirstOrDefault(m => m.UserId == userToGrant.Id && m.GroupId == groupId);

    //         if (groupMember == null)
    //         {
    //             groupEntity.GroupMembers.Add(new GroupMemberEntity() { GroupId = groupId, UserId = userToGrant.Id, RoleInGroup = "Member" });
    //         }
    //         else
    //         {
    //             groupMember.KickedAt = null;
    //             groupMember.JoinedAt = DateTime.UtcNow;
    //         }

    //     }
    //     else
    //     {
    //         GroupMemberEntity groupMember = groupEntity.GroupMembers.First(m => m.UserId == userToGrant.Id && m.GroupId == groupId);
    //         groupMember.KickedAt = DateTime.UtcNow;


    //         assignedTasks = await _dbContext.Tasks.Include(t => t.AssignedTo).Where(t => t.GroupId == groupId && t.AssignedTo.Contains(userToGrant) && t.DeletedAt == null).ToListAsync();

    //         foreach (TaskEntity task in assignedTasks)
    //         {
    //             task.AssignedTo.Remove(userToGrant);
    //             if (task.CompletedById == userToGrant.Id)
    //                 task.CompletedById = null;

    //             task.LastModifiedDate = DateTime.UtcNow;
    //         }

    //     }
    //     groupEntity.LastModifiedDate = DateTime.UtcNow;

    //     await _dbContext.SaveChangesAsync();


    //     // Pushing changes to the members
    //     if (allowAccess)
    //     {
    //         //If we allowed access, we send the new user all the group data
    //         List<UserEntity> usersData = groupEntity.GroupMembers.Where(m => m.KickedAt == null).Select(m => m.User).ToList();
    //         usersData.Add(groupEntity.OwnerUser);

    //         assignedTasks = await _dbContext.Tasks.Include(t => t.AssignedTo).Where(t => t.GroupId == groupId && t.AssignedTo.Contains(userToGrant) && t.DeletedAt == null).ToListAsync();

    //         List<TaskEntity> tasksData = await _dbContext.Tasks.Include(t => t.AssignedTo).Where(t => t.GroupId == groupId && t.DeletedAt == null).ToListAsync();


    //         // Sending the group + users + tasks to the user that was just added
    //         await _clientSyncService.PushPayloadToPerson(userToGrant.Id, SyncPayload.FromEntity(Groups: [groupEntity.ExcludeKickedUsers()], Users: usersData, Tasks: tasksData));

    //         await _clientSyncService.PushPayloadToGroup(groupEntity.Id, SyncPayload.FromEntity(Groups: [groupEntity.ExcludeKickedUsers()], Users: [userToGrant]));
    //         await _clientSyncService.AddUserToHubGroup(userToGrant.Id, groupEntity.Id);
    //     }
    //     else
    //     {
    //         // Remove the user from the group hub
    //         await _clientSyncService.RemoveUserFromHubGroup(userToGrant.Id, groupEntity.Id);

    //         // Sending a payload to the user that they have been removed
    //         await _clientSyncService.PushPayloadToPerson(userToGrant.Id, SyncPayload.FromEntity(KickedGroupsIds: [groupEntity.Id]));

    //         // Sending a payload to the entire group members without kicked users, and updated tasks without that user assigned
    //         await _clientSyncService.PushPayloadToGroup(groupEntity.Id, SyncPayload.FromEntity(Groups: [groupEntity.ExcludeKickedUsers()], Tasks: assignedTasks));

    //     }

    //     return Result<UserDTO>.Success(_mapper.Map<UserDTO>(userToGrant));
    // }

    public async Task<Result<string>> GrantAccessToGroup(int groupId, int userId, List<string> usernamesToGrant)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers).Include(g => g.OwnerUser).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);

        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == userId && m.KickedAt == null);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't grant access to a group they don't own", StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);


        if (usernamesToGrant.Count == 0)
            return Result<string>.Error("No usernames were provided.", StatusCodes.Status400BadRequest);



        List<UserEntity> usersToGrant = await _dbContext.Users.Where(u => usernamesToGrant.Contains(u.Username)).ToListAsync();
        List<int> usersIdsToGrant = usersToGrant.Select(u => u.Id).ToList();
        var now = DateTime.UtcNow;

        if (usersToGrant.Count != usernamesToGrant.Count)
            return Result<string>.Error((usernamesToGrant.Count - usersToGrant.Count) + " Users do not exist.", StatusCodes.Status404NotFound);

        if (usersToGrant.Any(u => u.Id == userId))
            return Result<string>.Error("You can't grant access to yourself as the group owner.", 400);

        foreach (UserEntity userToGrant in usersToGrant)
        {
            bool isUserAlreadyGranted = groupEntity.GroupMembers.Any(m => m.UserId == userToGrant.Id && m.GroupId == groupId && m.KickedAt == null);

            if (isUserAlreadyGranted)
                return Result<string>.Error($"User {userToGrant.Username} has already been granted access.", 400);
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
            return Result<string>.Error("Failed to grant access to group.", StatusCodes.Status500InternalServerError);
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
        return Result<string>.Success("Access granted.");
    }

    public async Task<Result<string>> RevokeAccessToGroup(int groupId, int userId, List<string> usernamesToRevoke)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);

        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == userId && m.KickedAt == null);
        if (!isOwner && isShared)
        {
            return Result<string>.Error("A shared user can't revoke access to a group they don't own", StatusCodes.Status403Forbidden);
        }
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);

        if (usernamesToRevoke.Count == 0)
            return Result<string>.Error("No usernames were provided.", StatusCodes.Status400BadRequest);

        List<UserEntity> usersToRevoke = await _dbContext.Users.Where(u => usernamesToRevoke.Contains(u.Username)).ToListAsync();
        List<int> usersIdsToRevoke = [.. usersToRevoke.Select(u => u.Id)];
        List<TaskEntity> assignedTasks = [];
        var now = DateTime.UtcNow;

        if (usersToRevoke.Count != usernamesToRevoke.Count)
            return Result<string>.Error((usernamesToRevoke.Count - usersToRevoke.Count) + " Users do not exist.", StatusCodes.Status404NotFound);

        if (usersToRevoke.Any(u => u.Id == userId))
            return Result<string>.Error("You can't revoke access to yourself as the group owner.", 400);

        foreach (UserEntity userToRevoke in usersToRevoke)
        {
            bool isUserAlreadyRevoked = !groupEntity.GroupMembers.Any(m => m.UserId == userToRevoke.Id && m.GroupId == groupId && m.KickedAt == null);

            if (isUserAlreadyRevoked)
                return Result<string>.Error($"User {userToRevoke.Username} has already been revoked access.", 400);
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
            // Commit transaction if all commands succeed, transaction will auto-rollback
            // when disposed if either commands fails
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to revoke access to group.");
            await transaction.RollbackAsync();
            return Result<string>.Error("Failed to revoke access to group.", StatusCodes.Status500InternalServerError);
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

    public async Task<Result<string>> LeaveGroup(int groupId, int userId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);

        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        bool isOwner = groupEntity.OwnerUserId == userId;
        bool isShared = groupEntity.GroupMembers.Any(m => m.UserId == userId && m.KickedAt == null);

        if (isOwner)
            return Result<string>.Error("Owners can't leave the group.", StatusCodes.Status403Forbidden);
        else if (!isOwner && !isShared)
            return Result<string>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);


        // Getting the user leaving
        UserEntity userLeaving = await _dbContext.Users.FirstAsync(u => u.Id == userId);

        // Removing the user from the group
        groupEntity.GroupMembers.RemoveWhere(m => m.UserId == userId && m.GroupId == groupId);


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
        await _clientSyncService.RemoveUserFromHubGroup(userId, groupEntity.Id);

        // Sending a payload to the entire group members without kicked users, and updated tasks without that user assigned
        await _clientSyncService.PushPayloadToGroup(groupEntity.Id, SyncPayload.FromEntity(Groups: [groupEntity.ExcludeKickedUsers()], Tasks: assignedTasks));

        // Sending to the user leaving the group
        await _clientSyncService.PushPayloadToPerson(userId, SyncPayload.FromEntity(DeletedGroupsIds: [groupEntity.Id]));


        return Result<string>.Success("User left group.");

    }
}