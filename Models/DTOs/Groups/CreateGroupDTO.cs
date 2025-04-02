using System.ComponentModel.DataAnnotations;

namespace SyncoraBackend.Models.DTOs.Groups;

public record CreateGroupDTO([Required] string Title, string? Description);