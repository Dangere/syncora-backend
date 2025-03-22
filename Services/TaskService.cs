using AutoMapper;
using TaskManagementWebAPI.Models.Entities;
using TaskManagementWebAPI.Models.DTOs.Tasks;
using TaskManagementWebAPI.Data;
using Microsoft.EntityFrameworkCore;
using TaskManagementWebAPI.Utilities;

namespace TaskManagementWebAPI.Services;
public class TaskService(IMapper mapper, SyncoraDbContext dbContext)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;

    // public async Task<Result<List<TaskDTO>>> GetTasksForUser(int userId)
    // {
    //     List<TaskDTO> tasks = await _dbContext.Tasks.Include(t => t.SharedUsers).AsNoTracking().Where(t => t.OwnerUserId == userId).OrderBy(t => t.CreationDate).Select(t => _mapper.Map<TaskDTO>(t)).ToListAsync();

    //     return Result<List<TaskDTO>>.Success(tasks);
    // }

    // public async Task<Result<TaskDTO>> GetTaskForUser(int taskId, int userId)
    // {
    //     TaskEntity? taskEntity = await _dbContext.Tasks.FindAsync(taskId);
    //     if (taskEntity == null)
    //         return Result<TaskDTO>.Error("Task does not exist.", 404);

    //     bool hasAccess = taskEntity.OwnerUserId == userId || taskEntity.SharedUsers.Any(u => u.Id == userId);

    //     if (!hasAccess)
    //         return Result<TaskDTO>.Error("User has no access to this task.", 403);
    //     return Result<TaskDTO>.Success(_mapper.Map<TaskDTO>(taskEntity));
    // }

    // public async Task<Result<string>> UpdateTaskForUser(int taskId, int userId, UpdateTaskDTO updatedTaskDTO)
    // {

    //     TaskEntity? taskEntity = await _dbContext.Tasks.FindAsync(taskId);

    //     if (taskEntity == null)
    //         return Result<string>.Error("Task does not exist.", 404);

    //     bool isOwner = taskEntity.OwnerUserId == userId;
    //     bool isShared = taskEntity.SharedUsers.Any(u => u.Id == userId);
    //     if (!isOwner && isShared)
    //     {
    //         return Result<string>.Error("A shared user can't update the details of a task they don't own", 403);
    //     }
    //     else if (!isOwner && !isShared)
    //         return Result<string>.Error("User has no access to this task.", 403);

    //     return await UpdateTaskEntity(taskEntity, updatedTaskDTO);
    // }

    // public async Task<Result<string>> AllowAccessToTask(int taskId, int userId, string userNameToGrant, bool allowAccess)
    // {
    //     TaskEntity? taskEntity = await _dbContext.Tasks.Include(t => t.SharedUsers).FirstOrDefaultAsync(t => t.Id == taskId);

    //     if (taskEntity == null)
    //         return Result<string>.Error("Task does not exist.", 404);

    //     UserEntity? userToGrant = await _dbContext.Users.FirstOrDefaultAsync(u => EF.Functions.ILike(u.UserName, userNameToGrant));

    //     if (userToGrant == null)
    //         return Result<string>.Error("User does not exist.", 404);


    //     if (allowAccess == taskEntity.SharedUsers.Any(u => u.Id == userToGrant.Id))
    //         return Result<string>.Error($"The user has already been " + (allowAccess ? "granted" : "revoked") + " access.", 403);


    //     bool isOwner = taskEntity.OwnerUserId == userId;
    //     bool isShared = taskEntity.SharedUsers.Any(u => u.Id == userId);
    //     if (!isOwner && isShared)
    //     {
    //         return Result<string>.Error("A shared user can't " + (allowAccess ? "grant" : "revoke") + " access to a task they don't own", 403);
    //     }
    //     else if (!isOwner && !isShared)
    //         return Result<string>.Error("User has no access to this task.", 403);

    //     if (allowAccess)
    //         taskEntity.SharedUsers.Add(userToGrant);
    //     else
    //         taskEntity.SharedUsers.Remove(userToGrant);

    //     await _dbContext.SaveChangesAsync();

    //     return Result<string>.Success(allowAccess ? "Access granted." : "Access revoked.");

    // }
    // public async Task<Result<List<TaskDTO>>> GetAllTaskDTOs()
    // {
    //     //this will load all entities into memory just to filter through them (bad approach)
    //     // await _dbContext.Tasks.ForEachAsync(t => taskDTOs.Add(_mapper.Map<TaskDTO>(t)));


    //     //this will run a `SELECT` sql query where it uses the TaskDTO properties as the selected columns

    //     List<TaskDTO> tasks = await _dbContext.Tasks.Include(t => t.SharedUsers).AsNoTracking().OrderBy(t => t.CreationDate).Select(t => _mapper.Map<TaskDTO>(t)).ToListAsync();

    //     return Result<List<TaskDTO>>.Success(tasks);
    // }

    // public async Task<Result<TaskDTO>> GetTaskDTO(int id)
    // {
    //     TaskEntity? taskEntity = await _dbContext.Tasks.FindAsync(id);
    //     if (taskEntity == null)
    //         return Result<TaskDTO>.Error("Task does not exist.", 404);

    //     return Result<TaskDTO>.Success(_mapper.Map<TaskDTO>(taskEntity));
    // }

    // public async Task<Result<TaskDTO>> CreateTask(CreateTaskDTO newTaskDTO, int userId)
    // {
    //     // Make sure the user exists
    //     if (await _dbContext.Users.FindAsync(userId) == null)
    //         return Result<TaskDTO>.Error("User does not exist.", 404);

    //     TaskEntity createdTask = new() { Title = newTaskDTO.Title, Description = newTaskDTO.Description, CreationDate = DateTime.UtcNow, OwnerUserId = userId };

    //     await _dbContext.Tasks.AddAsync(createdTask);
    //     await _dbContext.SaveChangesAsync();

    //     return Result<TaskDTO>.Success(_mapper.Map<TaskDTO>(createdTask));
    // }
    public async Task<Result<string>> UpdateTask(int id, UpdateTaskDTO updatedTaskDTO)
    {

        TaskEntity? taskEntity = await _dbContext.Tasks.FindAsync(id);

        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", 404);

        return await UpdateTaskEntity(taskEntity, updatedTaskDTO);
    }

    public async Task<Result<string>> RemoveTask(int id)
    {
        TaskEntity? taskEntity = await _dbContext.Tasks.FindAsync(id);

        if (taskEntity == null)
            return Result<string>.Error("Task does not exist.", 404);



        _dbContext.Tasks.Remove(taskEntity);
        await _dbContext.SaveChangesAsync();

        return Result<string>.Success("Task deleted.");
    }

    private async Task<Result<string>> UpdateTaskEntity(TaskEntity taskEntity, UpdateTaskDTO updatedTaskDTO)
    {
        taskEntity.Title = updatedTaskDTO.NewTitle ?? taskEntity.Title;
        taskEntity.Description = updatedTaskDTO.NewDescription ?? taskEntity.Description;

        if (updatedTaskDTO.NewTitle != null || updatedTaskDTO.NewDescription != null)
            taskEntity.LastUpdateDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Result<string>.Success("Task updated.");
    }
}