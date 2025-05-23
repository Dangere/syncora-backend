namespace SyncoraBackend.Models.DTOs.Groups;

//Need to return some information about the tasks inside it
public record class GroupDTO(
    int Id,
    string Title,
    string? Description,
    DateTime CreationDate,
    DateTime LastModifiedDate,
    int OwnerUserId,
    int[] GroupMembers
);