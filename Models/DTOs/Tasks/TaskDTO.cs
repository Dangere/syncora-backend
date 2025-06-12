namespace SyncoraBackend.Models.DTOs.Tasks;


public record class TaskDTO(
    int Id,
    string Title,
    string? Description,
    bool Completed,
    int? CompletedById,
    DateTime CreationDate,
    DateTime? LastModifiedDate,
    int GroupId
);