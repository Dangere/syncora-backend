using SyncoraBackend.Utilities;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Tasks;
using SyncoraBackend.Services;
using System.Security.Claims;

namespace SyncoraBackend.Controllers;

[AuthorizeRoles(UserRoles.User)]
[ApiController]
[Route("api/groups/{groupId}/[controller]")]
public class TasksController(TasksService taskService) : ControllerBase
{
    private readonly TasksService _taskService = taskService;

    private const string _getTaskEndpointName = "GetTask";


    [HttpGet]
    public async Task<IActionResult> GetTasks(int groupId)
    {

        Result<List<TaskDTO>> tasksFetchResult = await _taskService.GetTasks(groupId);

        if (!tasksFetchResult.IsSuccess)
            return this.ErrorResponse(tasksFetchResult);


        return Ok(tasksFetchResult.Data!);
    }


    [HttpGet("{taskId}", Name = _getTaskEndpointName)]
    public async Task<IActionResult> GetTask(int taskId, int groupId)
    {

        Result<TaskDTO> fetchResult = await _taskService.GetTask(taskId, groupId);

        if (!fetchResult.IsSuccess)
            return this.ErrorResponse(fetchResult);


        return Ok(fetchResult.Data);
    }

    [HttpPost()]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDTO newTaskDTO, int groupId)
    {

        Result<TaskDTO> createdResult = await _taskService.CreateTask(newTaskDTO, groupId);

        if (!createdResult.IsSuccess)
            return this.ErrorResponse(createdResult);


        return CreatedAtRoute(_getTaskEndpointName, new { taskId = createdResult.Data!.Id, groupId }, createdResult.Data!);
    }

    [HttpPut("{taskId}")]
    public async Task<IActionResult> UpdateTask([FromBody] UpdateTaskDetailsDTO updatedTaskDTO, int taskId, int groupId)
    {

        Result<string> updateResult = await _taskService.UpdateTask(taskId, groupId, updatedTaskDTO);

        if (!updateResult.IsSuccess)
            return this.ErrorResponse(updateResult);


        return NoContent();
    }


    [HttpPut("{taskId}/assign")]
    // ENDPOINT: /api/groups/{groupId}/tasks/{taskId}/assign?ids=1&ids=2&ids=3
    public async Task<IActionResult> AssignTask([FromQuery] int[] ids, int taskId, int groupId)
    {

        Result<string> assignResults = await _taskService.AssignTaskTo(taskId, groupId, ids);

        if (!assignResults.IsSuccess)
            return this.ErrorResponse(assignResults);


        return NoContent();
    }


    [HttpPut("{taskId}/set-assign")]
    // ENDPOINT: /api/groups/{groupId}/tasks/{taskId}/set-assign?ids=1&ids=2&ids=3
    public async Task<IActionResult> SetAssignTask([FromQuery] int[] ids, int taskId, int groupId)
    {

        Result<string> assignResults = await _taskService.SetAssignTaskToUsers(taskId, groupId, ids);

        if (!assignResults.IsSuccess)
            return this.ErrorResponse(assignResults);


        return NoContent();
    }
    [HttpPut("{taskId}/mark")]
    // ENDPOINT: /api/groups/{groupId}/tasks/{taskId}/mark?isDone=true
    public async Task<IActionResult> MarkTask(int taskId, int groupId, [FromQuery] bool isDone = true)
    {

        Result<string> updateResult = await _taskService.MarkTaskForUser(taskId, groupId, isDone);

        if (!updateResult.IsSuccess)
            return this.ErrorResponse(updateResult);


        return NoContent();
    }

    [HttpDelete("{taskId}")]
    public async Task<IActionResult> DeleteTask(int taskId, int groupId)
    {

        Result<string> deleteResult = await _taskService.DeleteTask(taskId, groupId);

        if (!deleteResult.IsSuccess)
            return this.ErrorResponse(deleteResult);


        return NoContent();
    }
}