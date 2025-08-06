
using System.ComponentModel.DataAnnotations;

namespace SyncoraBackend.Models.DTOs.Auth;

public record TokensDTO([Required] string AccessToken, [Required] string RefreshToken);