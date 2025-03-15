using System.ComponentModel.DataAnnotations;

namespace TaskManagementWebAPI.Models.DTOs.Auth;

public record RegisterRequestDTO([Required] string Email, [Required] string Password, [Required] string UserName);