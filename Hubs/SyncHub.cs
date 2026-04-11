using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Services;
namespace SyncoraBackend.Hubs;

[AuthorizeRoles(UserRoles.User, UserRoles.Admin)]
public class SyncHub(GroupsService groupService, UsersService usersService, ILogger<SyncHub> logger, InMemoryHubConnectionManager inMemoryConnectionManager) : Hub
{
    private readonly GroupsService _groupService = groupService;
    private readonly UsersService _usersService = usersService;

    private readonly ILogger<SyncHub> _logger = logger;
    private readonly InMemoryHubConnectionManager _inMemoryConnectionManager = inMemoryConnectionManager;


    public override async Task OnConnectedAsync()
    {
        int userId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        _logger.LogInformation("A client has connected, UserId: {UserId}", userId);
        _inMemoryConnectionManager.AddConnection(userId, Context.ConnectionId);

        // Get the groups the user is in
        List<GroupDTO> groups = await _groupService.GetGroups(userId);

        // Create / Add the user to the groups
        foreach (GroupDTO group in groups)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{group.Id}");
        }

        // Get the related user ids 
        List<int> relatedUserIds = await _usersService.GetRelatedUserIds(userId);

        // Create / Join the related users groups
        foreach (int relatedUserId in relatedUserIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{relatedUserId}");
        }


        await base.OnConnectedAsync();
    }

}