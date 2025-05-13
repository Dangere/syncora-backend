using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Models.DTOs.Tasks;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

class ClientSyncService(GroupService groupService, UsersService userService, TaskService taskService)
{
    private readonly GroupService _groupService = groupService;
    private readonly UsersService _userService = userService;
    private readonly TaskService _taskService = taskService;


    public async Task<Result<Dictionary<string, object>>> SyncSince(int userId, DateTime since)
    {
        Result<List<GroupDTO>> groups = await _groupService.GetGroups(userId, since);
        if (!groups.IsSuccess)
            return Result<Dictionary<string, object>>.Error(groups.ErrorMessage!, groups.ErrorStatusCode);

        List<TaskDTO> tasks = [];
        List<UserDTO> users = [];


        for (int i = 0; i < groups.Data!.Count; i++)
        {
            Result<List<TaskDTO>> tasksPerGroup = await _taskService.GetTasksForUser(userId, groups.Data[i].Id, since);
            if (!tasksPerGroup.IsSuccess)
                return Result<Dictionary<string, object>>.Error(tasksPerGroup.ErrorMessage!, tasksPerGroup.ErrorStatusCode);
            tasks.AddRange(tasksPerGroup.Data!);

            Result<List<UserDTO>> usersPerGroup = await _userService.GetUsersInGroup(userId, groups.Data[i].Id, since);
            if (!usersPerGroup.IsSuccess)
                return Result<Dictionary<string, object>>.Error(usersPerGroup.ErrorMessage!, usersPerGroup.ErrorStatusCode);
            users.AddRange(usersPerGroup.Data!);
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