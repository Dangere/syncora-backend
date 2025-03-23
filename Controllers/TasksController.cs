using TaskManagementWebAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Attributes;
using TaskManagementWebAPI.Enums;
using TaskManagementWebAPI.Models.DTOs.Tasks;
using TaskManagementWebAPI.Services;

namespace TaskManagementWebAPI.Controllers;

[AuthorizeRoles(UserRole.Admin)]
[ApiController]
[Route("api/[controller]")]
public class TasksController(TaskService taskService) : ControllerBase
{
    private readonly TaskService _taskService = taskService;

    private const string _getTaskEndpointName = "GetTask";


    // //GET /tasks
    // [HttpGet]
    // public async Task<IActionResult> GetAllTasks()
    // {
    //     Result<List<TaskDTO>> tasksFetchResult = await _taskService.GetAllTaskDTOs();

    //     if (!tasksFetchResult.IsSuccess)
    //         return StatusCode(tasksFetchResult.ErrorStatusCode, tasksFetchResult.ErrorMessage);

    //     return Ok(tasksFetchResult.Data!);
    // }

    // //GET /tasks/1
    // [HttpGet("{id}", Name = _getTaskEndpointName)]
    // public async Task<IActionResult> GetTask(int id)
    // {
    //     Result<TaskDTO> taskFetchResult = await _taskService.GetTaskDTO(id);

    //     if (!taskFetchResult.IsSuccess)
    //         return StatusCode(taskFetchResult.ErrorStatusCode, taskFetchResult.ErrorMessage);

    //     return Ok(taskFetchResult.Data);
    // }

    // //POST /tasks
    // [HttpPost("{userId}/tasks")]
    // public async Task<IActionResult> PostTask([FromRoute] int userId, [FromBody] CreateTaskDTO newTask)
    // {
    //     Result<TaskDTO> createdTaskResult = await _taskService.CreateTask(newTask, userId);

    //     if (!createdTaskResult.IsSuccess)
    //         return StatusCode(createdTaskResult.ErrorStatusCode, createdTaskResult.ErrorMessage);

    //     return CreatedAtRoute(_getTaskEndpointName, new { id = createdTaskResult.Data!.Id }, createdTaskResult.Data!);
    // }

    //PUT /tasks/id
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDTO updatedTaskDTO)
    {
        Result<string> updatedResult = await _taskService.UpdateTask(id, updatedTaskDTO);

        if (!updatedResult.IsSuccess)
            return StatusCode(updatedResult.ErrorStatusCode, updatedResult.ErrorMessage);

        return NoContent();
    }

    //DELETE /tasks/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        Result<string> deletedResult = await _taskService.RemoveTask(id);

        if (!deletedResult.IsSuccess)
            return StatusCode(deletedResult.ErrorStatusCode, deletedResult.ErrorMessage);

        return NoContent();
    }
}