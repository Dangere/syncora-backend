using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Models.Common;

namespace SyncoraBackend.Models.DTOs.Auth;

public record RegisterRequestDTO([Required, EmailAddress] string Email, [Required] string Password, [Required] string Username, UserPreferences? UserPreferences);