using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Data;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Models.DTOs.Tasks;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class ClientSyncService(SyncoraDbContext dbContext)
{
    private readonly SyncoraDbContext _dbContext = dbContext;

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
    // TODO: Make all clients call the sync endpoint whenever someone modifies data
    // Remember: The client will also delete all tasks associated with a group on group deletion cascading, the same goes for groups when owner deletes their account
    public async Task<Result<Dictionary<string, object>>> SyncSinceTimestamp(int userId, DateTime since, bool includeDeleted = false)
    {
        DateTime utcSince = since.Kind == DateTimeKind.Utc ? since : since.ToUniversalTime();

        // Getting all groups (regardless of the modification date which gets filtered later) then tasks and users since the last sync
        var raw = await _dbContext.Groups
            .AsNoTracking()
            .Where(g => (g.OwnerUserId == userId || g.GroupMembers.Any(m => m.UserId == userId)) && g.DeletedDate == null)
            .Select(g => new
            {
                Group = new GroupDTO(g.Id, g.Title, g.Description, g.CreationDate, g.LastModifiedDate, g.OwnerUserId, g.GroupMembers.Select(m => m.User.Id).ToArray()),
                Users = g.GroupMembers
                             .Where(m => m.JoinedAt > utcSince || m.User.LastModifiedDate > utcSince)
                             .Select(m => new UserDTO(m.User.Id, m.User.Email, m.User.Username, m.User.Role.ToString(), m.User.CreationDate, m.User.LastModifiedDate, m.User.ProfilePictureURL)),
                Tasks = g.Tasks
                             .Where(t => t.LastModifiedDate > utcSince)
                             .Select(t => new TaskDTO(t.Id, t.Title, t.Description, t.Completed, t.CompletedById, t.CreationDate, t.LastModifiedDate, t.GroupId))
            })
            .ToListAsync();

        List<GroupDTO> groupDTOs = raw.Select(x => x.Group).Where(x => x.LastModifiedDate > utcSince).ToList();
        HashSet<UserDTO> userDTOs = raw.SelectMany(x => x.Users).ToHashSet();
        List<TaskDTO> taskDTOs = raw.SelectMany(x => x.Tasks).ToList();



        Dictionary<string, object> payload = new()
        {
            { "groups", groupDTOs},
            { "users", userDTOs },
            { "tasks", taskDTOs },
        };


        // This actually increases the fetch time by a lot, optimize it
        if (includeDeleted)
        {
            List<int> deletedGroups = _dbContext.Groups.AsNoTracking().Where(g => g.DeletedDate != null && g.DeletedDate > utcSince).Select(g => g.Id).ToList();
            List<int> deletedTasks = _dbContext.DeletedRecords.AsNoTracking().Where(dr => groupDTOs.Select(g => g.Id).Contains(dr.EntityId) && dr.TableName == "tasks" && dr.DeletedAt > utcSince).Select(dr => dr.EntityId).ToList();

            payload.Add("deletedGroups", deletedGroups);
            payload.Add("deletedTasks", deletedTasks);
        }
        return Result<Dictionary<string, object>>.Success(payload);
    }
}