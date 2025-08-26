using SyncoraBackend.Utilities;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Tasks;
using SyncoraBackend.Services;
using System.Security.Claims;

namespace SyncoraBackend.Controllers;

[AuthorizeRoles(UserRole.User)]
[ApiController]
[Route("api/groups/{groupId}/[controller]")]
public class TasksController(TasksService taskService) : ControllerBase
{
    private readonly TasksService _taskService = taskService;

    private const string _getTaskEndpointName = "GetTask";


    [HttpGet]
    public async Task<IActionResult> GetTasks(int groupId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<List<TaskDTO>> tasksFetchResult = await _taskService.GetTasksForUser(userId, groupId);

        if (!tasksFetchResult.IsSuccess)
            return StatusCode(tasksFetchResult.ErrorStatusCode, tasksFetchResult.ErrorMessage);

        return Ok(tasksFetchResult.Data!);
    }


    [HttpGet("{taskId}", Name = _getTaskEndpointName)]
    public async Task<IActionResult> GetTask(int taskId, int groupId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<TaskDTO> fetchResult = await _taskService.GetTaskForUser(taskId, userId, groupId);

        if (!fetchResult.IsSuccess)
            return StatusCode(fetchResult.ErrorStatusCode, fetchResult.ErrorMessage);

        return Ok(fetchResult.Data);
    }

    [HttpPost()]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDTO newTaskDTO, int groupId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<TaskDTO> createdResult = await _taskService.CreateTaskForUser(newTaskDTO, userId, groupId);

        if (!createdResult.IsSuccess)
            return StatusCode(createdResult.ErrorStatusCode, createdResult.ErrorMessage);

        return CreatedAtRoute(_getTaskEndpointName, new { taskId = createdResult.Data!.Id, groupId }, createdResult.Data!);
    }

    [HttpPut("{taskId}")]
    public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskDTO updatedTaskDTO, int taskId, int groupId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> updateResult = await _taskService.UpdateTaskForUser(taskId, groupId, userId, updatedTaskDTO);

        if (!updateResult.IsSuccess)
            return StatusCode(updateResult.ErrorStatusCode, updateResult.ErrorMessage);

        return NoContent();
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(int taskId, int groupId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> deleteResult = await _taskService.DeleteTaskForUser(taskId, groupId, userId);

        if (!deleteResult.IsSuccess)
            return StatusCode(deleteResult.ErrorStatusCode, deleteResult.ErrorMessage);

        return NoContent();
    }
}