using System.Security.Claims;
using TaskManagementWebAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Attributes;
using TaskManagementWebAPI.Enums;
using TaskManagementWebAPI.Models.DTOs.Tasks;
using TaskManagementWebAPI.Services;


namespace TaskManagementWebAPI.Controllers;

[AuthorizeRoles(UserRole.Admin, UserRole.User)]
[ApiController]
[Route("api/[controller]")]
public class UsersController(TaskService taskService) : ControllerBase
{
    private readonly TaskService _taskService = taskService;
    private const string _getTaskEndpointName = "GetTaskForUser";


    // [HttpGet("tasks")]
    // public async Task<IActionResult> GetAllOwnedTasks()
    // {
    //     int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    //     Result<List<TaskDTO>> fetchingTasksResult = await _taskService.GetTasksForUser(userId);


    //     if (!fetchingTasksResult.IsSuccess)
    //         return StatusCode(fetchingTasksResult.ErrorStatusCode, fetchingTasksResult.ErrorMessage);

    //     return Ok(fetchingTasksResult.Data);
    // }

    // [HttpGet("tasks/{taskId}", Name = _getTaskEndpointName)]
    // public async Task<IActionResult> GetTask(int taskId)
    // {
    //     int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    //     Result<TaskDTO> fetchingTaskResult = await _taskService.GetTaskForUser(taskId, userId);

    //     if (!fetchingTaskResult.IsSuccess)
    //         return StatusCode(fetchingTaskResult.ErrorStatusCode, fetchingTaskResult.ErrorMessage);

    //     return Ok(fetchingTaskResult.Data);
    // }


    // [HttpPost("tasks")]
    // public async Task<IActionResult> CreateTask([FromBody] CreateTaskDTO newTask)
    // {
    //     int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    //     Result<TaskDTO> createdTaskResult = await _taskService.CreateTask(newTask, userId);


    //     if (!createdTaskResult.IsSuccess)
    //         return StatusCode(createdTaskResult.ErrorStatusCode, createdTaskResult.ErrorMessage);

    //     return CreatedAtRoute(_getTaskEndpointName, new { taskId = createdTaskResult.Data!.Id }, createdTaskResult.Data!);
    // }

    // [HttpPut("tasks/{taskId}")]
    // public async Task<IActionResult> UpdateTask(int taskId, [FromBody] UpdateTaskDTO updateTask)
    // {
    //     int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    //     Result<string> updatedTaskResult = await _taskService.UpdateTaskForUser(taskId, userId, updateTask);


    //     if (!updatedTaskResult.IsSuccess)
    //         return StatusCode(updatedTaskResult.ErrorStatusCode, updatedTaskResult.ErrorMessage);

    //     return Ok(updatedTaskResult.Data);
    // }

    // [HttpPost("tasks/{taskId}/grant-access/{userName}")]
    // public async Task<IActionResult> GrantAccessToTask(int taskId, string userName)
    // {
    //     int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    //     Result<string> grantResult = await _taskService.AllowAccessToTask(taskId, userId, userName, true);

    //     if (!grantResult.IsSuccess)
    //         return StatusCode(grantResult.ErrorStatusCode, grantResult.ErrorMessage);

    //     return Ok(grantResult.Data);

    // }

    // [HttpPost("tasks/{taskId}/revoke-access/{userName}")]
    // public async Task<IActionResult> RevokeAccessToTask(int taskId, string userName)
    // {
    //     int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    //     Result<string> revokeResult = await _taskService.AllowAccessToTask(taskId, userId, userName, false);

    //     if (!revokeResult.IsSuccess)
    //         return StatusCode(revokeResult.ErrorStatusCode, revokeResult.ErrorMessage);

    //     return Ok(revokeResult.Data);

    // }
}