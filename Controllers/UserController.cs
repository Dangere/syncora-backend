using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoMapper;
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


    [HttpPost("tasks")]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskDTO newTask)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<TaskDTO> createdTaskResult = await _taskService.CreateTask(newTask, userId);


        if (!createdTaskResult.IsSuccess)
            return BadRequest(createdTaskResult.ErrorMessage);

        return Ok(createdTaskResult.Data);
    }
}