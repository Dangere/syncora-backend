namespace TaskManagementWebAPI.Models.DTOs.Tasks;

public record class UpdateTaskDTO(string? NewTitle, string? NewDescription, bool? Completed);
