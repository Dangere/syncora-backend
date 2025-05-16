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

    public async Task<Result<Dictionary<string, object>>> SyncSinceTimestamp(int userId, DateTime since, bool includeDeleted = false)
    {
        DateTime utcSince = since.Kind == DateTimeKind.Utc ? since : since.ToUniversalTime();

        var raw = await _dbContext.Groups
            .AsNoTracking()
            .Where(g => g.OwnerUserId == userId || g.Members.Any(m => m.Id == userId))
            .Select(g => new
            {
                Group = new GroupDTO(g.Id, g.Title, g.Description, g.CreationDate, g.LastModifiedDate, g.OwnerUserId, g.Members.Select(m => m.Username).ToArray()),
                Users = g.Members
                             .Where(m => m.LastModifiedDate > utcSince)
                             .Select(m => new UserDTO(m.Id, m.Email, m.Username, m.Role.ToString(), m.CreationDate, m.LastModifiedDate, m.ProfilePictureURL)),
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
            { "groups",groupDTOs},
            { "users", userDTOs },
            { "tasks", taskDTOs }
            // Need to include deleted groups 
        };
        return Result<Dictionary<string, object>>.Success(payload);
    }
}