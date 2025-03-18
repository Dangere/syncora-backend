using AutoMapper;
using TaskManagementWebAPI.Models.Entities;
using TaskManagementWebAPI.Models.DTOs.Tasks;
using TaskManagementWebAPI.Data;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Utilities;

namespace TaskManagementWebAPI.Services;
public class TaskService(IMapper mapper, SyncoraDbContext dbContext)
{
    private readonly IMapper _mapper = mapper;
    private readonly SyncoraDbContext _dbContext = dbContext;

    public async Task<Result<List<TaskDTO>>> GetTasksForUser(int userId)
    {
        List<TaskDTO> tasks = await _dbContext.Tasks.AsNoTracking().Where(t => t.OwnerUserId == userId).OrderBy(t => t.CreationDate).Select(t => _mapper.Map<TaskDTO>(t)).ToListAsync();

        return Result<List<TaskDTO>>.Success(tasks);
    }

    public async Task<Result<List<TaskDTO>>> GetAllTaskDTOs()
    {
        //this will load all entities into memory just to filter through them (bad approach)
        // await _dbContext.Tasks.ForEachAsync(t => taskDTOs.Add(_mapper.Map<TaskDTO>(t)));


        //this will run a `SELECT` sql query where it uses the TaskDTO properties as the selected columns

        List<TaskDTO> tasks = await _dbContext.Tasks.AsNoTracking().OrderBy(t => t.CreationDate).Select(t => _mapper.Map<TaskDTO>(t)).ToListAsync();

        return Result<List<TaskDTO>>.Success(tasks);
    }

    public async Task<Result<TaskDTO>> GetTaskDTO(int id)
    {
        TaskEntity? taskEntity = await _dbContext.Tasks.FindAsync(id);
        if (taskEntity == null)
            return Result<TaskDTO>.Error("Task does not exist.");

        return Result<TaskDTO>.Success(_mapper.Map<TaskDTO>(taskEntity));
    }

    public async Task<Result<TaskDTO>> CreateTask(CreateTaskDTO newTaskDTO, int userId)
    {
        // Make sure the user exists
        if (await _dbContext.Users.FindAsync(userId) == null)
            return Result<TaskDTO>.Error("User does not exist.");

        TaskEntity createdTask = new() { Title = newTaskDTO.Title, Description = newTaskDTO.Description, CreationDate = DateTime.UtcNow, OwnerUserId = userId };

        await _dbContext.Tasks.AddAsync(createdTask);
        await _dbContext.SaveChangesAsync();

        return Result<TaskDTO>.Success(_mapper.Map<TaskDTO>(createdTask));
    }
    public async Task<Result<string>> UpdateTaskAsync(int id, UpdateTaskDTO updatedTaskDTO)
    {

        TaskEntity? task = await _dbContext.Tasks.FindAsync(id);

        if (task == null)
            return Result<string>.Error("Task does not exist.");

        task.Title = updatedTaskDTO.NewTitle ?? task.Title;
        task.Description = updatedTaskDTO.NewDescription ?? task.Description;

        if (updatedTaskDTO.NewTitle != null || updatedTaskDTO.NewDescription != null)
            task.LastUpdateDate = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Result<string>.Success("Task updated.");
    }

    public async Task<Result<string>> RemoveTask(int id)
    {
        TaskEntity? task = await _dbContext.Tasks.FindAsync(id);

        if (task == null)
            return Result<string>.Error("Task does not exist.");



        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync();

        return Result<string>.Success("Task deleted.");
    }
}