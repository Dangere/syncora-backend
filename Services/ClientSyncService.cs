using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Models.DTOs.Tasks;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class ClientSyncService(GroupService groupService, UsersService userService, TaskService taskService)
{
    private readonly GroupService _groupService = groupService;
    private readonly UsersService _userService = userService;
    private readonly TaskService _taskService = taskService;


    public async Task<Result<Dictionary<string, object>>> SyncSince(int userId, DateTime since)
    {
        DateTime utcSince = since.Kind == DateTimeKind.Utc ? since : since.ToUniversalTime();


        Result<List<GroupDTO>> groups = await _groupService.GetGroups(userId, utcSince);
        if (!groups.IsSuccess)
            return Result<Dictionary<string, object>>.Error(groups.ErrorMessage! + "Getting groups", groups.ErrorStatusCode);

        List<TaskDTO> tasks = [];
        HashSet<UserDTO> users = [];


        for (int i = 0; i < groups.Data!.Count; i++)
        {
            Result<List<TaskDTO>> tasksPerGroup = await _taskService.GetTasksForUser(userId, groups.Data[i].Id, utcSince);
            if (!tasksPerGroup.IsSuccess)
                return Result<Dictionary<string, object>>.Error(tasksPerGroup.ErrorMessage! + "Getting tasks", tasksPerGroup.ErrorStatusCode);
            tasks.AddRange(tasksPerGroup.Data!);

            Result<List<UserDTO>> usersPerGroup = await _userService.GetUsersInGroup(userId, groups.Data[i].Id, utcSince);
            if (!usersPerGroup.IsSuccess)
                return Result<Dictionary<string, object>>.Error(usersPerGroup.ErrorMessage! + "Getting users", usersPerGroup.ErrorStatusCode);
            usersPerGroup.Data!.ForEach(x => { users.Add(x); });


        }

        Dictionary<string, object> payload = new()
        {
            { "groups", groups.Data },
            { "users", users },
            { "tasks", tasks }
            // Need to include deleted groups 
        };

        return Result<Dictionary<string, object>>.Success(payload);

    }
}