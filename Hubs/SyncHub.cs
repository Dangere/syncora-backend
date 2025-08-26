using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
using SyncoraBackend.Models.DTOs.Groups;
using SyncoraBackend.Services;
namespace SyncoraBackend.Hubs;

[AuthorizeRoles(UserRole.User, UserRole.Admin)]
public class SyncHub(GroupsService groupService) : Hub
{
    private readonly GroupsService _groupService = groupService;

    public async Task SendSyncPayload(int groupId, Dictionary<string, object> payload)
    {
        await Clients.Groups($"group-{groupId}").SendAsync("ReceiveSync", payload);
    }

    public override async Task OnConnectedAsync()
    {
        int UserId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        Console.WriteLine($"A client has connected, UserId: {UserId}");

        List<GroupDTO> groups = await _groupService.GetGroups(UserId);

        foreach (GroupDTO group in groups)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{group.Id}");
        }

        await base.OnConnectedAsync();
    }

}