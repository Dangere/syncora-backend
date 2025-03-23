namespace TaskManagementWebAPI.Models.DTOs.Groups;


public record class GroupDTO(
    int Id,
    string Title,
    string? Description,
    DateTime CreationDate,
    int OwnerId,
    string[] SharedUsers
);