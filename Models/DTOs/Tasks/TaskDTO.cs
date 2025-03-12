namespace TaskManagementWebAPI.Models.DTOs.Tasks;


public record class TaskDTO(int Id, string Title, string? Description, bool Completed, DateTime CreatedAt, DateTime? UpdatedAt);