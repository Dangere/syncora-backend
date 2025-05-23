using System.ComponentModel.DataAnnotations;

namespace SyncoraBackend.Models.DTOs.Auth;

public record RegisterRequestDTO([Required, EmailAddress] string Email, [Required] string Password, [Required] string Username);