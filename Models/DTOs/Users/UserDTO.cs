using SyncoraBackend.Enums;

namespace SyncoraBackend.Models.DTOs.Users;

public record UserDTO(int Id, string Email, string Username, string Role, DateTime CreationDate, DateTime LastModifiedDate, string? ProfilePictureURL);