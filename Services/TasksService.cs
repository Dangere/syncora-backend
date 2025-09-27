using AutoMapper;
using SyncoraBackend.Models.Entities;
using SyncoraBackend.Models.DTOs.Tasks;
using SyncoraBackend.Data;
using Microsoft.EntityFrameworkCore;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Services;

public class TasksService(IMapper mapper, SyncoraDbContext dbContext, ClientSyncService clientSyncService)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;

    private readonly ClientSyncService _clientSyncService = clientSyncService;

    public async Task<Result<List<TaskDTO>>> GetTasksForUser(int userId, int groupId, DateTime? sinceUtc = null)
    {

        GroupEntity? groupEntity = await _dbContext.Groups.Include(g => g.Tasks).Include(g => g.GroupMembers).ThenInclude(m => m.User).SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedDate == null);
        if (groupEntity == null)
            return Result<List<TaskDTO>>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        if (!(groupEntity.OwnerUserId == userId || groupEntity.GroupMembers.Any(m => m.UserId == userId)))
        {
            return Result<List<TaskDTO>>.Error("User has no access to this group.", StatusCodes.Status403Forbidden);

        }

        List<TaskDTO> tasksDTO;
        if (sinceUtc != null)
        {
            tasksDTO = groupEntity.Tasks.OrderBy(t => t.CreationDate).Where(t => t.LastModifiedDate > sinceUtc).Select(t => _mapper.Map<TaskDTO>(t)).ToList();
        }
        else
        {
            tasksDTO = groupEntity.Tasks.OrderBy(t => t.CreationDate).Select(t => _mapper.Map<TaskDTO>(t)).ToList();

        }



        return Result<List<TaskDTO>>.Success(tasksDTO);
    }

    public async Task<Result<TaskDTO>> GetTaskForUser(int taskId, int userId, int groupId)
    {

        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedDate == null);
        if (groupEntity == null)
            return Result<TaskDTO>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        TaskEntity? taskEntity = await _dbContext.Tasks.FindAsync(taskId);
        if (taskEntity == null)
            return Result<TaskDTO>.Error("Task does not exist.", StatusCodes.Status404NotFound);

        bool hasAccess = groupEntity.OwnerUserId == userId || groupEntity.GroupMembers.Any(m => m.UserId == userId);

        if (!hasAccess)
            return Result<TaskDTO>.Error("User has no access to this task.", StatusCodes.Status403Forbidden);
        return Result<TaskDTO>.Success(_mapper.Map<TaskDTO>(taskEntity));
    }

    // TODO: Change it so group members can complete tasks using this method or make a new method designed for that 
    public async Task<Result<string>> UpdateTaskForUser(int taskId, int groupId, int userId, UpdateTaskDTO updatedTaskDTO)
    {

        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.OwnerUserId == userId && g.DeletedDate == null);
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);


        TaskEntity? taskEntity = await _dbContext.Tasks.FindAsync(taskId);

        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", StatusCodes.Status404NotFound);

        if (groupEntity.OwnerUserId != userId)
            return Result<string>.Error("Only group owners can update tasks.", StatusCodes.Status403Forbidden);

        return await UpdateTaskEntity(taskEntity, updatedTaskDTO, userId);
    }

    public async Task<Result<string>> DeleteTaskForUser(int taskId, int groupId, int userId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.OwnerUserId == userId && g.DeletedDate == null);
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);


        TaskEntity? taskEntity = await _dbContext.Tasks.FindAsync(taskId);

        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", StatusCodes.Status404NotFound);


        if (groupEntity.OwnerUserId != userId)
            return Result<string>.Error("Only group owners can delete tasks.", StatusCodes.Status403Forbidden);


        // TODO: Store deleted tasks to return in the response for syncing with client
        _dbContext.Tasks.Remove(taskEntity);
        await _dbContext.SaveChangesAsync();
        await _clientSyncService.NotifyGroupMembersToSync(groupId);

        return Result<string>.Success("Task deleted.");
    }

    public async Task<Result<string>> AssignTaskToUsers(int taskId, int groupId, int userId, int[] userIdsToAssign)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.OwnerUserId == userId && g.DeletedDate == null);
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);



        TaskEntity? taskEntity = await _dbContext.Tasks.Include(t => t.AssignedTo).SingleOrDefaultAsync(t => t.Id == taskId);

        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", StatusCodes.Status404NotFound);



        if (groupEntity.OwnerUserId != userId)
            return Result<string>.Error("Only group owners can update tasks.", StatusCodes.Status403Forbidden);

        var validUserIds = await _dbContext.GroupMembers
            .Where(m => m.GroupId == groupId && m.KickedAt == null)
            .Select(m => m.UserId)
            .ToListAsync();

        HashSet<UserEntity> usersToBeAssigned =
            await _dbContext.Users
                    .Where(u => userIdsToAssign.Contains(u.Id) && validUserIds.Contains(u.Id))
                    .ToHashSetAsync();

        taskEntity.AssignedTo.UnionWith(usersToBeAssigned);


        await _dbContext.SaveChangesAsync();
        await _clientSyncService.NotifyGroupMembersToSync(groupId);

        return Result<string>.Success("Users assigned.");
    }

    public async Task<Result<TaskDTO>> CreateTaskForUser(CreateTaskDTO newTaskDTO, int userId, int groupId)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.OwnerUserId == userId && g.DeletedDate == null);
        if (groupEntity == null)
            return Result<TaskDTO>.Error("Group does not exist.", StatusCodes.Status404NotFound);

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
        await _clientSyncService.NotifyGroupMembersToSync(groupId);

        return Result<TaskDTO>.Success(_mapper.Map<TaskDTO>(createdTask));
    }
    public async Task<Result<string>> UpdateTask(int id, int groupId, UpdateTaskDTO updatedTaskDTO)
    {
        GroupEntity? groupEntity = await _dbContext.Groups.AsNoTracking().SingleOrDefaultAsync(g => g.Id == groupId && g.DeletedDate == null);
        if (groupEntity == null)
            return Result<string>.Error("Group does not exist.", StatusCodes.Status404NotFound);

        TaskEntity? taskEntity = await _dbContext.Tasks.FindAsync(id);

        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", StatusCodes.Status404NotFound);

        return await UpdateTaskEntity(taskEntity, updatedTaskDTO);
    }

    public async Task<Result<string>> RemoveTask(int id)
    {
        TaskEntity? taskEntity = await _dbContext.Tasks.FindAsync(id);

        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", StatusCodes.Status404NotFound);


        // TODO: Store deleted tasks to return in the response for syncing with client
        _dbContext.Tasks.Remove(taskEntity);
        await _dbContext.SaveChangesAsync();
        await _clientSyncService.NotifyGroupMembersToSync(taskEntity.GroupId);

        return Result<string>.Success("Task deleted.");
    }

    private async Task<Result<string>> UpdateTaskEntity(TaskEntity taskEntity, UpdateTaskDTO updatedTaskDTO, int? userId = null)
    {
        taskEntity.Title = updatedTaskDTO.Title ?? taskEntity.Title;
        taskEntity.Description = updatedTaskDTO.Description ?? taskEntity.Description;

        // if (updatedTaskDTO.Completed != null)
        // {
        //     // Im here realizing that this is not the best way to do this
        //     // We are basically checking twice if the task is completed as a bool and as a user id when we can only use the user id
        //     // taskEntity.Completed = updatedTaskDTO.Completed ?? taskEntity.Completed;
        //     taskEntity.CompletedById = userId;
        // }



        if (updatedTaskDTO.Title != null || updatedTaskDTO.Description != null)
            taskEntity.LastModifiedDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        await _clientSyncService.NotifyGroupMembersToSync(taskEntity.GroupId);

        return Result<string>.Success("Task updated.");
    }
}