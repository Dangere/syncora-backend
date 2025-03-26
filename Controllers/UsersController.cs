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



}