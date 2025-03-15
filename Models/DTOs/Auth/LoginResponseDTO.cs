using System.ComponentModel.DataAnnotations;
using TaskManagementWebAPI.Models.DTOs.Users;

namespace TaskManagementWebAPI.Models.DTOs.Auth;
public record LoginResponseDTO([Required] string AccessToken, [Required] UserDTO UserData);