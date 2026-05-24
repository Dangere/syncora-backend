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

[AuthorizeRoles(UserRoles.Admin, UserRoles.User)]
[ApiController]
[Route("api/[controller]")]
public class UsersController(UsersService usersService, ImagesService imagesService) : ControllerBase
{
    private readonly UsersService _usersService = usersService;
    private readonly ImagesService _imagesService = imagesService;


    [HttpGet("{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        Result<UserDTO> result = await _usersService.GetUser(username);

        if (!result.IsSuccess)
            return this.ErrorResponse(result);

        return Ok(result.Data);
    }


    [HttpGet("images/generate-signature")]
    public async Task<IActionResult> GetImageUploadSignature()
    {

        Result<UploadSignature> result = await _imagesService.GenerateUploadSignature();

        if (!result.IsSuccess)
            return this.ErrorResponse(result);



        return Ok(result.Data!);
    }

    [HttpPost("images/profile/upload")]
    public async Task<IActionResult> UpdateProfileImage([FromBody] string imageUrl)
    {


        Result<string> result = await _usersService.UpdateUserProfilePicture(imageUrl);

        if (!result.IsSuccess)
            return this.ErrorResponse(result);



        return Ok(result.Data);
    }


    [HttpPost("profile")]
    public async Task<IActionResult> UpdateProfileProfile([FromBody] UpdateUserProfileDTO updateUserProfileDTO)
    {


        Result<string> result = await _usersService.UpdateUserProfile(updateUserProfileDTO);

        if (!result.IsSuccess)
            return this.ErrorResponse(result);



        return Ok(result.Data);
    }

}