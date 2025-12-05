using SyncoraBackend.Utilities;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Models.DTOs.Auth;
using SyncoraBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;


namespace SyncoraBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
[AuthorizeRoles(UserRole.Admin)]
public class AdminController(AdminServices adminService) : ControllerBase
{

    private readonly AdminServices _adminService = adminService;



    [HttpPost("modify-user/{username}/new-password/{newPassword}")]
    public async Task<IActionResult> UpdateUserPassword(string username, string newPassword)
    {
        Result<string> result = await _adminService.UpdateUserPassword(username, newPassword);

        if (result.IsSuccess)
        {
            return Ok(result.Data);
        }
        return StatusCode(result.ErrorStatusCode, result.ErrorMessage);

    }

}
