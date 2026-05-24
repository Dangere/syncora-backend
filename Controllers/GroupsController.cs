using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Models.DTOs.Users;
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

        List<GroupDTO> groups = await _groupService.GetGroups();


        return Ok(groups);
    }

    [HttpGet("{groupId}", Name = _getGroupEndpointName)]
    public async Task<IActionResult> GetGroup(int groupId)
    {

        Result<GroupDTO> fetchResult = await _groupService.GetGroup(groupId);

        if (!fetchResult.IsSuccess)
            return this.ErrorResponse(fetchResult);

        return Ok(fetchResult.Data);
    }

    [HttpPost()]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupDTO createGroupDTO)
    {

        Result<GroupDTO> createResult = await _groupService.CreateGroup(createGroupDTO);

        if (!createResult.IsSuccess)
            return this.ErrorResponse(createResult);

        return CreatedAtRoute(_getGroupEndpointName, new { groupId = createResult.Data!.Id }, createResult.Data);
    }

    [HttpPut("{groupId}")]
    public async Task<IActionResult> UpdateGroup([FromBody] UpdateGroupDTO updateGroupDTO, int groupId)
    {

        Result<string> updateResult = await _groupService.UpdateGroup(updateGroupDTO, groupId);

        if (!updateResult.IsSuccess)
            return this.ErrorResponse(updateResult);

        return NoContent();
    }

    [HttpDelete("{groupId}")]
    public async Task<IActionResult> DeleteGroup(int groupId)
    {

        Result<string> deleteResult = await _groupService.DeleteGroup(groupId);

        if (!deleteResult.IsSuccess)
            return this.ErrorResponse(deleteResult);

        return NoContent();
    }

    [HttpPost("{groupId}/grant-access")]
    public async Task<IActionResult> GrantAccessToGroup(int groupId, List<string> usernames)
    {

        Result<List<UserDTO>> grantResult = await _groupService.GrantAccessToGroup(groupId, usernames);

        if (!grantResult.IsSuccess)
            return this.ErrorResponse(grantResult);

        return Ok(grantResult.Data);
    }

    [HttpPost("{groupId}/revoke-access")]
    public async Task<IActionResult> RevokeAccessToGroup(int groupId, List<string> usernames)
    {

        Result<string> revokeResult = await _groupService.RevokeAccessToGroup(groupId, usernames);

        if (!revokeResult.IsSuccess)
            return this.ErrorResponse(revokeResult);

        return Ok(revokeResult.Data);

    }



    [HttpPost("{groupId}/leave")]
    public async Task<IActionResult> LeaveGroup(int groupId)
    {

        Result<string> revokeResult = await _groupService.LeaveGroup(groupId);

        if (!revokeResult.IsSuccess)
            return this.ErrorResponse(revokeResult);

        return NoContent();
    }
}