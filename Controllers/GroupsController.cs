using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Services;
using SyncoraBackend.Utilities;

namespace SyncoraBackend.Controllers;

[AuthorizeRoles(UserRoles.Admin, UserRoles.User)]
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
            return this.ErrorResponse(fetchResult);

        return Ok(fetchResult.Data);
    }

    [HttpPost()]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDTO createGroupDTO)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<GroupDTO> createResult = await _groupService.CreateGroup(createGroupDTO, userId);

        if (!createResult.IsSuccess)
            return this.ErrorResponse(createResult);

        return CreatedAtRoute(_getGroupEndpointName, new { groupId = createResult.Data!.Id }, createResult.Data);
    }

    [HttpPut("{groupId}")]
    public async Task<IActionResult> UpdateGroup([FromBody] UpdateGroupDTO updateGroupDTO, int groupId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> updateResult = await _groupService.UpdateGroup(updateGroupDTO, userId, groupId);

        if (!updateResult.IsSuccess)
            return this.ErrorResponse(updateResult);

        return NoContent();
    }

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> DeleteGroup(int groupId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> deleteResult = await _groupService.DeleteGroup(userId, groupId);

        if (!deleteResult.IsSuccess)
            return this.ErrorResponse(deleteResult);

        return NoContent();
    }

    [HttpPost("{groupId}/grant-access")]
    public async Task<IActionResult> GrantAccessToGroup(int groupId, List<string> usernames)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> grantResult = await _groupService.GrantAccessToGroup(groupId, userId, usernames);

        if (!grantResult.IsSuccess)
            return this.ErrorResponse(grantResult);

        return Ok(grantResult.Data);
    }

    [HttpPost("{groupId}/revoke-access")]
    public async Task<IActionResult> RevokeAccessToGroup(int groupId, List<string> usernames)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> revokeResult = await _groupService.RevokeAccessToGroup(groupId, userId, usernames);

        if (!revokeResult.IsSuccess)
            return this.ErrorResponse(revokeResult);

        return Ok(revokeResult.Data);

    }



    [HttpPost("{groupId}/leave")]
    public async Task<IActionResult> LeaveGroup(int groupId)
    {
        int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        Result<string> revokeResult = await _groupService.LeaveGroup(groupId, userId);

        if (!revokeResult.IsSuccess)
            return this.ErrorResponse(revokeResult);

        return NoContent();
    }
}