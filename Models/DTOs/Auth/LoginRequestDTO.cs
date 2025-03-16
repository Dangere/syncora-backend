using System.ComponentModel.DataAnnotations;

namespace TaskManagementWebAPI.Models.DTOs.Auth;

public record LoginRequestDTO([Required, EmailAddress] string Email, [Required] string Password);