using AutoMapper;
using TaskManagementWebAPI.Models.Entities;
using TaskManagementWebAPI.Models.DTOs.Tasks;

namespace TaskManagementWebAPI.Services;
public class TaskServices(IMapper mapper)
{
    private readonly IMapper _mapper = mapper;

    public List<TaskEntity> Tasks = [
        // new TaskEntity(1,1002, "Task 1", "Description 1", true, DateTime.Parse("2023-01-01"), null),
        // new TaskEntity(2,1002, "Task 2", "Description 2", false, DateTime.Parse("2023-01-01"), null),
        // new TaskEntity(3,1000, "Task 3", "Description 3", false, DateTime.Parse("2023-01-01"), null),
        // new TaskEntity(4,1000, "Task 4", "Description 4", true, DateTime.Parse("2023-01-01"), DateTime.Parse("2023-01-02")),
    ];

    public List<TaskDTO> GetTaskDTOs()
    {
        List<TaskDTO> taskDTOs = [];
        foreach (TaskEntity task in Tasks)
        {
            taskDTOs.Add(_mapper.Map<TaskDTO>(task));
        }

        return taskDTOs;
    }

    public TaskDTO? GetTaskDTO(int id)
    {
        TaskEntity? taskEntity = GetTaskEntity(id);
        if (taskEntity == null)
            return null;

        return _mapper.Map<TaskDTO>(taskEntity);
    }

    public TaskEntity? GetTaskEntity(int id)
    {

        return Tasks.Where(task => task.Id == id).FirstOrDefault();
    }

    public TaskDTO CreateTask(CreateTaskDTO newTaskDTO)
    {
        // int newId = NewGeneratedId();
        // TaskEntity createdTask = new(newId, newTaskDTO.UserId, newTaskDTO.Title, newTaskDTO.Description, false, DateTime.Now, null);
        // Tasks.Add(createdTask);

        // return _mapper.Map<TaskDTO>(createdTask);

        throw new NotImplementedException();
    }
    public bool UpdateTask(int id, UpdateTaskDTO updatedTaskDTO)
    {

        // TaskEntity? task = GetTaskEntity(id);
        // if (task == null)
        //     return false;

        // TaskEntity updatedTask = task with { Title = updatedTaskDTO.NewTitle ?? task.Title, Description = updatedTaskDTO.NewDescription ?? task.Description, Completed = updatedTaskDTO.Completed ?? task.Completed, };

        // if (updatedTaskDTO.Completed != null || updatedTaskDTO.NewTitle != null || updatedTaskDTO.NewDescription != null)
        //     updatedTask = updatedTask with { UpdatedAt = DateTime.Now };

        // int taskIndex = Tasks.IndexOf(task);
        // Tasks[taskIndex] = updatedTask;

        // return true;
        throw new NotImplementedException();
    }

    public bool RemoveTask(int id)
    {

        int taskIndex = Tasks.RemoveAll(task => task.Id == id);

        return true;

    }

    public int NewGeneratedId()
    {
        return Tasks.Count;
    }

}