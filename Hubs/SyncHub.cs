using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Services;
namespace SyncoraBackend.Hubs;

[AuthorizeRoles(UserRoles.User, UserRoles.Admin)]
public class SyncHub(GroupsService groupService, UsersService usersService, ILogger<SyncHub> logger, InMemoryHubConnectionManager inMemoryConnectionManager, UserRequestContext userRequestContext) : Hub
{
    private readonly GroupsService _groupService = groupService;
    private readonly UsersService _usersService = usersService;

    private readonly ILogger<SyncHub> _logger = logger;
    private readonly InMemoryHubConnectionManager _inMemoryConnectionManager = inMemoryConnectionManager;

    private readonly UserRequestContext _userRequestContext = userRequestContext;


    public override async Task OnConnectedAsync()
    {
        // Get the device id to be used later to exclude sending events to the same device firing them
        var deviceId = Context.GetHttpContext()?.Request.Query["deviceId"].ToString();
        int userId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        _logger.LogInformation("A client has connected, UserId: {UserId} DeviceId: {DeviceId}", userId, deviceId);
        _inMemoryConnectionManager.AddConnection(userId, Context.ConnectionId, deviceId ?? userId.ToString());

        _userRequestContext.PopulateContext(userId, deviceId!);

        // Get the groups the user is in
        List<int> groupIds = await _groupService.GetGroupIds();

        // Create / Add the user to the groups
        foreach (int id in groupIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{id}");
        }

        // // Get the related user ids 
        // List<int> relatedUserIds = await _usersService.GetRelatedUserIds(userId);

        // // Create / Join the related users groups
        // foreach (int relatedUserId in relatedUserIds)
        // {
        //     await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{relatedUserId}");
        // }


        await base.OnConnectedAsync();
    }

}