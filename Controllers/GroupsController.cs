using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Services;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Controllers;

[AuthorizeRoles(UserRole.Admin, UserRole.User)]
[ApiController]
[Route("api/[controller]")]
public class GroupsController(GroupsService groupService) : ControllerBase
{
    private readonly GroupsService _groupService = groupService;
    private const string _getGroupEndpointName = "GetGroup";

    [HttpGet()]
    public async Task<IActionResult> GetGroups()
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        List<GroupDTO> groups = await _groupService.GetGroups(userId);


        return Ok(groups);
    }

    [HttpGet("{groupId}", Name = _getGroupEndpointName)]
    public async Task<IActionResult> GetGroup(int groupId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<GroupDTO> fetchResult = await _groupService.GetGroup(userId, groupId);

        if (!fetchResult.IsSuccess)
            return StatusCode(fetchResult.ErrorStatusCode, fetchResult.ErrorMessage);

        return Ok(fetchResult.Data);
    }

    [HttpPost()]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDTO createGroupDTO)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<GroupDTO> createResult = await _groupService.CreateGroup(createGroupDTO, userId);

        if (!createResult.IsSuccess)
            return StatusCode(createResult.ErrorStatusCode, createResult.ErrorMessage);

        return CreatedAtRoute(_getGroupEndpointName, new { groupId = createResult.Data!.Id }, createResult.Data);
    }

    [HttpPut("{groupId}")]
    public async Task<IActionResult> UpdateGroup([FromBody] UpdateGroupDTO updateGroupDTO, int groupId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> updateResult = await _groupService.UpdateGroup(updateGroupDTO, userId, groupId);

        if (!updateResult.IsSuccess)
            return StatusCode(updateResult.ErrorStatusCode, updateResult.ErrorMessage);

        return NoContent();
    }

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> DeleteGroup(int groupId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> deleteResult = await _groupService.DeleteGroup(userId, groupId);

        if (!deleteResult.IsSuccess)
            return StatusCode(deleteResult.ErrorStatusCode, deleteResult.ErrorMessage);

        return NoContent();
    }

    [HttpPost("{groupId}/grant-access/{userName}")]
    public async Task<IActionResult> GrantAccessToGroup(int groupId, string userName)
    {
        int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> grantResult = await _groupService.AllowAccessToGroup(groupId, currentUserId, userName, true);

        if (!grantResult.IsSuccess)
            return StatusCode(grantResult.ErrorStatusCode, grantResult.ErrorMessage);

        return NoContent();
    }

    [HttpPost("{groupId}/revoke-access/{userName}")]
    public async Task<IActionResult> RevokeAccessToGroup(int groupId, string userName)
    {
        int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> revokeResult = await _groupService.AllowAccessToGroup(groupId, currentUserId, userName, false);

        if (!revokeResult.IsSuccess)
            return StatusCode(revokeResult.ErrorStatusCode, revokeResult.ErrorMessage);

        return NoContent();
    }
}