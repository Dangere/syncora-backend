using AutoMapper;
using TaskManagementWebAPI.Models.DTOs.Entities;
using TaskManagementWebAPI.Models.DTOs.Tasks;

var builder = WebApplication.CreateBuilder(args);

//Register AutoMapper
builder.Services.AddAutoMapper(typeof(Program));
string GetTaskEndpointName = "GetTask";

var app = builder.Build();


//Mimicking database rows
List<TaskEntity> tasks =
[
 new TaskEntity(1,1002, "Task 1", "Description 1", true, DateTime.Parse("2023-01-01"), null),
 new TaskEntity(2,1002, "Task 2", "Description 2", false, DateTime.Parse("2023-01-01"), null),
 new TaskEntity(3,1000, "Task 3", "Description 3", false, DateTime.Parse("2023-01-01"), null),
 new TaskEntity(4,1000, "Task 4", "Description 4", true, DateTime.Parse("2023-01-01"), DateTime.Parse("2023-01-02")),
];

app.MapGet("/", () => "Hello World!");

//GET /tasks
app.MapGet("/tasks", (IMapper mapper) =>
{
    List<TaskDTO> taskDTOs = [];
    foreach (TaskEntity task in tasks)
    {
        taskDTOs.Add(mapper.Map<TaskDTO>(task));
    }

    return Results.Ok(taskDTOs);

});

//GET /tasks/1
app.MapGet("/tasks/{id}", (int id) =>
{
    return tasks.Where(task => task.Id == id);
}).WithName(GetTaskEndpointName);

//POST /tasks
app.MapPost("/tasks", (CreateTaskDTO newTask) =>
{
    int newId = tasks.Count;
    TaskEntity createdTask = new(newId, newTask.UserId, newTask.Title, newTask.Description, false, DateTime.Now, null);
    tasks.Add(createdTask);

    return Results.CreatedAtRoute(GetTaskEndpointName, new { id = createdTask.Id }, createdTask);
});

//PUT /tasks/1
app.MapPut("/tasks/{id}", (int id, UpdateTaskDTO updatedTaskDTO) =>
{
    if (tasks.Count < id)
        return Results.NotFound();

    TaskEntity currentTask = tasks[id - 1];
    TaskEntity updatedTask = currentTask with { Title = updatedTaskDTO.NewTitle ?? currentTask.Title, Description = updatedTaskDTO.NewDescription ?? currentTask.Description, Completed = updatedTaskDTO.Completed ?? currentTask.Completed, };

    if (updatedTaskDTO.Completed != null || updatedTaskDTO.NewTitle != null || updatedTaskDTO.NewDescription != null)
        updatedTask = updatedTask with { UpdatedAt = DateTime.Now };


    tasks[id - 1] = updatedTask;
    return Results.Ok(updatedTask);
});

//DELETE /tasks/1
app.MapDelete("/tasks/{id}", (int id) =>
{
    if (tasks.Count < id)
        return Results.NotFound();

    tasks.RemoveAt(id - 1);
    return Results.Ok();
});



app.Run();
