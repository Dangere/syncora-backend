using Microsoft.VisualBasic;
using TaskManagementWebAPI.Models.DTOs;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<TaskDTO> tasks =
[
 new TaskDTO(1, "Task 1", "Description 1", true, DateTime.Parse("2023-01-01"), DateTime.Parse("2023-01-02")),
 new TaskDTO(2, "Task 2", "Description 2", false, DateTime.Parse("2023-01-01"), DateTime.Parse("2023-01-02")),
 new TaskDTO(3, "Task 3", "Description 3", false, DateTime.Parse("2023-01-01"), DateTime.Parse("2023-01-02")),
 new TaskDTO(4, "Task 4", "Description 4", true, DateTime.Parse("2023-01-01"), DateTime.Parse("2023-01-02")),
];

app.MapGet("/", () => "Hello World!");

app.MapGet("/tasks", () => tasks);


app.Run();
