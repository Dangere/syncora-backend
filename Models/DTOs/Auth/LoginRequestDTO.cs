using System.ComponentModel.DataAnnotations;

namespace SyncoraBackend.Models.DTOs.Auth;

public record LoginRequestDTO([Required, EmailAddress] string Email, [Required] string Password);