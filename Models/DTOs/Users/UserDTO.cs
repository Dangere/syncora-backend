using TaskManagementWebAPI.Enums;

namespace TaskManagementWebAPI.Models.DTOs.Users;

public record UserDTO(int Id, string Email, string UserName, UserRole Role, DateTime CreationDate, string? ProfilePictureURL);