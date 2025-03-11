namespace TaskManagementWebAPI.Models.Entities;


public record class TaskEntity(int Id, int UserId, string Title, string Description, bool Completed, DateTime CreatedAt, DateTime? UpdatedAt);