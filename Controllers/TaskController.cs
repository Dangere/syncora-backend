using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TaskManagementWebAPI.Models.DTOs.Tasks;
using TaskManagementWebAPI.Services;

namespace TaskManagementWebAPI.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController(TaskServices taskServices, IMapper mapper) : ControllerBase
{
    private readonly TaskServices _taskServices = taskServices;
    private readonly IMapper _mapper = mapper;

    private const string _getTaskEndpointName = "GetTask";


    //GET /tasks
    [HttpGet]
    public IActionResult GetTasks()
    {
        return Ok(_taskServices.GetTaskDTOs());
    }


    //GET /tasks/1
    [HttpGet("{id}", Name = _getTaskEndpointName)]
    public IActionResult GetTask(int id)
    {
        return Ok(_taskServices.GetTaskDTO(id));
    }


    //POST /tasks
    [HttpPost]
    public IActionResult PostTask([FromBody] CreateTaskDTO newTask)
    {
        Console.WriteLine("Posting");

        TaskDTO createdTask = _taskServices.CreateTask(newTask);

        return CreatedAtRoute(_getTaskEndpointName, new { id = createdTask.Id }, createdTask);
    }

    //PUT /tasks/id
    [HttpPut("{id}")]
    public IActionResult UpdateTask(int id, [FromBody] UpdateTaskDTO updatedTaskDTO)
    {
        Console.WriteLine("Updating");

        bool updatedTask = _taskServices.UpdateTask(id, updatedTaskDTO);

        if (updatedTask)
            return NoContent();
        else
            return NotFound();

    }



    //DELETE /tasks/1
    [HttpDelete("{id}")]
    public IActionResult DeleteTask(int id)
    {
        Console.WriteLine("Deleting");
        _taskServices.RemoveTask(id);
        return NoContent();

    }





}