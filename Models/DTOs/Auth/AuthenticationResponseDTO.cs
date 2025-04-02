using System.ComponentModel.DataAnnotations;
using SyncoraBackend.Models.DTOs.Users;

namespace SyncoraBackend.Models.DTOs.Auth;
public record AuthenticationResponseDTO([Required] string AccessToken, [Required] UserDTO UserData);