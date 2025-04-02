namespace SyncoraBackend.Models.DTOs.Tasks;


public record class TaskDTO(
    int Id,
    string Title,
    string? Description,
    bool Completed,
    DateTime CreationDate,
    DateTime? LastUpdateDate,
    int GroupId
);