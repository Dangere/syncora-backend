using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Models.DTOs.Users;

namespace SyncoraBackend.Models.DTOs.Auth;

public record RegisterRequestDTO([Required, EmailAddress] string Email, [Required] string Password, [Required] string Username, [Required, StringLength(20, MinimumLength = 3, ErrorMessage = "First Name must be between 3 and 20 characters")] string FirstName, [Required, StringLength(20, MinimumLength = 3, ErrorMessage = "Last Name must be between 3 and 20 characters")] string LastName, UserPreferencesDTO? UserPreferences);