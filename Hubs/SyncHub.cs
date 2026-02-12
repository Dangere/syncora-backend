using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Services;
namespace SyncoraBackend.Hubs;

[AuthorizeRoles(UserRole.User, UserRole.Admin)]
public class SyncHub(GroupsService groupService, InMemoryHubConnectionManager inMemoryConnectionManager) : Hub
{
    private readonly GroupsService _groupService = groupService;
    private readonly InMemoryHubConnectionManager _inMemoryConnectionManager = inMemoryConnectionManager;


    public override async Task OnConnectedAsync()
    {
        int UserId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        Console.WriteLine($"A client has connected, UserId: {UserId}");
        _inMemoryConnectionManager.AddConnection(UserId, Context.ConnectionId);

        List<GroupDTO> groups = await _groupService.GetGroups(UserId);

        foreach (GroupDTO group in groups)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{group.Id}");
        }
        Console.WriteLine("generated connection id is " + Context.ConnectionId);

        await base.OnConnectedAsync();
    }

}