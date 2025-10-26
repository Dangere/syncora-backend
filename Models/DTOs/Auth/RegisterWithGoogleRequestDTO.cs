using System.ComponentModel.DataAnnotations;

namespace SyncoraBackend.Models.DTOs.Auth;

public record RegisterWithGoogleRequestDTO([Required] string IdToken, [Required] string Password, [Required] string Username);