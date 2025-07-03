using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using SyncoraBackend.Attributes;
using SyncoraBackend.Enums;
namespace SyncoraBackend.Hubs;

[AuthorizeRoles(UserRole.User, UserRole.Admin)]
public class NotificationHub() : Hub
{

    public async Task NotifyGroupMembers(int groupId, string message)
    {
        await Clients.Groups($"group-{groupId}").SendAsync("Notify", message);
    }

    public async Task NotifyAllUsers(string message)
    {
        Console.WriteLine(message);
        await Clients.Groups("SignalR Users").SendAsync("Notify", message);
    }

    public override async Task OnConnectedAsync()
    {
        int UserId = int.Parse(Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        Console.WriteLine($"A client has connected, UserId: {UserId}");

        // List<GroupDTO> groups = await _groupService.GetGroups(UserId);

        // foreach (GroupDTO group in groups)
        // {
        //     await Groups.AddToGroupAsync(Context.ConnectionId, $"group-{group.Id}");
        // }

        await Groups.AddToGroupAsync(Context.ConnectionId, "SignalR Users");
        await base.OnConnectedAsync();
    }

}