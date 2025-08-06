using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SyncoraBackend.Models.Entities;

[Table("refresh_tokens", Schema = "public"), Index(nameof(UserId), nameof(RefreshToken))]
public class RefreshTokenEntity()
{
    public int Id { get; set; }

    [Required]
    public required int UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    [Required]
    public required string RefreshToken { get; set; }

    [Required]
    public required DateTime ExpiresAt { get; set; }

    [Required]
    public required bool IsRevoked { get; set; }
};