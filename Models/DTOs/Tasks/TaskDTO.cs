namespace SyncoraBackend.Models.DTOs.Tasks;


public record class TaskDTO(
    int Id,
    string Title,
    string? Description,
    int? CompletedById,
    int[] AssignedTo,
    DateTime CreationDate,
    DateTime? LastModifiedDate,
    int GroupId
);