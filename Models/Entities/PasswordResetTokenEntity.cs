using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SyncoraBackend.Models.Entities;

[Table("password_rest_tokens", Schema = "public"), Index(nameof(HashedToken), nameof(IsConsumed), nameof(ExpiresAt), IsUnique = true)]
public class PasswordResetTokenEntity()
{
    public int Id { get; set; }

    [Required]
    public required int UserId { get; set; }

    public UserEntity User { get; set; } = null!;

    [Required]
    public required string HashedToken { get; set; }

    [Required]
    public required DateTime ExpiresAt { get; set; }

    [Required]
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    [Required]
    public required bool IsConsumed { get; set; }


    public static PasswordResetTokenEntity CreateToken(int userId, string hashedToken, int expiryMinutes)
    => new() { UserId = userId, HashedToken = hashedToken, ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes), IsConsumed = false };

};