using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.Common;
using SyncoraBackend.Models.DTOs.Users;
using SyncoraBackend.Services;
using SyncoraBackend.Utilities;


namespace SyncoraBackend.Controllers;

[AuthorizeRoles(UserRole.Admin, UserRole.User)]
[ApiController]
[Route("api/[controller]")]
public class UsersController(UsersService usersService, ImagesService imagesService) : ControllerBase
{
    private readonly UsersService _usersService = usersService;
    private readonly ImagesService _imagesService = imagesService;

    [HttpGet("images/generate-signature")]
    public async Task<IActionResult> GetImageUploadSignature()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<UploadSignature> result = await _imagesService.GenerateUploadSignature(userId);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorStatusCode, result.ErrorMessage);


        return Ok(result.Data!);
    }

    [HttpPost("images/profile/upload")]
    public async Task<IActionResult> UpdateProfileImage([FromBody] string imageUrl)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> result = await _usersService.UpdateUserProfilePicture(userId, imageUrl);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorStatusCode, result.ErrorMessage);


        return Ok(result.Data!);
    }


    [HttpPost("profile")]
    public async Task<IActionResult> UpdateProfileProfile([FromBody] UpdateUserProfileDTO updateUserProfileDTO)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> result = await _usersService.UpdateUserProfile(updateUserProfileDTO, userId);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorStatusCode, result.ErrorMessage);


        return Ok(result.Data!);
    }

}