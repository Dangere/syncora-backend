namespace TaskManagementWebAPI.Models.DTOs.Tasks;

public record class CreateTaskDTO(string Title, string Description, int OwnerId);