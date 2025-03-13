using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Models.DTOs.Tasks;
using TaskManagementWebAPI.Services;

namespace TaskManagementWebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController(TaskService taskServices) : ControllerBase
{
    private readonly TaskService _taskServices = taskServices;

    private const string _getTaskEndpointName = "GetTask";


    //GET /tasks
    [HttpGet]
    public async Task<IActionResult> GetTasks()
    {
        return Ok(await _taskServices.GetTaskDTOs());
    }


    //GET /tasks/1
    [HttpGet("{id}", Name = _getTaskEndpointName)]
    public async Task<IActionResult> GetTask(int id)
    {
        TaskDTO? task = await _taskServices.GetTaskDTO(id);

        if (task == null)
            return NotFound();
        else
            return Ok(task);
    }


    //POST /tasks
    [HttpPost]
    public async Task<IActionResult> PostTask([FromBody] CreateTaskDTO newTask)
    {
        TaskDTO createdTask = await _taskServices.CreateTask(newTask);

        return CreatedAtRoute(_getTaskEndpointName, new { id = createdTask.Id }, createdTask);
    }

    //PUT /tasks/id
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskDTO updatedTaskDTO)
    {

        bool updatedTask = await _taskServices.UpdateTaskAsync(id, updatedTaskDTO);

        if (updatedTask)
            return NoContent();
        else
            return NotFound();

    }



    //DELETE /tasks/1
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        bool deleted = await _taskServices.RemoveTask(id);

        if (deleted)
            return NoContent();
        else
            return NotFound();


    }
}