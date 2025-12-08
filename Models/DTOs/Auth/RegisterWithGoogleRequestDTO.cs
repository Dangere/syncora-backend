using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Models.Common;

namespace SyncoraBackend.Models.DTOs.Auth;

public record RegisterWithGoogleRequestDTO([Required] string IdToken, [Required] string Password, [Required] string Username, UserPreferences? UserPreferences);