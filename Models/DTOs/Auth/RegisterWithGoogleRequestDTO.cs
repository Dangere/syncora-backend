using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Models.DTOs.Users;

namespace SyncoraBackend.Models.DTOs.Auth;

public record RegisterWithGoogleRequestDTO([Required] string IdToken, [Required] string Password, [Required] string Username, UserPreferencesDTO? UserPreferences);