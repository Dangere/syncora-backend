using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Enums;
using SyncoraBackend.Hubs;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Models.DTOs.Tasks;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class ClientSyncService(SyncoraDbContext dbContext, IHubContext<SyncHub> hubContext, InMemoryHubConnectionManager connectionManager)
{
    private readonly SyncoraDbContext _dbContext = dbContext;
    private readonly IHubContext<SyncHub> _hubContext = hubContext;

    private readonly InMemoryHubConnectionManager _connectionManager = connectionManager;

    // The sync method will return a data batch to the client
    // When its the first time the client is syncing (i.e logging in), it will get all the data using an old 'since' timestamp
    // After the initial sync, it will get all the data since the last sync, by returning data with a 'LastModifiedDate' later than the last sync
    // This way after the initial sync, the client will get only the newly added data to be cached, and only that. making for a smaller payload
    // The method will return added, modified or deleted groups
    // The method will return added, modified or deleted tasks
    // The method will return modified or added users to groups, however it will not return deleted users, because the client will know if users are in groups and automatically delete them
    // The client takes the data and stores it locally by inserting and updating the groups, tasks and users

    // TODO: Add deleted groups in record when a user deletes their account
    // TODO: Add deleted tasks in record when a group is deleted
    // TODO: Update groups when a user deletes their account
    // TODO: Make all clients call the sync endpoint whenever someone modifies data  (Done)
    // Remember: The client will also delete all tasks associated with a group on group deletion cascading, the same goes for groups when owner deletes their account

    // BUG: When a user is added to a group and they are given a sync payload for the first time, the user gets their own user data because their JoinedAt is later than the last sync
    // BUG:  When a user is added to a group and they are given a sync payload for the first time, they don't get the already added users in the group
    // BUG: When a shared user tried to get a payload, they get the data for all the member in group, EXCEPT the owner's user data
    public async Task<Result<Dictionary<string, object>>> SyncSinceTimestamp(int userId, DateTime since, bool includeDeleted = false)
    {
        DateTime utcSince = since.Kind == DateTimeKind.Utc ? since : since.ToUniversalTime();

        // Getting all groups (regardless of the modification date which gets filtered later) then tasks and users since the last sync
        var raw = await _dbContext.Groups
            .AsNoTracking()
            // We select the groups that are either owned by the user or shared with the user and that are not deleted or kicked from
            .Where(g => (g.OwnerUserId == userId || g.GroupMembers.Any(m => m.UserId == userId && m.KickedAt == null)) && g.DeletedAt == null)
            .Select(g => new
            {
                Group = new GroupDTO(g.Id, g.Title, g.Description, g.CreationDate, g.LastModifiedDate, g.OwnerUserId, g.GroupMembers.Where(m => m.KickedAt == null).Select(m => m.User.Id).ToArray()),
                Users = g.GroupMembers
                             // We select the users that newly joined or were newly modified or all of them if the group was modified
                             .Where(m => (m.JoinedAt > utcSince || m.User.LastModifiedDate > utcSince || g.LastModifiedDate > utcSince) && m.KickedAt == null)
                             .Select(m => new UserDTO(m.User.Id, m.User.Email, m.User.Username, m.User.Role.ToString(), m.User.CreationDate, m.User.LastModifiedDate, m.User.ProfilePictureURL)),
                Tasks = g.Tasks
                             .Where(t => t.LastModifiedDate > utcSince && t.DeletedAt == null)
                             .Select(t => new TaskDTO(t.Id, t.Title, t.Description, t.CompletedById, t.AssignedTo.Select(m => m.Id).ToArray(), t.CreationDate, t.LastModifiedDate, t.GroupId)),//_mapper.Map<TaskDTO>(t, t.CreationDate, t.LastModifiedDate, t.GroupId)),

                Owner = (g.OwnerUser.LastModifiedDate > utcSince || g.LastModifiedDate > utcSince) ? new UserDTO(g.OwnerUser.Id, g.OwnerUser.Email, g.OwnerUser.Username, g.OwnerUser.Role.ToString(), g.OwnerUser.CreationDate, g.OwnerUser.LastModifiedDate, g.OwnerUser.ProfilePictureURL) : null
            })
            .ToListAsync();

        List<GroupDTO> groupDTOs = raw.Select(x => x.Group).Where(x => x.LastModifiedDate > utcSince).ToList();
        HashSet<UserDTO> userDTOs = raw.SelectMany(x => x.Users).Union(raw.Select(x => x.Owner).Where(x => x != null).Cast<UserDTO>()).ToHashSet();
        List<TaskDTO> taskDTOs = raw.SelectMany(x => x.Tasks).ToList();



        Dictionary<string, object> payload = new()
        {
            {"timestamp", DateTime.UtcNow},
            { "groups", groupDTOs},
            { "users", userDTOs },
            { "tasks", taskDTOs },
        };


        if (includeDeleted)
        {
            List<int> kickedGroups = await _dbContext.Groups.AsNoTracking().Where(g => g.GroupMembers.Any(m => m.UserId == userId && m.KickedAt > utcSince)).Select(g => g.Id).ToListAsync();
            List<int> deletedGroups = await _dbContext.Groups.AsNoTracking().Where(g => g.DeletedAt > utcSince).Select(g => g.Id).ToListAsync();
            List<int> deletedTasks = await _dbContext.Tasks.AsNoTracking().Where(t => t.DeletedAt > utcSince && (t.Group.GroupMembers.Any(m => m.UserId == userId) || t.Group.OwnerUserId == userId)).Select(t => t.Id).ToListAsync();

            payload.Add("kickedGroupsIds", kickedGroups);
            payload.Add("deletedTasks", deletedTasks);
            payload.Add("deletedGroups", deletedGroups);
        }
        return Result<Dictionary<string, object>>.Success(payload);
    }


    // This should get called whenever a user gets added to a group before the sync is triggered
    public async Task AddUserToHubGroup(int userId, int groupId)
    {
        IReadOnlyList<string> connections = _connectionManager.GetConnections(userId);

        foreach (string connectionId in connections)
        {
            await _hubContext.Groups.AddToGroupAsync(connectionId, $"group-{groupId}");

        }

    }

    // This should get called whenever a user gets removed to a group before the sync is triggered
    public async Task RemoveUserFromHubGroup(int userId, int groupId)
    {
        IReadOnlyList<string> connections = _connectionManager.GetConnections(userId);


        foreach (string connectionId in connections)
        {
            await _hubContext.Groups.RemoveFromGroupAsync(connectionId, $"group-{groupId}");

        }
    }

    public async Task NotifyGroupMembersToSync(int groupId)
    {
        Console.WriteLine("Sending sync payload");
        await _hubContext.Clients.Groups($"group-{groupId}").SendAsync("ReceiveSync");
    }
}