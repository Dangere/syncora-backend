using System.Security.Claims;
using LibraryManagementSystem.Utilities;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Attributes;
using TaskManagementWebAPI.Enums;
using TaskManagementWebAPI.Models.DTOs.Tasks;
using TaskManagementWebAPI.Services;


namespace TaskManagementWebAPI.Controllers;

[AuthorizeRoles(UserRole.Admin, UserRole.User)]
[ApiController]
[Route("api/[controller]")]
public class UserController(TaskService taskService) : ControllerBase
{
    private readonly TaskService _taskService = taskService;
    private const string _getTaskEndpointName = "GetTaskForUser";


    [HttpGet("tasks")]
    public async Task<IActionResult> GetAllOwnedTasks()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<List<TaskDTO>> fetchingTasksResult = await _taskService.GetTasksForUser(userId);


        if (!fetchingTasksResult.IsSuccess)
            return BadRequest(fetchingTasksResult.ErrorMessage);

        return Ok(fetchingTasksResult.Data);
    }

    [HttpGet("tasks/{taskId}", Name = _getTaskEndpointName)]
    public async Task<IActionResult> GetTask(int taskId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<TaskDTO> fetchingTaskResult = await _taskService.GetTaskForUser(taskId, userId);

        if (!fetchingTaskResult.IsSuccess)
            return Unauthorized(fetchingTaskResult.ErrorMessage);

        return Ok(fetchingTaskResult.Data);
    }


    [HttpPost("tasks")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDTO newTask)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<TaskDTO> createdTaskResult = await _taskService.CreateTask(newTask, userId);


        if (!createdTaskResult.IsSuccess)
            return BadRequest(createdTaskResult.ErrorMessage);

        return CreatedAtRoute(_getTaskEndpointName, new { id = createdTaskResult.Data!.Id }, createdTaskResult.Data!);
    }

    [HttpPut("tasks/{taskId}")]
    public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateTaskDTO updateTask)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> updatedTaskResult = await _taskService.UpdateTaskForUser(taskId, userId, updateTask);


        if (!updatedTaskResult.IsSuccess)
            return BadRequest(updatedTaskResult.ErrorMessage);

        return Ok(updatedTaskResult.Data);
    }

}