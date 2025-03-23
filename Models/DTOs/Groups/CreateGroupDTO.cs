using System.ComponentModel.DataAnnotations;

namespace TaskManagementWebAPI.Models.DTOs.Groups;

public record CreateGroupDTO([Required] string Title, string? Description);