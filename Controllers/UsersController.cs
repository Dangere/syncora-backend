using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Services;


namespace SyncoraBackend.Controllers;

[AuthorizeRoles(UserRole.Admin, UserRole.User)]
[ApiController]
[Route("api/[controller]")]
public class UsersController(TasksService taskService) : ControllerBase
{
    private readonly TasksService _taskService = taskService;
    private const string _getTaskEndpointName = "GetTaskForUser";



}