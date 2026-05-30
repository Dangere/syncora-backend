using AutoMapper;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Models.DTOs.Tasks;
using SyncoraBackend.Data;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Utilities;
using SyncoraBackend.Models.DTOs.Sync;
using SyncoraBackend.Enums;
using SyncoraBackend.Models;

namespace SyncoraBackend.Services;

public class TasksService(IMapper mapper, SyncoraDbContext dbContext, ClientSyncService clientSyncService, UserRequestContext userRequestContext)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;

    private readonly ClientSyncService _clientSyncService = clientSyncService;

    private readonly UserRequestContext _userRequestContext = userRequestContext;

    public async Task<Result<List<TaskDTO>>> GetTasks(int groupId)
    {

        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.Tasks.Where(t => t.DeletedAt == null)).Include(g => g.GroupMembers).ThenInclude(m => m.User).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);
        if (groupEntity == null)
            return Result<List<TaskDTO>>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);

        if (!(groupEntity.OwnerUserId == _userRequestContext.UserId || groupEntity.GroupMembers.Any(m => m.UserId == _userRequestContext.UserId)))
        {
            return Result<List<TaskDTO>>.Error("User has no access to this group.", ErrorCodes.ACCESS_DENIED, StatusCodes.Status403Forbidden);

        }

        List<TaskDTO> tasksDTO;

        tasksDTO = groupEntity.Tasks.OrderBy(t => t.CreationDate).Select(t => _mapper.Map<TaskDTO>(t)).ToList();


        return Result<List<TaskDTO>>.Success(tasksDTO);
    }

    public async Task<Result<TaskDTO>> GetTask(int taskId, int groupId)
    {

        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers.Where(m => m.KickedAt == null)).AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null);
        if (groupEntity == null)
            return Result<TaskDTO>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);

        TaskEntity? taskEntity = await _dbContext.Tasks.Where(t => t.DeletedAt == null && t.Id == taskId).FirstOrDefaultAsync();
        if (taskEntity == null)
            return Result<TaskDTO>.Error("Task does not exist.", ErrorCodes.TASK_NOT_FOUND, StatusCodes.Status404NotFound);

        bool hasAccess = groupEntity.OwnerUserId == _userRequestContext.UserId || groupEntity.GroupMembers.Any(m => m.UserId == _userRequestContext.UserId);

        if (!hasAccess)
            return Result<TaskDTO>.Error("User has no access to this task.", ErrorCodes.ACCESS_DENIED, StatusCodes.Status403Forbidden);
        return Result<TaskDTO>.Success(_mapper.Map<TaskDTO>(taskEntity));
    }

    /// <summary>
    ///     Updates a task with new details. Only the group owner can modify these details
    ///     Returns an error if the task details are the same
    ///         Also pushes a sync payload to all connected clients on success
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="groupId"></param>
    /// <param name="updatedTaskDTO"></param>
    /// <returns></returns>
    public async Task<Result<string>> UpdateTask(int taskId, int groupId, UpdateTaskDetailsDTO updatedTaskDTO)
    {

        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers.Where(m => m.KickedAt == null)).AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.OwnerUserId == _userRequestContext.UserId && g.DeletedAt == null);
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);


        TaskEntity? taskEntity = await _dbContext.Tasks.Include(t => t.AssignedTo).Where(t => t.DeletedAt == null && t.Id == taskId).FirstOrDefaultAsync();


        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", ErrorCodes.TASK_NOT_FOUND, StatusCodes.Status404NotFound);


        GroupAccess groupAccess = groupEntity.GetGroupAccess(_userRequestContext.UserId);
        if (groupAccess == GroupAccess.Shared)
        {
            return Result<string>.Error("Only group owners can update tasks.", ErrorCodes.SHARED_USER_CANNOT_PERFORM_ACTION, StatusCodes.Status403Forbidden);
        }
        else if (groupAccess == GroupAccess.Denied)
        {
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status403Forbidden);
        }

        if (updatedTaskDTO.Title == taskEntity.Title && updatedTaskDTO.Description == taskEntity.Description)
            return Result<string>.Error("Task details are the same.", errorCode: ErrorCodes.TASK_DETAILS_UNCHANGED);

        taskEntity.Title = updatedTaskDTO.Title ?? taskEntity.Title;
        taskEntity.Description = updatedTaskDTO.Description ?? taskEntity.Description;

        if (updatedTaskDTO.Title != null || updatedTaskDTO.Description != null)
            taskEntity.LastModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await _clientSyncService.PushPayloadToGroup(groupId, SyncPayload.FromEntity(Tasks: [taskEntity]));

        return Result<string>.Success("Task updated.");
    }
    /// <summary>
    ///     Deletes a task. Only the group owner can delete tasks
    ///     Also pushes a sync payload to all connected clients on success
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public async Task<Result<string>> DeleteTask(int taskId, int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers.Where(m => m.KickedAt == null)).AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.OwnerUserId == _userRequestContext.UserId && g.DeletedAt == null);
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);

        TaskEntity? taskEntity = await _dbContext.Tasks.Where(t => t.DeletedAt == null && t.Id == taskId).FirstOrDefaultAsync();

        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", ErrorCodes.TASK_NOT_FOUND, StatusCodes.Status404NotFound);


        GroupAccess groupAccess = groupEntity.GetGroupAccess(_userRequestContext.UserId);
        if (groupAccess == GroupAccess.Shared)
        {
            return Result<string>.Error("Only group owners can delete tasks.", ErrorCodes.SHARED_USER_CANNOT_PERFORM_ACTION, StatusCodes.Status403Forbidden);
        }
        else if (groupAccess == GroupAccess.Denied)
        {
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status403Forbidden);
        }

        taskEntity.DeletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        await _clientSyncService.PushPayloadToGroup(groupId, SyncPayload.FromEntity(DeletedTasksIds: [taskId]));

        return Result<string>.Success("Task deleted.");
    }

    /// <summary>
    ///     Assigns a task to a list of users using user ids. Only the group owner can assign tasks
    ///     Pushes a sync payload to all connected clients on success
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="groupId"></param>
    /// <param name="userIdsToAssign"></param>
    /// <returns></returns>
    public async Task<Result<string>> AssignTaskTo(int taskId, int groupId, int[] userIdsToAssign)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers.Where(m => m.KickedAt == null)).AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.OwnerUserId == _userRequestContext.UserId && g.DeletedAt == null);
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);



        TaskEntity? taskEntity = await _dbContext.Tasks.Include(t => t.AssignedTo).SingleOrDefaultAsync(t => t.Id == taskId && t.DeletedAt == null);

        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", ErrorCodes.TASK_NOT_FOUND, StatusCodes.Status404NotFound);



        GroupAccess groupAccess = groupEntity.GetGroupAccess(_userRequestContext.UserId);
        if (groupAccess == GroupAccess.Shared)
        {
            return Result<string>.Error("Only group owners can assign tasks.", ErrorCodes.SHARED_USER_CANNOT_PERFORM_ACTION, StatusCodes.Status403Forbidden);
        }
        else if (groupAccess == GroupAccess.Denied)
        {
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status403Forbidden);
        }


        var memberIds = await _dbContext.GroupMembers
            .Where(m => m.GroupId == groupId && m.KickedAt == null)
            .Select(m => m.UserId)
            .ToListAsync();

        HashSet<UserEntity> usersToBeAssigned =
            await _dbContext.Users
                    .Where(u => userIdsToAssign.Contains(u.Id) && memberIds.Contains(u.Id))
                    .ToHashSetAsync();

        taskEntity.AssignedTo.UnionWith(usersToBeAssigned);
        taskEntity.LastModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await _clientSyncService.PushPayloadToGroup(groupId, SyncPayload.FromEntity(Tasks: [taskEntity]));

        return Result<string>.Success("Users assigned.");
    }
    // Directly sets a list of users to be assigned to a task
    /// <summary>
    ///     Sets a list of users to be assigned to a task overriding previous assignments. Only the group owner can assign tasks
    ///     Pushes a sync payload to all connected clients on success
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="groupId"></param>
    /// <param name="assignedUsers"></param>
    /// <returns></returns>
    public async Task<Result<string>> SetAssignTaskToUsers(int taskId, int groupId, int[] assignedUsers)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers.Where(m => m.KickedAt == null)).AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.OwnerUserId == _userRequestContext.UserId && g.DeletedAt == null);
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);



        TaskEntity? taskEntity = await _dbContext.Tasks.Include(t => t.AssignedTo).SingleOrDefaultAsync(t => t.Id == taskId && t.DeletedAt == null);

        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", ErrorCodes.TASK_NOT_FOUND, StatusCodes.Status404NotFound);



        GroupAccess groupAccess = groupEntity.GetGroupAccess(_userRequestContext.UserId);
        if (groupAccess == GroupAccess.Shared)
        {
            return Result<string>.Error("Only group owners can assign tasks.", ErrorCodes.SHARED_USER_CANNOT_PERFORM_ACTION, StatusCodes.Status403Forbidden);
        }
        else if (groupAccess == GroupAccess.Denied)
        {
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status403Forbidden);
        }

        var memberIds = await _dbContext.GroupMembers
            .Where(m => m.GroupId == groupId && m.KickedAt == null)
            .Select(m => m.UserId)
            .ToListAsync();

        HashSet<UserEntity> usersToBeAssigned =
            await _dbContext.Users
                    .Where(u => assignedUsers.Contains(u.Id) && memberIds.Contains(u.Id))
                    .ToHashSetAsync();

        taskEntity.AssignedTo = usersToBeAssigned;
        taskEntity.LastModifiedDate = DateTime.UtcNow;


        await _dbContext.SaveChangesAsync();
        await _clientSyncService.PushPayloadToGroup(groupId, SyncPayload.FromEntity(Tasks: [taskEntity]));


        return Result<string>.Success("Users set assigned.");
    }

    /// <summary>
    ///     Marks a task for a user. Only the assigned users can mark tasks or the owner
    ///     Pushes a sync payload to all connected clients on success
    /// </summary>
    /// <param name="taskId"></param>
    /// <param name="groupId"></param>
    /// <param name="isCompleted"></param>
    /// <returns></returns>
    public async Task<Result<string>> MarkTaskForUser(int taskId, int groupId, bool isCompleted)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.GroupMembers.Where(m => m.KickedAt == null)).AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedAt == null && (g.OwnerUserId == _userRequestContext.UserId || g.GroupMembers.Any(m => m.UserId == _userRequestContext.UserId)));
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);

        TaskEntity? taskEntity = await _dbContext.Tasks.Include(t => t.AssignedTo).SingleOrDefaultAsync(t => t.Id == taskId && t.DeletedAt == null);

        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", ErrorCodes.TASK_NOT_FOUND, StatusCodes.Status404NotFound);


        if (groupEntity.OwnerUserId != _userRequestContext.UserId && taskEntity.AssignedTo.FirstOrDefault(u => u.Id == _userRequestContext.UserId) == null)
        {
            return Result<string>.Error("User is not assigned to this task.", ErrorCodes.USER_NOT_ASSIGNED_TO_TASK, StatusCodes.Status403Forbidden);
        }


        taskEntity.LastModifiedDate = DateTime.UtcNow;
        taskEntity.CompletedById = isCompleted ? _userRequestContext.UserId : null;


        await _dbContext.SaveChangesAsync();
        await _clientSyncService.PushPayloadToGroup(groupId, SyncPayload.FromEntity(Tasks: [taskEntity]));

        return Result<string>.Success("Task marked.");
    }

    /// <summary>
    ///     Creates a new task. Only the group owner can create tasks
    ///     Pushes a sync payload to all connected clients on success
    /// </summary>
    /// <param name="newTaskDTO"></param>
    /// <param name="groupId"></param>
    /// <returns></returns>
    public async Task<Result<TaskDTO>> CreateTask(CreateTaskDTO newTaskDTO, int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.OwnerUserId == _userRequestContext.UserId && g.DeletedAt == null);
        if (groupEntity == null)
            return Result<TaskDTO>.Error("Group does not exist.", ErrorCodes.GROUP_NOT_FOUND, StatusCodes.Status404NotFound);

        var validUserIds = await _dbContext.GroupMembers
            .Where(m => m.GroupId == groupId && m.KickedAt == null)
            .Select(m => m.UserId)
            .ToListAsync();

        HashSet<UserEntity> usersToBeAssigned =
            newTaskDTO.AssignedUserIds != null
                ? await _dbContext.Users
                    .Where(u => newTaskDTO.AssignedUserIds.Contains(u.Id) && validUserIds.Contains(u.Id))
                    .ToHashSetAsync()
                : new();

        TaskEntity createdTask = new() { Title = newTaskDTO.Title, Description = newTaskDTO.Description, CreationDate = DateTime.UtcNow, LastModifiedDate = DateTime.UtcNow, GroupId = groupId, AssignedTo = usersToBeAssigned };

        await _dbContext.Tasks.AddAsync(createdTask);
        await _dbContext.SaveChangesAsync();
        await _clientSyncService.PushPayloadToGroup(groupId, SyncPayload.FromEntity(Tasks: [createdTask]));


        return Result<TaskDTO>.Success(_mapper.Map<TaskDTO>(createdTask));
    }

}